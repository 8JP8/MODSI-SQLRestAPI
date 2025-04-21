using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;



namespace MODSI_SQLRestAPI
{
    public class API
    {
        private readonly ILogger _logger;
        private readonly DatabaseHandler _databaseHandler;

        public API(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<API>();
            _databaseHandler = new DatabaseHandler();
        }

        #region 3D Points Visualization
        [Function("GetAllPoints")]
        public async Task<HttpResponseData> GetAllPoints([HttpTrigger(AuthorizationLevel.Function, "get", Route = "3DPoints/GetAll")] HttpRequestData req)
        {
            try
            {
                _logger.LogInformation("Retrieving all 3D points.");

                var points = await _databaseHandler.GetAllPointsAsync();
                _logger.LogInformation($"Retrieved points: {JsonSerializer.Serialize(points)}");

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(JsonSerializer.Serialize(points));

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving points.");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        [Function("GetPointById")]
        public async Task<HttpResponseData> GetPointById([HttpTrigger(AuthorizationLevel.Function, "get", Route = "3DPoints/Get/{id:int}")] HttpRequestData req, int id)
        {
            try
            {
                _logger.LogInformation($"Retrieving point with Id: {id}");

                var point = await _databaseHandler.GetPointByIdAsync(id);
                if (point == null)
                {
                    return req.CreateResponse(HttpStatusCode.NotFound);
                }

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(JsonSerializer.Serialize(point));

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving point with Id {id}.");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        [Function("AddPoints")]
        public async Task<HttpResponseData> AddPoints([HttpTrigger(AuthorizationLevel.Function, "post", Route = "3DPoints/Add")] HttpRequestData req)
        {
            try
            {
                _logger.LogInformation("Adding new 3D points.");

                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var points = JsonSerializer.Deserialize<List<Point3D>>(requestBody);

                await _databaseHandler.AddPointsAsync(points);

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
                await response.WriteStringAsync("Points added successfully.");

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding points.");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        [Function("SendRandomPoints")]
        public async Task<HttpResponseData> SendRandomPoints([HttpTrigger(AuthorizationLevel.Function, "post", Route = "3DPoints/SendRandom")] HttpRequestData req)
        {
            try
            {
                _logger.LogInformation("Sending random 3D points.");

                var random = new Random();
                var points = new List<Point3D>();

                for (int i = 0; i < 10; i++)
                {
                    points.Add(new Point3D
                    {
                        X = random.Next(-100, 100),
                        Y = random.Next(-100, 100),
                        Z = random.Next(-100, 100)
                    });
                }

                await _databaseHandler.AddPointsAsync(points);

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(JsonSerializer.Serialize(points));

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending random points.");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        [Function("DeletePointById")]
        public async Task<HttpResponseData> DeletePointById([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "3DPoints/Delete/{id:int}")] HttpRequestData req, int id)
        {
            try
            {
                _logger.LogInformation($"Deleting point with Id: {id}");

                await _databaseHandler.DeletePointByIdAsync(id);

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
                await response.WriteStringAsync($"Point with Id {id} deleted successfully.");

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while deleting point with Id {id}.");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        [Function("ReplacePointById")]
        public async Task<HttpResponseData> ReplacePointById([HttpTrigger(AuthorizationLevel.Function, "put", Route = "3DPoints/Replace/{id:int}")] HttpRequestData req, int id)
        {
            try
            {
                _logger.LogInformation($"Replacing point with Id: {id}");

                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var point = JsonSerializer.Deserialize<Point3D>(requestBody);
                point.Id = id;

                await _databaseHandler.ReplacePointByIdAsync(point);

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
                await response.WriteStringAsync($"Point with Id {id} replaced successfully.");

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while replacing point with Id {id}.");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }
        #endregion

        #region PieChart Visualization
        [Function("GetAllPieCharts")]
        public async Task<HttpResponseData> GetAllPieCharts([HttpTrigger(AuthorizationLevel.Function, "get", Route = "PieChart/GetAll")] HttpRequestData req)
        {
            try
            {
                _logger.LogInformation("Retrieving all pie charts.");

                var pieCharts = await _databaseHandler.GetAllPieChartsAsync();
                _logger.LogInformation($"Retrieved pie charts: {JsonSerializer.Serialize(pieCharts)}");

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(JsonSerializer.Serialize(pieCharts));

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving pie charts.");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        [Function("GetPieChartById")]
        public async Task<HttpResponseData> GetPieChartById([HttpTrigger(AuthorizationLevel.Function, "get", Route = "PieChart/Get/{id:int}")] HttpRequestData req, int id)
        {
            try
            {
                _logger.LogInformation($"Retrieving pie chart with Id: {id}");

                var pieChart = await _databaseHandler.GetPieChartByIdAsync(id);
                if (pieChart == null)
                {
                    return req.CreateResponse(HttpStatusCode.NotFound);
                }

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(JsonSerializer.Serialize(pieChart));

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving pie chart with Id {id}.");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        [Function("AddPieCharts")]
        public async Task<HttpResponseData> AddPieCharts([HttpTrigger(AuthorizationLevel.Function, "post", Route = "PieChart/Add")] HttpRequestData req)
        {
            try
            {
                _logger.LogInformation("Adding new pie charts.");

                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var pieCharts = JsonSerializer.Deserialize<List<PieChart>>(requestBody);

                await _databaseHandler.AddPieChartsAsync(pieCharts);

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
                await response.WriteStringAsync("Pie charts added successfully.");

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding pie charts.");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        [Function("SendRandomPieCharts")]
        public async Task<HttpResponseData> SendRandomPieCharts([HttpTrigger(AuthorizationLevel.Function, "post", Route = "PieChart/SendRandom")] HttpRequestData req)
        {
            try
            {
                _logger.LogInformation("Sending random pie charts.");

                await _databaseHandler.SetRandomPieChartsAsync();

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
                await response.WriteStringAsync("Random pie charts sent successfully.");

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending random pie charts.");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        [Function("DeletePieChartById")]
        public async Task<HttpResponseData> DeletePieChartById([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "PieChart/Delete/{id:int}")] HttpRequestData req, int id)
        {
            try
            {
                _logger.LogInformation($"Deleting pie chart with Id: {id}");

                await _databaseHandler.DeletePieChartByIdAsync(id);

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
                await response.WriteStringAsync($"Pie chart with Id {id} deleted successfully.");

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while deleting pie chart with Id {id}.");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        [Function("ReplacePieChartById")]
        public async Task<HttpResponseData> ReplacePieChartById([HttpTrigger(AuthorizationLevel.Function, "put", Route = "PieChart/Replace/{id:int}")] HttpRequestData req, int id)
        {
            try
            {
                _logger.LogInformation($"Replacing pie chart with Id: {id}");

                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var pieChart = JsonSerializer.Deserialize<PieChart>(requestBody);
                pieChart.Id = id;

                await _databaseHandler.ReplacePieChartByIdAsync(pieChart);

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
                await response.WriteStringAsync($"Pie chart with Id {id} replaced successfully.");

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while replacing pie chart with Id {id}.");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }
        #endregion

        #region User Management

        [Function("Login")]
        public async Task<HttpResponseData> Login([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "User/Login")] HttpRequestData req)
        {
            try
            {
                _logger.LogInformation("User login attempt.");

                // Ler o corpo da requisição
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var loginRequest = JsonSerializer.Deserialize<LoginRequest>(requestBody);

                // Validar credenciais do usuário por email ou username
                User user = null;
                if (!string.IsNullOrEmpty(loginRequest.Email))
                {
                    _logger.LogInformation("Attempting login by email.");
                    user = await _databaseHandler.AuthenticateUserAsync(loginRequest.Email, loginRequest.Password);
                }
                else if (!string.IsNullOrEmpty(loginRequest.Username))
                {
                    _logger.LogInformation("Attempting login by username.");
                    user = await _databaseHandler.AuthenticateUserAsync(loginRequest.Username, loginRequest.Password);
                }

                // Verificar se o usuário foi encontrado e a senha é válida
                if (user == null)
                {
                    _logger.LogWarning("Invalid login attempt.");
                    var result = req.CreateResponse(HttpStatusCode.Unauthorized);
                    await result.WriteStringAsync("Invalid username/email or password.");
                    return result;
                }

                // Gerar o token JWT
                var token = Utils.GenerateJwtToken(user);

                // Retornar o token ao cliente
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(JsonSerializer.Serialize(new { Token = token }));
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during login.");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }


        [Function("GetAllUsers")]
        public async Task<HttpResponseData> GetAllUsers([HttpTrigger(AuthorizationLevel.Function, "get", Route = "User/GetAll")] HttpRequestData req)
        {
            try
            {
                _logger.LogInformation("Retrieving all users.");
                var users = await _databaseHandler.GetAllUsersAsync();
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

                if (string.IsNullOrWhiteSpace(user.Password)) { await response.WriteStringAsync("A Password is required."); return response; };

                // Verifica se o salt foi fornecido
                if (string.IsNullOrEmpty(user.Salt) || !Convert.ToBoolean(user.Encrypted))
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
                    Role = "user",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    Group = "USER",
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
                var user = await _databaseHandler.GetUserByIdentifierAsync(identifier, true);

                if (user == null)
                {
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFoundResponse.WriteStringAsync("User not found.");
                    return notFoundResponse;
                }

                // Retornar o salt
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(JsonSerializer.Serialize(new { Salt = user.Salt }));
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the user salt.");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        #endregion

    }
    internal static class Utils
    {
        public static string GenerateSalt()
        {
            // Generate a cryptographic random salt
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        }

        public static string HashPassword(string password, string salt)
        {
            // Combine password and salt, then hash using SHA256
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var combined = Encoding.UTF8.GetBytes(password + salt);
                var hash = sha256.ComputeHash(combined);
                return Convert.ToBase64String(hash);
            }
        }

        public static string GenerateJwtToken(User user)
        {
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("MODSI$$AUTHT0K3N$$:(:/:)$$2024-2025_JRS"));
            var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

           // Token Claims
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("role", user.Role),
                new Claim("group", user.Group),
                new Claim("id", user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            
            // Token Attributes
            var token = new JwtSecurityToken(
                issuer: "MODSI_SQLRestAPI",
                audience: "MODSI_SQLRestAPI",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1), // Token valid for 1 hour
                signingCredentials: signingCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token); //Return token as a string
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