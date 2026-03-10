using Xunit;

using mini_pos.Services;

using System;
using System.Security.Cryptography;

namespace mini_pos.Tests;

public class PasswordHelperTests
{
    [Fact]
    public void HashPassword_ReturnsTaggedPbkdf2Hash()
    {
        var password = "TestPassword123";
        var hash = PasswordHelper.HashPassword(password);

        Assert.NotNull(hash);
        Assert.StartsWith("pbkdf2-sha256$", hash, StringComparison.Ordinal);

        var parts = hash.Split('$');
        Assert.Equal(4, parts.Length);
        Assert.Equal("pbkdf2-sha256", parts[0]);
        Assert.True(int.TryParse(parts[1], out var iterations));
        Assert.True(iterations > 0);
        Assert.Matches("^[0-9a-f]+$", parts[2]);
        Assert.Matches("^[0-9a-f]+$", parts[3]);
    }

    [Fact]
    public void HashPassword_SamePassword_ProducesDifferentHashes()
    {
        var password = "TestPassword123";
        var hash1 = PasswordHelper.HashPassword(password);
        var hash2 = PasswordHelper.HashPassword(password);

        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void VerifyPassword_CorrectPassword_ReturnsTrue()
    {
        var password = "TestPassword123";
        var hash = PasswordHelper.HashPassword(password);

        var result = PasswordHelper.VerifyPassword(password, hash);

        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_LegacyPbkdf2V1_ReturnsTrue()
    {
        var password = "TestPassword123";
        var legacyHash = CreateLegacyV1Hash(password);

        var result = PasswordHelper.VerifyPassword(password, legacyHash);

        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_WrongPassword_ReturnsFalse()
    {
        var password = "TestPassword123";
        var wrongPassword = "WrongPassword456";
        var hash = PasswordHelper.HashPassword(password);

        var result = PasswordHelper.VerifyPassword(wrongPassword, hash);

        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_CorrectMd5Hash_ReturnsTrue()
    {
        var password = "test123";
        var md5Hash = ComputeMd5Hash(password);

        var result = PasswordHelper.VerifyPassword(password, md5Hash);

        Assert.True(result);
    }

    private static string ComputeMd5Hash(string input)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
        var hashBytes = md5.ComputeHash(inputBytes);
        return Convert.ToHexString(hashBytes).ToLower();
    }

    [Fact]
    public void VerifyPassword_WrongMd5Password_ReturnsFalse()
    {
        var wrongPassword = "wrongpassword";
        var md5Hash = "482c811da5d5b4bc6d497ffa98491e38";

        var result = PasswordHelper.VerifyPassword(wrongPassword, md5Hash);

        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_EmptyStoredHash_ReturnsFalse()
    {
        var result = PasswordHelper.VerifyPassword("password", "");

        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_NullStoredHash_ReturnsFalse()
    {
        var result = PasswordHelper.VerifyPassword("password", null!);

        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_InvalidHashFormat_ReturnsFalse()
    {
        var result = PasswordHelper.VerifyPassword("password", "invalid-hash-format");

        Assert.False(result);
    }

    [Fact]
    public void HashPassword_EmptyPassword_GeneratesValidHash()
    {
        var hash = PasswordHelper.HashPassword("");

        Assert.NotNull(hash);
        Assert.StartsWith("pbkdf2-sha256$", hash, StringComparison.Ordinal);
        Assert.True(PasswordHelper.VerifyPassword("", hash));
    }

    [Fact]
    public void HashPassword_UnicodePassword_GeneratesValidHash()
    {
        var password = "ລາວ123ABC";
        var hash = PasswordHelper.HashPassword(password);

        Assert.NotNull(hash);
        Assert.StartsWith("pbkdf2-sha256$", hash, StringComparison.Ordinal);
        Assert.True(PasswordHelper.VerifyPassword(password, hash));
    }

    private static string CreateLegacyV1Hash(string password)
    {
        const int saltSize = 16;
        const int keySize = 32;
        const int iterations = 10_000;

        byte[] salt = RandomNumberGenerator.GetBytes(saltSize);
        byte[] key = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, keySize);

        byte[] hashBytes = new byte[saltSize + keySize];
        Array.Copy(salt, 0, hashBytes, 0, saltSize);
        Array.Copy(key, 0, hashBytes, saltSize, keySize);

        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
