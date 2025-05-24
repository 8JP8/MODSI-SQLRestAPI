using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using MODSI_SQLRestAPI.UserAuth.Models;
using MODSI_SQLRestAPI.UserAuth.Repositories;
using MODSI_SQLRestAPI.UserAuth.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

// Apenas resposável por fazer o CRUD de Users e verificar se autenticação é válida (token) para o pedido efetuado

namespace MODSI_SQLRestAPI.UserAuth.Controllers
{
    class UserController
    {

        private readonly ILogger _logger;
        private readonly UserService _userService;
        public UserController(ILoggerFactory loggerFactory, UserService userService)
        {
            _logger = loggerFactory.CreateLogger<UserController>();
            _userService = userService;
            Task.Run(() => InitializeGroupsAsync()).Wait();
        }

        public async Task InitializeGroupsAsync()
        {
            var groupsRepository = new GroupsRepository();
            var predefinedGroups = new List<Groups>
            {
                new Groups(1, "ADMIN"),
                new Groups(2, "USER"),
                new Groups(3, "GUEST")
            };

            await groupsRepository.EnsureGroupsExistAsync(predefinedGroups);
        }

        // Check if user exists by username

        [Function("UserExistsByUsername")]
        public async Task<HttpResponseData> UserExistsByUsername(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "User/ExistsByUsername")] HttpRequestData req)
        {
            try
            {
                string username = req.Query["username"];
                if (string.IsNullOrEmpty(username))
                {
                    return req.CreateResponse(HttpStatusCode.BadRequest);
                }
                _logger.LogInformation($"Checking if user exists by username: {username}");
                var exists = await _userService.UserExistsByUsername(username);
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(JsonSerializer.Serialize(new { Exists = exists }));
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while checking if user exists.");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }


        [Function("GetAllUsers")]
        public async Task<HttpResponseData> GetAllUsers([HttpTrigger(AuthorizationLevel.Function, "get", Route = "User/GetAll")] HttpRequestData req)
        {
            try
            {
                var retrieveToken = new RetrieveToken();
                var principal = retrieveToken.GetPrincipalFromRequest(req);
                _logger.LogInformation("Retrieving all users.", retrieveToken);

                try { _logger.LogInformation(principal.Identity.IsAuthenticated ? "User successfully authenticated." : ""); } catch (Exception ex) { _logger.LogInformation("Erro de Token: " + ex.Message); }

                if (principal == null || !principal.Identity.IsAuthenticated)
                {
                    var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Forbidden);
                    unauthorizedResponse.WriteString("Unauthorized: Invalid auth token.");
                    return unauthorizedResponse;
                }


                if (!principal.IsInGroup("ADMIN"))
                {
                    var forbiddenResponse = req.CreateResponse(HttpStatusCode.Forbidden);
                    forbiddenResponse.WriteString("Forbidden");
                    return forbiddenResponse;
                }

                var users = await _userService.GetAllUsers();



                _logger.LogInformation($"Retrieved users: {JsonSerializer.Serialize(users)}");
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(JsonSerializer.Serialize(users));
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving users.");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }



        // Second organized function as example with exeption handling
        [Function("GetUserById")]
        public async Task<HttpResponseData> GetUserById(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "User/Get/{id:int}")] HttpRequestData req, int id)
        {
            try
            {
                var user = await _userService.GetUserById(id);
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(JsonSerializer.Serialize(user));
                return response;
            }
            catch (HttpException ex)
            {
                _logger.LogWarning(ex, ex.Message);
                var response = req.CreateResponse(ex.StatusCode);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(JsonSerializer.Serialize(new { error = ex.Message }));
                return response;
            }
        }



        //Kinda organized put more exceptions

        [Function("AddUser")]
        public async Task<HttpResponseData> AddUser([HttpTrigger(AuthorizationLevel.Function, "post", Route = "User/Add")] HttpRequestData req)
        {
            try
            {
                _logger.LogInformation("Adding new user.");
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var user = JsonSerializer.Deserialize<User>(requestBody);

                string salt;
                string passwordHash;

                if (string.IsNullOrWhiteSpace(user.Password))
                    throw new BadRequestException($"Password cannot be empty");

                // Verifica se o salt foi fornecido
                if (string.IsNullOrEmpty(user.Salt))
                {
                    salt = Utils.GenerateSalt();
                    passwordHash = Utils.HashPassword(user.Password, salt);
                }
                else
                {
                    salt = user.Salt;
                    passwordHash = user.Password;
                }

                // Map
                // to MODSI_SQLRestAPI.DatabaseHandler.User
                var dbUser = new User(
                    name: user.Name,
                    email: user.Email,
                    password: passwordHash,
                    username: user.Username,
                    role: user.Role ?? "User",
                    group: user.Group ?? "USER",
                    salt: salt,
                    tel: user.Tel,
                    photo: user.Photo ?? new string(' ', 0)
                );

                var userDTO = await _userService.CreateUser(dbUser);
                var response = req.CreateResponse(HttpStatusCode.Created);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(JsonSerializer.Serialize(userDTO, new JsonSerializerOptions { WriteIndented = true }));
                return response;
            }
            catch (HttpException ex)
            {
                _logger.LogWarning(ex, ex.Message);
                var response = req.CreateResponse(ex.StatusCode);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(JsonSerializer.Serialize(new { error = ex.Message }));
                return response;
            }
        }


        [Function("DeleteUserById")]
        public async Task<HttpResponseData> DeleteUserById([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "User/Delete/{id:int}")] HttpRequestData req, int id)
        {
            try
            {
                var retrieveToken = new RetrieveToken();
                var principal = retrieveToken.GetPrincipalFromRequest(req);


                if (principal == null || !principal.Identity.IsAuthenticated || !principal.IsInGroup("ADMIN"))
                {
                    var forbiddenResponse = req.CreateResponse(HttpStatusCode.Forbidden);
                    await forbiddenResponse.WriteStringAsync("Unauthorized or insufficient permissions.");
                    return forbiddenResponse;
                }

                _logger.LogInformation($"Deleting user with Id: {id}");
                await _userService.DeleteUser(id);
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
                await response.WriteStringAsync($"User with Id {id} deleted successfully.");
                return response;
            }
            catch (HttpException ex)
            {
                _logger.LogWarning(ex, ex.Message);
                var response = req.CreateResponse(ex.StatusCode);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(JsonSerializer.Serialize(new { error = ex.Message }));
                return response;
            }
        }


        [Function("UpdateUserById")]
        public async Task<HttpResponseData> UpdateUserById([HttpTrigger(AuthorizationLevel.Function, "put", Route = "User/UpdateById/{id:int}")] HttpRequestData req, int id)
        {
            try
            {

                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                var user = JsonSerializer.Deserialize<User>(requestBody);
                if (user == null)
                {
                    var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequest.WriteStringAsync("Request body is invalid.");
                    return badRequest;
                }

                // Verifica se está autenticado e se tem permissões de admin
                var retriveToken = new RetrieveToken();
                var principal = retriveToken.GetPrincipalFromRequest(req);

                if (principal == null || !principal.Identity.IsAuthenticated || !principal.IsInGroup("ADMIN"))
                {
                    var forbiddenResponse = req.CreateResponse(HttpStatusCode.Forbidden);
                    await forbiddenResponse.WriteStringAsync("Unauthorized or insufficient permissions.");
                    return forbiddenResponse;
                }

                var existingUser = await _userService.GetUserById(id);
                if (existingUser == null)
                {
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFoundResponse.WriteStringAsync($"User with id {id} not found.");
                    return notFoundResponse;
                }

                user.Id = id;

                // Map MODSI_SQLRestAPI.User to MODSI_SQLRestAPI.DatabaseHandler.User
                var dbUser = new User
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Password = user.Password,
                    Username = user.Username,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt,
                    IsActive = user.IsActive,
                    Group = user.Group,
                    Tel = user.Tel, // Pode ser null
                    Photo = user.Photo // Pode ser null
                };

                await _userService.UpdateUser(id, dbUser);

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
                await response.WriteStringAsync($"User with Id {id} updated successfully.");
                return response;
            }
            catch (HttpException ex)
            {
                _logger.LogWarning(ex, ex.Message);
                var response = req.CreateResponse(ex.StatusCode);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(JsonSerializer.Serialize(new { error = ex.Message }));
                return response;
            }
        }


        [Function("UpdateUserByEmail")]
        public async Task<HttpResponseData> UpdateUserByEmail(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "User/Update/{email}")] HttpRequestData req, string email)
        {
            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var user = JsonSerializer.Deserialize<User>(requestBody);
                if (user == null)
                {
                    var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequest.WriteStringAsync("Request body is invalid.");
                    return badRequest;
                }

                var retriveToken = new RetrieveToken();
                var principal = retriveToken.GetPrincipalFromRequest(req);
                bool isAdmin = principal != null && principal.Identity.IsAuthenticated && principal.IsInGroup("ADMIN");

                var existingUser = await _userService.GetUserByIdentifier(email);
                if (existingUser == null)
                {
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFoundResponse.WriteStringAsync($"User with email {email} not found.");
                    return notFoundResponse;
                }

                if (isAdmin)
                {
                    // Admin can update everything
                    existingUser.Role = string.IsNullOrWhiteSpace(user.Role) ? existingUser.Role : user.Role;
                    existingUser.Group = string.IsNullOrWhiteSpace(user.Group) ? existingUser.Group : user.Group;

                    if (!string.IsNullOrWhiteSpace(user.Password))
                    {
                        string newSalt = Utils.GenerateSalt();
                        string hashedPassword = Utils.HashPassword(user.Password, newSalt);
                        existingUser.Password = hashedPassword;
                        existingUser.Salt = newSalt;
                    }
                }

                // Everyone can update these fields
                existingUser.Name = string.IsNullOrWhiteSpace(user.Name) ? existingUser.Name : user.Name;
                existingUser.Tel = string.IsNullOrWhiteSpace(user.Tel) ? existingUser.Tel : user.Tel;
                existingUser.Photo = user.Photo ?? existingUser.Photo;

                // If not admin and trying to update restricted fields, deny
                if (!isAdmin && (!string.IsNullOrWhiteSpace(user.Role) || !string.IsNullOrWhiteSpace(user.Group) || !string.IsNullOrWhiteSpace(user.Password)))
                {
                    var forbiddenResponse = req.CreateResponse(HttpStatusCode.Forbidden);
                    await forbiddenResponse.WriteStringAsync("Unauthorized to update certain fields.");
                    return forbiddenResponse;
                }

                var userDTO = await _userService.UpdateUser(existingUser.Id, existingUser);

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(JsonSerializer.Serialize(userDTO, new JsonSerializerOptions { WriteIndented = true }));
                return response;
            }
            catch (HttpException ex)
            {
                _logger.LogWarning(ex, ex.Message);
                var response = req.CreateResponse(ex.StatusCode);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(JsonSerializer.Serialize(new { error = ex.Message }));
                return response;
            }
        }





        [Function("EmailUserExists")]
        public async Task<HttpResponseData> EmailUserExists(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "User/EmailExists")] HttpRequestData req)
        {
            try
            {
                // Extract email from query string
                string email = req.Query["email"];

                if (string.IsNullOrEmpty(email))
                {
                    return req.CreateResponse(HttpStatusCode.BadRequest);
                }

                _logger.LogInformation($"Checking if email exists: {email}");
                var exists = await _userService.EmailUserExists(email);

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(JsonSerializer.Serialize(new { Exists = exists }));
                return response;
            }
            catch (HttpException ex)
            {
                _logger.LogWarning(ex, ex.Message);
                var response = req.CreateResponse(ex.StatusCode);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(JsonSerializer.Serialize(new { error = ex.Message }));
                return response;
            }
        }




        [Function("GetUserByEmail")]
        public async Task<HttpResponseData> GetUserByEmail(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "User/GetByEmail")] HttpRequestData req)
        {
            try
            {
                // Extract email from query string
                string email = req.Query["email"];

                if (string.IsNullOrEmpty(email))
                {
                    return req.CreateResponse(HttpStatusCode.BadRequest);
                }

                _logger.LogInformation($"Retrieving user with email: {email}");

                var user = await _userService.GetUserByIdentifier(email);

                if (user == null)
                {
                    return req.CreateResponse(HttpStatusCode.NotFound);
                }

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(JsonSerializer.Serialize(user));
                return response;
            }
            catch (HttpException ex)
            {
                _logger.LogWarning(ex, ex.Message);
                var response = req.CreateResponse(ex.StatusCode);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(JsonSerializer.Serialize(new { error = ex.Message }));
                return response;
            }
        }



        [Function("GetUserByUsername")]
        public async Task<HttpResponseData> GetUserByUsername(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "User/GetByUsername")] HttpRequestData req)
        {
            try
            {
                // Extract email from query string
                string username = req.Query["username"];

                if (string.IsNullOrEmpty(username))
                {
                    return req.CreateResponse(HttpStatusCode.BadRequest);
                }

                _logger.LogInformation($"Retrieving user with username: {username}");
                var user = await _userService.GetUserByIdentifier(username);

                if (user == null)
                {
                    return req.CreateResponse(HttpStatusCode.NotFound);
                }

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(JsonSerializer.Serialize(user));
                return response;
            }
            catch (HttpException ex)
            {
                _logger.LogWarning(ex, ex.Message);
                var response = req.CreateResponse(ex.StatusCode);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(JsonSerializer.Serialize(new { error = ex.Message }));
                return response;
            }
        }




        [Function("GetUserSalt")]
        public async Task<HttpResponseData> GetUserSalt(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "User/GetUserSalt")] HttpRequestData req)
        {
            try
            {
                // Extrair username ou email da query string
                var queryParams = Utils.ParseQueryString(req.Url.Query);
                string identifier = !string.IsNullOrWhiteSpace(queryParams["identifier"]) ? queryParams["identifier"] : queryParams["Identifier"];

                if (string.IsNullOrWhiteSpace(identifier))
                {
                    var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequestResponse.WriteStringAsync("Identifier (username or email) is required.");
                    return badRequestResponse;
                }

                _logger.LogInformation($"Retrieving salt for identifier: {identifier}");

                // Buscar o usuário no banco de dados
                var user = await _userService.GetUserByIdentifier(identifier, true);

                if (user == null)
                {
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFoundResponse.WriteStringAsync("User not found.");
                    return notFoundResponse;
                }

                // Retornar o salt
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(JsonSerializer.Serialize(new { user.Salt }));
                return response;
            }
            catch (HttpException ex)
            {
                _logger.LogWarning(ex, ex.Message);
                var response = req.CreateResponse(ex.StatusCode);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(JsonSerializer.Serialize(new { error = ex.Message }));
                return response;
            }
        }

        [Function("ChangeUserRole")]
        public async Task<HttpResponseData> ChangeUserRole(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "User/ChangeRole/{userId:int}")] HttpRequestData req, int userId)
        {
            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var data = JsonSerializer.Deserialize<Dictionary<string, string>>(requestBody);
                if (data == null || !data.ContainsKey("role"))
                {
                    var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequest.WriteStringAsync("Request body must contain 'role' (name or id).");
                    return badRequest;
                }

                var userDTO = await _userService.ChangeUserRole(userId, data["role"]);
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(JsonSerializer.Serialize(userDTO, new JsonSerializerOptions { WriteIndented = true }));
                return response;
            }
            catch (HttpException ex)
            {
                _logger.LogWarning(ex, ex.Message);
                var response = req.CreateResponse(ex.StatusCode);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(JsonSerializer.Serialize(new { error = ex.Message }));
                return response;
            }
        }

        [Function("ChangeUserGroup")]
        public async Task<HttpResponseData> UpdateUserGroup(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "User/ChangeGroup/{userId:int}")] HttpRequestData req, int userId)
        {
            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var data = JsonSerializer.Deserialize<Dictionary<string, string>>(requestBody);
                if (data == null || !data.ContainsKey("group"))
                {
                    var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequest.WriteStringAsync("Request body must contain 'group' (name or id).");
                    return badRequest;
                }

                var userDTO = await _userService.ChangeUserGroup(userId, data["group"]);
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(JsonSerializer.Serialize(userDTO, new JsonSerializerOptions { WriteIndented = true }));
                return response;
            }
            catch (HttpException ex)
            {
                _logger.LogWarning(ex, ex.Message);
                var response = req.CreateResponse(ex.StatusCode);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(JsonSerializer.Serialize(new { error = ex.Message }));
                return response;
            }
        }

        internal static class Utils
        {
            /// <summary>
            /// Gera um salt aleatório em Base64.
            /// </summary>
            public static string GenerateSalt()
            {
                return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            }

            /// <summary>
            /// Faz o hash da password combinada com o salt, usando SHA256.
            /// </summary>
            public static string HashPassword(string password, string salt)
            {
                using (var sha256 = SHA256.Create())
                {
                    var combined = Encoding.UTF8.GetBytes(password + salt);
                    var hash = sha256.ComputeHash(combined);
                    return Convert.ToBase64String(hash);
                }
            }

            public static Dictionary<string, string> ParseQueryString(string query)
            {
                var queryParams = new Dictionary<string, string>();
                if (string.IsNullOrEmpty(query)) return queryParams;

                foreach (var pair in query.TrimStart('?').Split('&'))
                {
                    var parts = pair.Split('=');
                    if (parts.Length == 2)
                    {
                        queryParams[Uri.UnescapeDataString(parts[0])] = Uri.UnescapeDataString(parts[1]);
                    }
                }

                return queryParams;
            }
        }



    }
}
