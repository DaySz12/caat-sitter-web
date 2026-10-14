namespace CatSitter.Api.Dtos;

public record CreatePayoutRequest(
    decimal Amount,
    string BankName,
    string BankAccountNumber,
    string BankAccountName,
    string? PromptPayNumber
);

public record PayoutDto(
    Guid Id,
    Guid SitterId,
    string SitterName,
    string? SitterAvatarUrl,
    decimal Amount,
    string BankName,
    string BankAccountNumber,
    string BankAccountName,
    string? PromptPayNumber,
    string Status,
    string? AdminNotes,
    string? TransferSlipUrl,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ProcessedAt
);

public record AdminRejectPayoutRequest(string Reason);

public record AdminApprovePayoutRequest(string? TransferSlipUrl);

public record PlatformPaymentSettingDto(
    string BankName,
    string BankAccountNumber,
    string BankAccountName,
    string PromptPayNumber,
    string PromptPayName
);

public record UpdatePlatformPaymentSettingRequest(
    string BankName,
    string BankAccountNumber,
    string BankAccountName,
    string PromptPayNumber,
    string PromptPayName
);
