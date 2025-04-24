using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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