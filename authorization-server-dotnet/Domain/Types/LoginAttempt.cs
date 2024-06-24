namespace AuthService.Domain.Types;

public class LoginAttempt {
    public DateTime LastAttemptDate { get; set; }
    public int FailedAttemptsCount { get; set; }

    // Navigation properties
        // Navigation properties
    public int AccountId { get; set; }
    public Account Account { get; set; }
}