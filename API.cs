using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;



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
                _logger.LogInformation($"Retrieving point with ID: {id}");

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
                _logger.LogError(ex, $"An error occurred while retrieving point with ID {id}.");
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
                _logger.LogInformation($"Deleting point with ID: {id}");

                await _databaseHandler.DeletePointByIdAsync(id);

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
                await response.WriteStringAsync($"Point with ID {id} deleted successfully.");

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while deleting point with ID {id}.");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        [Function("ReplacePointById")]
        public async Task<HttpResponseData> ReplacePointById([HttpTrigger(AuthorizationLevel.Function, "put", Route = "3DPoints/Replace/{id:int}")] HttpRequestData req, int id)
        {
            try
            {
                _logger.LogInformation($"Replacing point with ID: {id}");

                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var point = JsonSerializer.Deserialize<Point3D>(requestBody);
                point.ID = id;

                await _databaseHandler.ReplacePointByIdAsync(point);

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
                await response.WriteStringAsync($"Point with ID {id} replaced successfully.");

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while replacing point with ID {id}.");
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
                _logger.LogInformation($"Retrieving pie chart with ID: {id}");

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
                _logger.LogError(ex, $"An error occurred while retrieving pie chart with ID {id}.");
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
                _logger.LogInformation($"Deleting pie chart with ID: {id}");

                await _databaseHandler.DeletePieChartByIdAsync(id);

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
                await response.WriteStringAsync($"Pie chart with ID {id} deleted successfully.");

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while deleting pie chart with ID {id}.");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        [Function("ReplacePieChartById")]
        public async Task<HttpResponseData> ReplacePieChartById([HttpTrigger(AuthorizationLevel.Function, "put", Route = "PieChart/Replace/{id:int}")] HttpRequestData req, int id)
        {
            try
            {
                _logger.LogInformation($"Replacing pie chart with ID: {id}");

                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var pieChart = JsonSerializer.Deserialize<PieChart>(requestBody);
                pieChart.Id = id;

                await _databaseHandler.ReplacePieChartByIdAsync(pieChart);

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
                await response.WriteStringAsync($"Pie chart with ID {id} replaced successfully.");

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while replacing pie chart with ID {id}.");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }
        #endregion

        #region User Management

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
                _logger.LogInformation($"Retrieving user with ID: {id}");
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
                _logger.LogError(ex, $"An error occurred while retrieving user with ID {id}.");
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

                // Generate salt and hash password
                var salt = PasswordUtils.GenerateSalt();
                var passwordHash = PasswordUtils.HashPassword(user.Password, salt);

                // Map MODSI_SQLRestAPI.User to MODSI_SQLRestAPI.DatabaseHandler.User
                var dbUser = new User
                {
                    Name = user.Name,
                    Email = user.Email,
                    Password = passwordHash,
                    Username = user.Username,
                    Role = "User",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    Group = "USER",
                    Salt = salt
                };

                await _databaseHandler.AddUserAsync(dbUser);

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
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
                _logger.LogInformation($"Deleting user with ID: {id}");
                await _databaseHandler.DeleteUserByIdAsync(id);
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
                await response.WriteStringAsync($"User with ID {id} deleted successfully.");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while deleting user with ID {id}.");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        [Function("UpdateUserById")]
        public async Task<HttpResponseData> UpdateUserById([HttpTrigger(AuthorizationLevel.Function, "put", Route = "User/Update/{id:int}")] HttpRequestData req, int id)
        {
            try
            {
                _logger.LogInformation($"Updating user with ID: {id}");
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var user = JsonSerializer.Deserialize<User>(requestBody);
                user.ID = id;

                // Map MODSI_SQLRestAPI.User to MODSI_SQLRestAPI.DatabaseHandler.User
                var dbUser = new User
                {
                    ID = user.ID,
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
                await response.WriteStringAsync($"User with ID {id} updated successfully.");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating user with ID {id}.");
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
                var user = await _databaseHandler.GetUserByEmailAsync(email);

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

        #endregion

    }
    internal static class PasswordUtils
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
                var combined = System.Text.Encoding.UTF8.GetBytes(password + salt);
                var hash = sha256.ComputeHash(combined);
                return Convert.ToBase64String(hash);
            }
        }
    }
}