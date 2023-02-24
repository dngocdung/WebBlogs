using System.Security.Claims;

namespace WebBlogs.Extension
{
    public static class IdentityExtensions
    {
        public static string GetSpecificClaim(this ClaimsPrincipal claimsPrincipal, string claimType)
        {
            var claim = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == claimType);
            return (claim != null) ? claim.Value : string.Empty;
        }
    }
}
