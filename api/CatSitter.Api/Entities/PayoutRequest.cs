namespace CatSitter.Api.Entities;

public class PayoutRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SitterId { get; set; }
    public decimal Amount { get; set; }
    public string BankName { get; set; } = string.Empty;
    public string BankAccountNumber { get; set; } = string.Empty;
    public string BankAccountName { get; set; } = string.Empty;
    public string? PromptPayNumber { get; set; }
    public string Status { get; set; } = "pending"; // pending, completed, rejected
    public string? AdminNotes { get; set; }
    public string? TransferSlipUrl { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ProcessedAt { get; set; }

    public User? Sitter { get; set; }
}
