namespace CustomerServiceApp.Domain.Users;

public class Agent : User
{
    public override UserType UserType => UserType.Agent;
}
