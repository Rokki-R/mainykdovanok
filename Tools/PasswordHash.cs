using System.Security.Cryptography;

namespace mainykdovanok.Tools
{
    public class PasswordHash
    {
        private static byte[] createSalt()
        {
            byte[] salt = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        public static string hashPassword(string password, out byte[] salt)
        {
            salt = createSalt();
            var passwordHash = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA512);

            byte[] hash = passwordHash.GetBytes(32);
            return Convert.ToBase64String(hash);
        }

        public static string hashPassword(string password, byte[] salt)
        {
            var passwordHash = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA512);

            byte[] hash = passwordHash.GetBytes(32);
            return Convert.ToBase64String(hash);
        }

        public static bool doesPasswordMatch(string plain_password, string hashed_password, string password_salt)
        {
            byte[] salt = Convert.FromBase64String(password_salt);
            string hashed_plain_password = hashPassword(plain_password, salt);

            return hashed_plain_password == hashed_password;
        }
    }
}
