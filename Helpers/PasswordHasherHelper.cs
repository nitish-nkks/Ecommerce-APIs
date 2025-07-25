using Microsoft.AspNetCore.Identity;

namespace Ecommerce_APIs.Helpers
{
    public static class PasswordHasherHelper
    {
        private static readonly PasswordHasher<string> hasher = new();

        public static string HashPassword(string password)
        {
            return hasher.HashPassword(null, password);
        }

        public static bool VerifyPassword(string hashedPassword, string providedPassword)
        {
            var result = hasher.VerifyHashedPassword(null, hashedPassword, providedPassword);
            return result == PasswordVerificationResult.Success;
        }
    }
}
