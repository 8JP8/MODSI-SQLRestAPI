using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace MODSI_SQLRestAPI.UserAuth.Controllers
{
    internal static class AuthSecrets
    {
        internal static byte[] Secret = Encoding.ASCII.GetBytes("MODSI$$AUTHT0K3N$$:(:/:)$$2024-2025_JRS");

        // NOTE: In a real application it is extremely important that this key is kept completely secret,
        // This is the secret encryption key that will generate our tokens.
        // In the case of this application, it is in this repository for educational purposes only. In a real application, this file and directory would be inside my .gitignore.

    }
}