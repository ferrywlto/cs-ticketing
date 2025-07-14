using CustomerServiceApp.Infrastructure.Options;
using CustomerServiceApp.Infrastructure.Services;
using Microsoft.Extensions.Options;
using Xunit;

namespace CustomerServiceApp.UnitTests.Infrastructure.Services;

public class PasswordHasherTests
{
    private readonly PasswordHasher _passwordHasher;
    private const string TestSalt = "TestSalt_32Characters_ForTesting_Purpose";

    public PasswordHasherTests()
    {
        var options = new PasswordHasherOptions
        {
            Salt = TestSalt,
            Iterations = 10000 // Lower for faster tests
        };
        var optionsWrapper = Options.Create(options);
        _passwordHasher = new PasswordHasher(optionsWrapper);
    }

    [Fact]
    public void HashPassword_Should_Return_Non_Empty_Hash()
    {
        var password = "testpassword123";

        var hash = _passwordHasher.HashPassword(password);

        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
        Assert.NotEqual(password, hash);
    }

    [Fact]
    public void HashPassword_Should_Return_Consistent_Hash_For_Same_Password()
    {
        var password = "testpassword123";

        var hash1 = _passwordHasher.HashPassword(password);
        var hash2 = _passwordHasher.HashPassword(password);

        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void HashPassword_Should_Return_Different_Hash_For_Different_Passwords()
    {
        var password1 = "testpassword123";
        var password2 = "differentpassword456";

        var hash1 = _passwordHasher.HashPassword(password1);
        var hash2 = _passwordHasher.HashPassword(password2);

        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void VerifyPassword_Should_Return_True_For_Correct_Password()
    {
        var password = "testpassword123";
        var hash = _passwordHasher.HashPassword(password);

        var result = _passwordHasher.VerifyPassword(password, hash);

        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_Should_Return_False_For_Incorrect_Password()
    {
        var password = "testpassword123";
        var wrongPassword = "wrongpassword456";
        var hash = _passwordHasher.HashPassword(password);

        var result = _passwordHasher.VerifyPassword(wrongPassword, hash);

        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_Should_Return_False_For_Empty_Password()
    {
        var password = "testpassword123";
        var hash = _passwordHasher.HashPassword(password);

        var result = _passwordHasher.VerifyPassword("", hash);

        Assert.False(result);
    }

    [Theory]
    [InlineData("simple")]
    [InlineData("ComplexP@ssw0rd!")]
    [InlineData("verylongpasswordwithmanycharactersandnumbers123456789")]
    [InlineData("短密码")]
    [InlineData("пароль")]
    public void HashPassword_Should_Work_With_Various_Password_Types(string password)
    {
        var hash = _passwordHasher.HashPassword(password);
        var isValid = _passwordHasher.VerifyPassword(password, hash);

        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
        Assert.True(isValid);
    }
}
