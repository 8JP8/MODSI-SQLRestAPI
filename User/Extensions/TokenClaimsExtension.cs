using System.Security.Claims;

namespace MODSI_SQLRestAPI
{
    public static class ClaimsPrincipalExtensions
    {
        public static bool IsInGroup(this ClaimsPrincipal claimsPrincipal, string group)
        {
            if (claimsPrincipal == null) return false;

            const string groupClaimType = "http://schemas.xmlsoap.org/claims/Group";

            foreach (var identity in claimsPrincipal.Identities)
            {
                if (identity != null && identity.HasClaim(groupClaimType, group))
                {
                    return true;
                }
            }

            return false;
        }
    }
}