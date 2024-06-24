namespace AuthService.Domain.Types;

public class Account {
    public int Id { get; set; }
    public string Name { get; set; }
    public string Password { get; set; }
    public bool Active { get; set; }

    // Navigation properties
    public ICollection<RefreshToken> RefreshTokens { get; set; }
    public LoginAttempt LoginAttempt { get; set; }
}