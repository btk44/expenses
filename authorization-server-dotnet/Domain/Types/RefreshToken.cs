namespace AuthService.Domain.Types;

public class RefreshToken {
    public int Id { get; set; }
    public string Token { get; set; }
    public DateTime ExpiresAtUtc { get; set; }

    // Navigation properties
    public int AccountId { get; set; }
    public Account Account { get; set; }
}