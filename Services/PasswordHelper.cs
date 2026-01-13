using System;
using System.Security.Cryptography;
using System.Text;

namespace mini_pos.Services;

public static class PasswordHelper
{
    private const int SaltSize = 16; // 128 bit
    private const int KeySize = 32;  // 256 bit
    private const int Iterations = 10000;

    /// <summary>
    /// Hashes a password using PBKDF2 (HMACSHA256) with a random salt.
    /// Format: [16 bytes salt][32 bytes hash] as a hex string (96 chars).
    /// </summary>
    public static string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            KeySize);

        byte[] hashBytes = new byte[SaltSize + KeySize];
        Array.Copy(salt, 0, hashBytes, 0, SaltSize);
        Array.Copy(hash, 0, hashBytes, SaltSize, KeySize);

        return Convert.ToHexString(hashBytes).ToLower();
    }

    /// <summary>
    /// Verifies a password against a stored hash.
    /// Supports both legacy MD5 (32 chars) and new PBKDF2 (96 chars).
    /// </summary>
    public static bool VerifyPassword(string inputPassword, string storedHash)
    {
        if (string.IsNullOrEmpty(storedHash)) return false;

        // Legacy MD5 Check (32 hex chars)
        if (storedHash.Length == 32)
        {
            var inputMd5 = ComputeMd5Hash(inputPassword);
            return string.Equals(inputMd5, storedHash, StringComparison.OrdinalIgnoreCase);
        }

        // New PBKDF2 Check (96 hex chars)
        if (storedHash.Length == 96)
        {
            try
            {
                byte[] hashBytes = Convert.FromHexString(storedHash);
                
                // Extract salt
                byte[] salt = new byte[SaltSize];
                Array.Copy(hashBytes, 0, salt, 0, SaltSize);

                // Extract stored key
                byte[] storedKey = new byte[KeySize];
                Array.Copy(hashBytes, SaltSize, storedKey, 0, KeySize);

                // Compute hash of input using extracted salt
                byte[] computedKey = Rfc2898DeriveBytes.Pbkdf2(
                    inputPassword,
                    salt,
                    Iterations,
                    HashAlgorithmName.SHA256,
                    KeySize);

                return CryptographicOperations.FixedTimeEquals(storedKey, computedKey);
            }
            catch
            {
                // Invalid format
                return false;
            }
        }

        return false;
    }

    private static string ComputeMd5Hash(string input)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        byte[] hashBytes = MD5.HashData(inputBytes);
        return Convert.ToHexString(hashBytes).ToLower();
    }
}
