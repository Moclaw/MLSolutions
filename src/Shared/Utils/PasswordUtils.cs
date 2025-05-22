// ReSharper disable All
namespace Shared.Utils
{
    /// <summary>
    /// Provides utility methods for password hashing, verification, and generation.
    /// </summary>
    public struct PasswordUtils
    {
        /// <summary>
        /// Hashes a password using the MD5 algorithm.
        /// </summary>
        /// <param name="password">The password to hash.</param>
        /// <returns>The hashed password as a Base64-encoded string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the password is null or empty.</exception>
        public static string HashMd5(string password)
        {
            if (string.IsNullOrEmpty(password)) throw new ArgumentNullException(nameof(password));
            using var md5 = System.Security.Cryptography.MD5.Create();
            return ComputeHash(md5, password);
        }

        /// <summary>
        /// Hashes a password using the SHA-1 algorithm.
        /// </summary>
        /// <param name="password">The password to hash.</param>
        /// <returns>The hashed password as a Base64-encoded string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the password is null or empty.</exception>
        public static string HashSha1(string password)
        {
            if (string.IsNullOrEmpty(password)) throw new ArgumentNullException(nameof(password));
            using var sha1 = System.Security.Cryptography.SHA1.Create();
            return ComputeHash(sha1, password);
        }

        /// <summary>
        /// Hashes a password using the SHA-256 algorithm.
        /// </summary>
        /// <param name="password">The password to hash.</param>
        /// <returns>The hashed password as a Base64-encoded string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the password is null or empty.</exception>
        public static string HashSha256(string password)
        {
            if (string.IsNullOrEmpty(password)) throw new ArgumentNullException(nameof(password));
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            return ComputeHash(sha256, password);
        }

        /// <summary>
        /// Verifies if a password matches a given hashed password using SHA-256.
        /// </summary>
        /// <param name="password">The plain text password to verify.</param>
        /// <param name="hashedPassword">The hashed password to compare against.</param>
        /// <returns>True if the password matches the hashed password; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either the password or hashedPassword is null or empty.</exception>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrEmpty(password)) throw new ArgumentNullException(nameof(password));
            if (string.IsNullOrEmpty(hashedPassword)) throw new ArgumentNullException(nameof(hashedPassword));
            return HashSha256(password) == hashedPassword;
        }

        /// <summary>
        /// Generates a random password of the specified length.
        /// </summary>
        /// <param name="length">The length of the password to generate.</param>
        /// <returns>A randomly generated password.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the length is less than or equal to zero.</exception>
        public static string GenerateRandomPassword(int length)
        {
            if (length <= 0)
                throw new ArgumentOutOfRangeException(nameof(length), "Password length must be greater than zero.");
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            var passwordChars = new char[length];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            var buffer = new byte[4];
            for (int i = 0; i < length; i++)
            {
                rng.GetBytes(buffer);
                passwordChars[i] = validChars[(int)(BitConverter.ToUInt32(buffer, 0) % validChars.Length)];
            }

            return new string(passwordChars);
        }

        /// <summary>
        /// Computes the hash of a given input using the specified hash algorithm.
        /// </summary>
        /// <param name="algorithm">The hash algorithm to use.</param>
        /// <param name="input">The input string to hash.</param>
        /// <returns>The hashed input as a Base64-encoded string.</returns>
        private static string ComputeHash(System.Security.Cryptography.HashAlgorithm algorithm, string input)
        {
            var inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
            var hashBytes = algorithm.ComputeHash(inputBytes);
            return Convert.ToBase64String(hashBytes);
        }
    }
}