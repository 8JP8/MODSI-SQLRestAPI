using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using MODSI_SQLRestAPI.UserAuth.DTO;
using MODSI_SQLRestAPI.UserAuth.Models;
using MODSI_SQLRestAPI.UserAuth.Repositories;
using MODSI_SQLRestAPI.UserAuth.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MODSI_SQLRestAPI.UserAuth.Controllers
{
    internal class Login
    {
        private readonly ILogger _logger;
        private readonly UserRepository _userRepository;
        private readonly UserService _userService;
        private static readonly Dictionary<string, DateTime> PasswordResetRateLimit = new Dictionary<string, DateTime>();
        private static readonly TimeSpan RateLimitWindow = TimeSpan.FromMinutes(2);

        public Login(ILoggerFactory loggerFactory, UserService userService)
        {
            _logger = loggerFactory.CreateLogger<Login>();
            _userRepository = new UserRepository();
            _userService = userService;
        }

        [Function("Login")]
        public async Task<HttpResponseData> UserLogin([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "User/Login")] HttpRequestData req)
        {
            try
            {
                _logger.LogInformation("User login attempt.");

                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var loginRequest = JsonSerializer.Deserialize<LoginRequest>(requestBody);

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

                if (user == null)
                {
                    _logger.LogWarning("Invalid login attempt.");
                    var result = req.CreateResponse(HttpStatusCode.Unauthorized);
                    await result.WriteStringAsync("Invalid username/email or password.");
                    return result;
                }

                var token = TokenService.GenerateToken(user);

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

        [Function("CheckToken")]
        public async Task<HttpResponseData> CheckToken(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "User/CheckToken")] HttpRequestData req)
        {
            try
            {
                if (!req.Headers.TryGetValues("Authorization", out var authHeaders))
                {
                    var forbidden = req.CreateResponse(HttpStatusCode.Forbidden);
                    await forbidden.WriteStringAsync("Token is invalid");
                    return forbidden;
                }

                var authHeader = authHeaders.FirstOrDefault();
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    var forbidden = req.CreateResponse(HttpStatusCode.Forbidden);
                    await forbidden.WriteStringAsync("Token is invalid");
                    return forbidden;
                }

                var token = authHeader.Substring("Bearer ".Length);

                var principal = TokenService.ValidateToken(token);
                if (principal == null || !principal.Identity.IsAuthenticated)
                {
                    var forbidden = req.CreateResponse(HttpStatusCode.Forbidden);
                    await forbidden.WriteStringAsync("Token is invalid");
                    return forbidden;
                }

                var ok = req.CreateResponse(HttpStatusCode.OK);
                await ok.WriteStringAsync("Token is valid");
                return ok;
            }
            catch
            {
                var forbidden = req.CreateResponse(HttpStatusCode.Forbidden);
                await forbidden.WriteStringAsync("Token is invalid");
                return forbidden;
            }
        }

        [Function("RequestPasswordReset")]
        public async Task<HttpResponseData> RequestPasswordReset(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "User/RequestPasswordReset")] HttpRequestData req)
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var dto = JsonSerializer.Deserialize<PasswordResetRequestDTO>(requestBody.Replace("email", "Email"));

            if (dto == null || string.IsNullOrWhiteSpace(dto.Email))
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Email is required.");
                return badRequest;
            }

            // Rate limit por IP
            string ip = null;
            if (req.Headers.TryGetValues("X-Forwarded-For", out var values))
            {
                ip = values.FirstOrDefault();
            }
            else if (req.Headers.TryGetValues("REMOTE_ADDR", out var remoteAddrValues))
            {
                ip = remoteAddrValues.FirstOrDefault();
            }
            if (string.IsNullOrEmpty(ip)) ip = "unknown";

            bool isRateLimited = false;
            lock (PasswordResetRateLimit)
            {
                if (PasswordResetRateLimit.TryGetValue(ip, out var lastRequest) && DateTime.UtcNow - lastRequest < RateLimitWindow)
                {
                    isRateLimited = true;
                }
                else
                {
                    PasswordResetRateLimit[ip] = DateTime.UtcNow;
                }
            }
            if (isRateLimited)
            {
                var rateLimited = req.CreateResponse((HttpStatusCode)429);
                rateLimited.Headers.Add("Retry-After", RateLimitWindow.TotalSeconds.ToString());
                rateLimited.Headers.Add("Content-Type", "text/plain; charset=utf-8");
                await rateLimited.WriteStringAsync("RATE LIMITED");
                return rateLimited;
            }

            // Check if user exists
            var user = await _userService.GetUserByIdentifier(dto.Email);
            if (user == null)
            {
                var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                await notFound.WriteStringAsync("User not found.");
                return notFound;
            }

            // Generate secure code
            var code = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("=", "").Replace("+", "").Replace("/", "");
            await _userService.StorePasswordResetCode(user.Id, code, DateTime.UtcNow.AddMinutes(15));

            var link = ConfigurationManager.AppSettings["PasswordResetPageLink"] + code;

            var apiKey = ConfigurationManager.AppSettings["Resend_APIKey"];
            var emailBody = $"Olá, clique no link para resetar sua senha: <a href=\"{link}\">{link}</a><br><br>Código: {code}";
            var emailSent = await SendEmailWithResendApi(apiKey, user.Email, "Password Reset", emailBody);

            if (!emailSent)
            {
                var error = req.CreateResponse(HttpStatusCode.InternalServerError);
                await error.WriteStringAsync("Failed to send email.");
                return error;
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync("Password reset email sent.");
            return response;
        }

        // Auxiliar function to send the email
        internal async Task<bool> SendEmailWithResendApi(string apiKey, string to, string subject, string html)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                var content = new StringContent(JsonSerializer.Serialize(new
                {
                    from = "no-reply@modsivr.pt",
                    to = to,
                    subject = subject,
                    html = html
                }), Encoding.UTF8, "application/json");

                var result = await client.PostAsync("https://api.resend.com/emails", content);
                return result.IsSuccessStatusCode;
            }
        }

        [Function("SetPasswordByResetCode")]
        public async Task<HttpResponseData> SetPasswordByResetCode(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "User/SetPasswordByResetCode")] HttpRequestData req)
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonSerializer.Deserialize<Dictionary<string, string>>(requestBody);

            if (data == null || !data.ContainsKey("code") || string.IsNullOrWhiteSpace(data["code"]))
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Reset code is required.");
                return badRequest;
            }
            if (!data.ContainsKey("password") || string.IsNullOrWhiteSpace(data["password"]))
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Password is required.");
                return badRequest;
            }

            string code = data["code"];

            var resetEntry = await _userService.GetPasswordResetCodeEntry(code);
            if (resetEntry == null || resetEntry.Expiration < DateTime.UtcNow)
            {
                var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                await notFound.WriteStringAsync("Invalid or expired code.");
                return notFound;
            }

            var user = await _userRepository.GetUserByIdEntityAsync(resetEntry.UserId);
            if (user == null)
            {
                var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                await notFound.WriteStringAsync("User not found.");
                return notFound;
            }

            string newPassword = data["password"];
            string newSalt = data.ContainsKey("salt") ? data["salt"] : null;

            if (string.IsNullOrWhiteSpace(newSalt))
            {
                newSalt = UserRepository.PasswordUtils.GenerateSalt();
                newPassword = UserRepository.PasswordUtils.HashPassword(newPassword, newSalt);
            }

            user.Password = newPassword;
            user.Salt = newSalt;
            await _userRepository.UpdateUserByIdAsync(user);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync("Password updated successfully.");
            return response;
        }

        [Function("ChangePassword")]
        public async Task<HttpResponseData> ChangePassword(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "User/ChangePassword")] HttpRequestData req)
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonSerializer.Deserialize<Dictionary<string, string>>(requestBody);

            if (data == null ||
                !data.ContainsKey("identifier") ||
                !data.ContainsKey("currentPasswordHash") ||
                !data.ContainsKey("newPasswordHash") ||
                !data.ContainsKey("newSalt"))
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Required fields: identifier, currentPasswordHash, newPasswordHash, newSalt.");
                return badRequest;
            }

            var success = await _userService.ChangePassword(
                data["identifier"],
                data["currentPasswordHash"],
                data["newPasswordHash"],
                data["newSalt"]
            );

            var response = req.CreateResponse(success ? HttpStatusCode.OK : HttpStatusCode.Forbidden);
            await response.WriteStringAsync(success ? "Password changed successfully." : "Current password incorrect or user not found.");
            return response;
        }

    }
}
