using System.Security.Cryptography;
using System.Text;

namespace visitor_admin.Helpers
{
    public static class PasswordHelper
    {
        // converts password into hashed password
        public static string Hash(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
        // verifies if a password is equal to the provided hashed password
        public static bool Verify(string password, string hash)
        {
            return Hash(password) == hash;
        }
    }
}
