namespace Shard.Utils
{
    public static struct PasswordUtils
    {
        public static string HashMd5(string password)
        {
            if (string.IsNullOrEmpty(password)) throw new ArgumentNullException(nameof(password));
            using var md5 = System.Security.Cryptography.MD5.Create();
            return ComputeHash(md5, password);
        }

        public static string HashSha1(string password)
        {
            if (string.IsNullOrEmpty(password)) throw new ArgumentNullException(nameof(password));
            using var sha1 = System.Security.Cryptography.SHA1.Create();
            return ComputeHash(sha1, password);
        }

        public static string HashSha256(string password)
        {
            if (string.IsNullOrEmpty(password)) throw new ArgumentNullException(nameof(password));
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            return ComputeHash(sha256, password);
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrEmpty(password)) throw new ArgumentNullException(nameof(password));
            if (string.IsNullOrEmpty(hashedPassword)) throw new ArgumentNullException(nameof(hashedPassword));
            return HashSha256(password) == hashedPassword;
        }

        public static string GenerateRandomPassword(int length)
        {
            if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length), "Password length must be greater than zero.");
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            var passwordChars = new char[length];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            var buffer = new byte[4];
            for (int i = 0; i < length; i++)
            {
                rng.GetBytes(buffer);
                passwordChars[i] = validChars[BitConverter.ToUInt32(buffer, 0) % validChars.Length];
            }
            return new string(passwordChars);
        }

        private static string ComputeHash(System.Security.Cryptography.HashAlgorithm algorithm, string input)
        {
            var inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
            var hashBytes = algorithm.ComputeHash(inputBytes);
            return Convert.ToBase64String(hashBytes);
        }
    }
