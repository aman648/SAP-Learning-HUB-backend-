namespace SAPWEbAPI_SOL.Models;

public class RegisterRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    // Optional: allow Admins to create users with explicit roles.
    public string? Role { get; set; }

    // When true, the response includes a JWT for the created user.
    public bool ReturnToken { get; set; } = true;
}

