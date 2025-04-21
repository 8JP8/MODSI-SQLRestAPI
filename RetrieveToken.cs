using Microsoft.Azure.Functions.Worker.Http;
using MODSI_SQLRestAPI.UserAuth.Services;
using System.Linq;
using System.Security.Claims;

namespace MODSI_SQLRestAPI
{
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
