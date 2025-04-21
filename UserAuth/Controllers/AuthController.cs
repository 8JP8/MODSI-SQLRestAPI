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
using System.Net;

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
        public async Task<HttpResponseData> UserLogin([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "User/Login")] HttpRequestData req)
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
                    user = await _userRepository.AuthenticateUserAsync(loginRequest.Email, loginRequest.Password);
                }
                else if (!string.IsNullOrEmpty(loginRequest.Username))
                {
                    _logger.LogInformation("Attempting login by username.");
                    user = await _userRepository.AuthenticateUserAsync(loginRequest.Username, loginRequest.Password);
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
                var token = TokenService.GenerateToken(user);

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
    }
}
