using System.Security.Claims;

namespace Ecommerce_APIs.Helpers
{
    public static class TokenHelper
    {
        public static int GetUserIdFromClaims(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst("UserId") ??
                              user.FindFirst(ClaimTypes.NameIdentifier) ??
                              user.FindFirst("sub");

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                throw new UnauthorizedAccessException("User ID not found in token.");
            }

            return userId;
        }
        public static string GetUserTypeFromClaims(ClaimsPrincipal user)
        {
            var userTypeClaim = user.FindFirst("UserType"); 
            return userTypeClaim?.Value ?? "Customer"; 
        }
        public static (int? userId, string? userType) GetUserInfoFromClaims(ClaimsPrincipal user)
        {
            try
            {
                var userId = GetUserIdFromClaims(user);
                var userType = GetUserTypeFromClaims(user);
                return (userId, userType);
            }
            catch
            {
                return (null, null);
            }
        }
    }
}
