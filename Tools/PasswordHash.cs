using System.Security.Cryptography;

namespace mainykdovanok.Tools
{
    public class PasswordHash
    {
        /// <summary>
        /// Creates a new randomly generated 32 byte salt.
        /// </summary>
        /// <returns>32 byte array containing the salt.</returns>
        private static byte[] createSalt()
        {
            byte[] salt = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        /// <summary>
        /// Creates a new password hash using a new salt.
        /// </summary>
        /// <param name="password">The plain text password to hash.</param>
        /// <param name="salt">The new salt that will be generated.</param>
        /// <returns>The hashed password.</returns>
        public static string hashPassword(string password, out byte[] salt)
        {
            salt = createSalt();
            var passwordHash = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA512);

            byte[] hash = passwordHash.GetBytes(32);
            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Creates a password hash using an existing salt.
        /// </summary>
        /// <param name="password">The plain text password to hash.</param>
        /// <param name="salt">The existing salt that will be used.</param>
        /// <returns>The hashed password.</returns>
        public static string hashPassword(string password, byte[] salt)
        {
            var passwordHash = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA512);

            byte[] hash = passwordHash.GetBytes(32);
            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Checks if the plain text password matches the hashed one.
        /// </summary>
        /// <param name="plain_password">The plain text password.</param>
        /// <param name="hashed_password">The hashed password.</param>
        /// <param name="salt">The hashed password's salt.</param>
        /// <returns>True if the passwords match, false otherwise.</returns>
        public static bool doesPasswordMatch(string plain_password, string hashed_password, string password_salt)
        {
            byte[] salt = Convert.FromBase64String(password_salt);
            string hashed_plain_password = hashPassword(plain_password, salt);

            return hashed_plain_password == hashed_password;
        }
    }
}
