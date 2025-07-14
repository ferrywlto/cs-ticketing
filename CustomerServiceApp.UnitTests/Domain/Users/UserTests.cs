using CustomerServiceApp.Domain.Users;

namespace CustomerServiceApp.UnitTests.Domain.Users;

public class UserTests
{
    [Fact]
    public void Player_Should_Have_Required_Properties()
    {
        var player = new Player
        {
            Email = "test@example.com",
            Name = "John Doe",
            Avatar = "avatar.jpg",
            PlayerNumber = "P12345",
            PasswordHash = "hashed_password"
        };
        
        Assert.Equal("test@example.com", player.Email);
        Assert.Equal("John Doe", player.Name);
        Assert.Equal("avatar.jpg", player.Avatar);
        Assert.Equal("P12345", player.PlayerNumber);
        Assert.Equal("hashed_password", player.PasswordHash);
        Assert.Equal(UserType.Player, player.UserType);
    }

    [Fact]
    public void Agent_Should_Have_Required_Properties()
    {
        var agent = new Agent
        {
            Email = "agent@example.com",
            Name = "CS Agent",
            Avatar = "cs-avatar.jpg",
            PasswordHash = "hashed_password"
        };
        
        Assert.Equal("agent@example.com", agent.Email);
        Assert.Equal("CS Agent", agent.Name);
        Assert.Equal("cs-avatar.jpg", agent.Avatar);
        Assert.Equal("hashed_password", agent.PasswordHash);
        Assert.Equal(UserType.Agent, agent.UserType);
    }
}
