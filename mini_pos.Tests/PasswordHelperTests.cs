using Xunit;
using mini_pos.Services;

namespace mini_pos.Tests;

public class PasswordHelperTests
{
    [Fact]
    public void HashPassword_Returns96CharHexString()
    {
        var password = "TestPassword123";
        var hash = PasswordHelper.HashPassword(password);
        
        Assert.NotNull(hash);
        Assert.Equal(96, hash.Length);
        Assert.Matches("^[0-9a-f]{96}$", hash);
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
        Assert.Equal(96, hash.Length);
        Assert.True(PasswordHelper.VerifyPassword("", hash));
    }

    [Fact]
    public void HashPassword_UnicodePassword_GeneratesValidHash()
    {
        var password = "ລາວ123ABC";
        var hash = PasswordHelper.HashPassword(password);
        
        Assert.NotNull(hash);
        Assert.Equal(96, hash.Length);
        Assert.True(PasswordHelper.VerifyPassword(password, hash));
    }
}
