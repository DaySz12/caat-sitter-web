namespace CatSitter.Api.Dtos;

public record RegisterRequest(
    string Email,
    string Password,
    string Name,
    string? Phone,
    string Role // "owner" | "sitter"
);

public record LoginRequest(string Email, string Password);

public record AuthResponse(string Token, UserDto User);
