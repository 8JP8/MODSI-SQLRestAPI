using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using MODSI_SQLRestAPI.UserAuth.Models.User;
using System.Text.Json;
using MODSI_SQLRestAPI.UserAuth.Services;
using System.Configuration;
using UserAuthenticate.Repositories;

namespace MODSI_SQLRestAPI.Functions.Auth
{
    public class Login
    {
        private readonly ILogger _logger;
        private readonly UserRepository _userRepository;

        public Login(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Login>();

            // Inicializa o repositório com as configurações do Azure
            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            _userRepository = new UserRepository();
        }

        [Function("Login")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/login")] HttpRequestData req,
            FunctionContext executionContext)
        {
            _logger.LogInformation("Login request received");

            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                _logger.LogInformation($"Request body: {requestBody}");

                var loginRequest = JsonSerializer.Deserialize<User>(requestBody,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (loginRequest == null || string.IsNullOrEmpty(loginRequest.Email) || string.IsNullOrEmpty(loginRequest.Password))
                {
                    _logger.LogWarning("Invalid request: missing email or password");
                    var badResponse = req.CreateResponse();
                    badResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    await badResponse.WriteStringAsync("Email and password are required");
                    return badResponse;
                }

                _logger.LogInformation($"Attempting login for user: {loginRequest.Email}");

                // Tenta autenticar com repositório Azure
                var user = await _userRepository.GetByCredentialsAsync(loginRequest.Email, loginRequest.Password);

                // Mantém autenticação hardcoded para desenvolvimento (opcional)
                var hardcodedValid = loginRequest.Email == "admin" && loginRequest.Password == "1234";

                _logger.LogInformation($"Hardcoded auth result: {hardcodedValid}, Azure repository auth result: {user != null}");

                if (hardcodedValid || user != null)
                {
                    var authenticatedUser = user ?? new User
                    {
                        Id = 0,
                        Username = "boss",
                        Role = "admin",
                        Name = "Administrator",
                        Email = loginRequest.Email

                    };

                    // Nunca retornar a senha ao cliente
                    authenticatedUser.Password = null;

                    var token = TokenService.GenerateToken(authenticatedUser);

                    var response = req.CreateResponse();
                    await response.WriteAsJsonAsync(new { user = authenticatedUser, token });
                    return response;
                }

                _logger.LogWarning($"Authentication failed for user: {loginRequest.Email}");
                var unauthorizedResponse = req.CreateResponse();
                unauthorizedResponse.StatusCode = System.Net.HttpStatusCode.Unauthorized;
                await unauthorizedResponse.WriteStringAsync("Invalid credentials");
                return unauthorizedResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during login process: {ex.Message}");
                var errorResponse = req.CreateResponse();
                errorResponse.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                await errorResponse.WriteStringAsync($"An error occurred during authentication: {ex.Message}");
                return errorResponse;
            }
        }
    }
}