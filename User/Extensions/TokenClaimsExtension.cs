using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MODSI_SQLRestAPI //Leave this as is to be able to use the extension method across the project
{
    /// <summary>
    /// Extensions to ClaimsPrincipal.
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Determines whether the current claims principal belongs to the specified group.
        /// </summary>
        /// <param name="principal">The claims principal.</param>
        /// <param name="group">The group to check.</param>
        /// <returns>true if the principal is in the specified group; otherwise, false.</returns>
        public static bool IsInGroup(this ClaimsPrincipal principal, string group)
        {
            if (principal == null) return false;

            foreach (var identity in principal.Identities)
            {
                if (identity != null && identity.HasClaim(CustomClaimTypes.Group, group))
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Custom claim types used in the application.
    /// </summary>
    public static class CustomClaimTypes
    {
        public const string Group = "group";
    }
}
