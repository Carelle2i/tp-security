using System.Security.Cryptography;
using System.Text;

namespace YourNamespace.Helpers // Remplace 'YourNamespace' par ton namespace
{
    public static class StringExtensions
    {
        public static string Sha256(this string str)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(str));
                var builder = new StringBuilder();
                foreach (var byteValue in bytes)
                {
                    builder.Append(byteValue.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
