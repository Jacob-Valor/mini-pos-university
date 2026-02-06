using System;
using System.Security.Cryptography;
using System.Text;

namespace mini_pos.Services;

public static class PasswordHelper
{
    private const int SaltSize = 16; // 128-bit
    private const int KeySize = 32;  // 256-bit

    private const int LegacyIterations = 10_000;
    private const int CurrentIterations = 200_000;
    private const string AlgorithmTag = "pbkdf2-sha256";

    /// <summary>
    /// Hashes a password using PBKDF2 (HMACSHA256) with a random salt.
    /// Format: pbkdf2-sha256$&lt;iterations&gt;$&lt;saltHex&gt;$&lt;keyHex&gt;
    /// </summary>
    public static string HashPassword(string password)
    {
        return HashPasswordV2(password, CurrentIterations);
    }

    public static bool NeedsRehash(string? storedHash)
    {
        if (string.IsNullOrEmpty(storedHash))
            return false;

        // Always upgrade legacy formats.
        if (storedHash.Length == 32)
            return true;

        if (storedHash.Length == 96)
            return true;

        if (TryParseV2(storedHash, out var iterations, out _, out _))
            return iterations < CurrentIterations;

        return false;
    }

    /// <summary>
    /// Verifies a password against a stored hash.
    /// Supports legacy MD5 (32 chars), legacy PBKDF2 v1 (96 hex chars), and PBKDF2 v2 (tagged format).
    /// </summary>
    public static bool VerifyPassword(string inputPassword, string? storedHash)
    {
        if (string.IsNullOrEmpty(storedHash)) return false;

        // Legacy MD5 Check (32 hex chars)
        if (storedHash.Length == 32)
        {
            var inputMd5 = ComputeMd5Hash(inputPassword);
            return string.Equals(inputMd5, storedHash, StringComparison.OrdinalIgnoreCase);
        }

        // Legacy PBKDF2 v1 (96 hex chars)
        if (storedHash.Length == 96)
        {
            try
            {
                byte[] hashBytes = Convert.FromHexString(storedHash);
                if (hashBytes.Length != SaltSize + KeySize)
                    return false;

                byte[] salt = new byte[SaltSize];
                Array.Copy(hashBytes, 0, salt, 0, SaltSize);

                byte[] storedKey = new byte[KeySize];
                Array.Copy(hashBytes, SaltSize, storedKey, 0, KeySize);

                byte[] computedKey = Rfc2898DeriveBytes.Pbkdf2(
                    inputPassword,
                    salt,
                    LegacyIterations,
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

        // PBKDF2 v2 (tagged format)
        if (TryParseV2(storedHash, out var iterations, out var saltBytes, out var storedKeyBytes))
        {
            byte[] computedKey = Rfc2898DeriveBytes.Pbkdf2(
                inputPassword,
                saltBytes,
                iterations,
                HashAlgorithmName.SHA256,
                KeySize);

            return CryptographicOperations.FixedTimeEquals(storedKeyBytes, computedKey);
        }

        return false;
    }

    private static string HashPasswordV2(string password, int iterations)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] key = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            KeySize);

        return $"{AlgorithmTag}${iterations}${Convert.ToHexString(salt).ToLowerInvariant()}${Convert.ToHexString(key).ToLowerInvariant()}";
    }

    private static bool TryParseV2(string storedHash, out int iterations, out byte[] salt, out byte[] key)
    {
        iterations = 0;
        salt = Array.Empty<byte>();
        key = Array.Empty<byte>();

        var parts = storedHash.Split('$');
        if (parts.Length != 4)
            return false;

        if (!string.Equals(parts[0], AlgorithmTag, StringComparison.Ordinal))
            return false;

        if (!int.TryParse(parts[1], out iterations) || iterations <= 0)
            return false;

        if (!TryDecodeHex(parts[2], SaltSize, out salt))
            return false;

        if (!TryDecodeHex(parts[3], KeySize, out key))
            return false;

        return true;
    }

    private static bool TryDecodeHex(string hex, int expectedBytes, out byte[] bytes)
    {
        bytes = Array.Empty<byte>();
        if (hex.Length != expectedBytes * 2)
            return false;

        try
        {
            bytes = Convert.FromHexString(hex);
            return bytes.Length == expectedBytes;
        }
        catch
        {
            return false;
        }
    }

    private static string ComputeMd5Hash(string input)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        byte[] hashBytes = MD5.HashData(inputBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
