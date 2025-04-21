using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading.Tasks;
using MODSI_SQLRestAPI.Functions;
using Microsoft.Extensions.Logging;

using System.IO;
using MODSI_SQLRestAPI.UserAuth.Services;
using System.Net;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;
using UserAuthenticate.Repositories;
using MODSI_SQLRestAPI.UserAuth.DTO;
using MODSI_SQLRestAPI.UserAuth.Models.User;


// Apenas resposável por fazer o CRUD de Users e verificar se autenticação é válida (token) para o pedido efetuado

namespace MODSI_SQLRestAPI.UserAuth.Controllers
{
    class UserController
    {
        //Temporary for testing
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
        private readonly UserRepository _databaseHandler;
        private readonly RetriveToken retriveToken;

        //Service
        private readonly UserService _userService;

        public UserController(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<UserController>();
            _databaseHandler = new UserRepository();
            _userService = new UserService(loggerFactory);
            retriveToken = new RetriveToken();
        }


        // First Organized function as example

        [Function("GetAllUsers")]
        public async Task<HttpResponseData> GetAllUsers([HttpTrigger(AuthorizationLevel.Function, "get", Route = "User/GetAll")] HttpRequestData req)
        {
            try
            {
                var retriveToken = new RetriveToken();
                var principal = retriveToken.GetPrincipalFromRequest(req);

                if (principal == null || !principal.Identity.IsAuthenticated)
                {
                    var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                    unauthorizedResponse.WriteString("Unauthorized");
                    return unauthorizedResponse;
                }


                if (!principal.IsInRole("manager") && !principal.IsInRole("admin"))
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


        [Function("GetUserById")]
        public async Task<HttpResponseData> GetUserById([HttpTrigger(AuthorizationLevel.Function, "get", Route = "User/Get/{id:int}")] HttpRequestData req, int id)
        {
            try
            {
                _logger.LogInformation($"Retrieving user with Id: {id}");
                var user = await _databaseHandler.GetUserByIdAsync(id);

                if (user == null)
                {
                    return req.CreateResponse(HttpStatusCode.NotFound);
                }
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(JsonSerializer.Serialize(user));
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving user with Id {id}.");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

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

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

                if (string.IsNullOrWhiteSpace(user.Password)) 
                { await response.WriteStringAsync("A Password is required.");
                  return response;
                }
                

                // Verifica se o salt foi fornecido
                if (string.IsNullOrEmpty(user.Salt))
                {
                    // Caso o salt não tenha sido fornecido, gera um novo salt e realiza o hashing da senha
                    salt = Utils.GenerateSalt();
                    passwordHash = Utils.HashPassword(user.Password, salt);
                }
                else
                {
                    // Caso o salt tenha sido fornecido, utiliza o salt e a senha como estão
                    salt = user.Salt;
                    passwordHash = user.Password;
                }

                // Map MODSI_SQLRestAPI.User to MODSI_SQLRestAPI.DatabaseHandler.User
                var dbUser = new User
                {
                    Name = user.Name,
                    Email = user.Email,
                    Password = passwordHash,
                    Username = user.Username,
                    Role = user.Role ?? "User",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    Group = user.Group ?? "USER",
                    Salt = salt
                };

                await _databaseHandler.AddUserAsync(dbUser);

                await response.WriteStringAsync("User added successfully.");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding user.");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }


        [Function("DeleteUserById")]
        public async Task<HttpResponseData> DeleteUserById([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "User/Delete/{id:int}")] HttpRequestData req, int id)
        {
            try
            {
                _logger.LogInformation($"Deleting user with Id: {id}");
                await _databaseHandler.DeleteUserByIdAsync(id);
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
                await response.WriteStringAsync($"User with Id {id} deleted successfully.");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while deleting user with Id {id}.");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        [Function("UpdateUserById")]
        public async Task<HttpResponseData> UpdateUserById([HttpTrigger(AuthorizationLevel.Function, "put", Route = "User/Update/{id:int}")] HttpRequestData req, int id)
        {
            try
            {
                _logger.LogInformation($"Updating user with Id: {id}");
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var user = JsonSerializer.Deserialize<User>(requestBody);
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
                    Group = user.Group
                };

                await _databaseHandler.UpdateUserByIdAsync(dbUser);

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
                await response.WriteStringAsync($"User with Id {id} updated successfully.");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating user with Id {id}.");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
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
                var exists = await _databaseHandler.EmailUserExistsAsync(email);
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(JsonSerializer.Serialize(new { Exists = exists }));
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while checking if email exists");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
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
                var user = await _databaseHandler.GetUserByIdentifierAsync(email);

                if (user == null)
                {
                    return req.CreateResponse(HttpStatusCode.NotFound);
                }

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(JsonSerializer.Serialize(user));
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving user by email.");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
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
                var user = await _databaseHandler.GetUserByIdentifierAsync(username);

                if (user == null)
                {
                    return req.CreateResponse(HttpStatusCode.NotFound);
                }

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(JsonSerializer.Serialize(user));
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving user by email.");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }


        // MUDADO para não tratar do token visto que o token só depende do login e não do registo!
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
        }





    }
}
