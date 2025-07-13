using System.ComponentModel.DataAnnotations;

namespace CustomerServiceApp.Domain.Users;

public class Player : User
{
    [Required(ErrorMessage = "Player number is required.")]
    public required string PlayerNumber { get; init; }

    public override UserType UserType => UserType.Player;
}
