namespace CatSitter.Api.Entities;

public class PlatformPaymentSetting
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string BankName { get; set; } = "กสิกรไทย (KBANK)";
    public string BankAccountNumber { get; set; } = "123-2-88990-0";
    public string BankAccountName { get; set; } = "บจก. แคท ซิตเตอร์ (ไทยแลนด์)";
    public string PromptPayNumber { get; set; } = "010-5-56600-0000";
    public string PromptPayName { get; set; } = "บจก. แคท ซิตเตอร์ (ไทยแลนด์)";
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
