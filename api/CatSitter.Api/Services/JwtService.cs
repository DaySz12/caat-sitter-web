using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CatSitter.Api.Entities;
using Microsoft.IdentityModel.Tokens;

namespace CatSitter.Api.Services;

public class JwtService
{
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(User user)
    {
        var secret = _config["Jwt:Secret"]
            ?? throw new InvalidOperationException("Jwt:Secret is not configured");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Role, user.Role),
            new("name", user.Name),
            new("status", user.Status),
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"] ?? "catsitter-api",
            audience: _config["Jwt:Audience"] ?? "catsitter-web",
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
