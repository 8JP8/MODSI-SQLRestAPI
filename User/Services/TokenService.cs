using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.IdentityModel.Tokens;
using MODSI_SQLRestAPI.UserAuth.Controllers;
using MODSI_SQLRestAPI.UserAuth.Models;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace MODSI_SQLRestAPI.UserAuth.Services
{
    public static class TokenService
    {
        private static string Issuer = "MODSI_SQLRestAPI";


        public static string GenerateToken(User user)
        {
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("MODSI$$AUTHT0K3N$$:(:/:)$$2024-2025_JRS"));
            var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            // Claims adicionais
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("role", user.Role ?? "user"),
                new Claim("group", user.Group ?? "USER"),
                new Claim("id", user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: "MODSI_SQLRestAPI",
                audience: "MODSI_SQLRestAPI",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: signingCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static ClaimsPrincipal ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            byte[] key = AuthSecrets.Secret;

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true, // Valida o emissor
                    ValidIssuer = Issuer, // Substitua pelo emissor válido
                    ValidateAudience = false, // Valida a audiência
                    //ValidAudience = "YourAudience", // Substitua pela audiência válida
                    ValidateLifetime = true, // Valida a expiração do token
                    ClockSkew = TimeSpan.Zero // Sem tolerância para diferenças de tempo
                }, out var validatedToken);

                // Verifica se o token é um JWT
                if (validatedToken is JwtSecurityToken jwtToken)
                {
                    // Valida o algoritmo de assinatura
                    if (!jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                        throw new SecurityTokenException("Token inválido.");
                }

                return principal;
            }
            catch
            {
                // Log de erro pode ser adicionado aqui
                return null;
            }
        }
    }

    class RetrieveToken
    {
        public ClaimsPrincipal GetPrincipalFromRequest(HttpRequestData req)
        {
            // Extract token from Authorization header
            if (!req.Headers.TryGetValues("Authorization", out var authHeaders))
                return null;

            var authHeader = authHeaders.FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return null;

            var token = authHeader.Substring("Bearer ".Length);
            return TokenService.ValidateToken(token);
        }
    }
}