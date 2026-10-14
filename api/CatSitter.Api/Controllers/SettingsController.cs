using CatSitter.Api.Data;
using CatSitter.Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatSitter.Api.Controllers;

[ApiController]
[Route("api/settings")]
public class SettingsController : ControllerBase
{
    private readonly AppDbContext _db;

    public SettingsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("payment-info")]
    public async Task<ActionResult<PlatformPaymentSettingDto>> GetPaymentInfo()
    {
        var setting = await _db.PlatformPaymentSettings.FirstOrDefaultAsync();
        if (setting is null)
        {
            return Ok(new PlatformPaymentSettingDto(
                "กสิกรไทย (KBANK)",
                "123-2-88990-0",
                "บจก. แคท ซิตเตอร์ (ไทยแลนด์)",
                "010-5-56600-0000",
                "บจก. แคท ซิตเตอร์ (ไทยแลนด์)"
            ));
        }

        return Ok(new PlatformPaymentSettingDto(
            setting.BankName,
            setting.BankAccountNumber,
            setting.BankAccountName,
            setting.PromptPayNumber,
            setting.PromptPayName
        ));
    }
}
