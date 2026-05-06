using Microsoft.IdentityModel.Tokens;
using SAPWEbAPI_SOL.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SAPWEbAPI_SOL.ServiceLayer;

public class JwtTokenService
{
    private readonly IConfiguration config;

    public JwtTokenService(IConfiguration config)
    {
        this.config = config;
    }

    public string CreateToken(user u)
    {
        var jwt = config.GetSection("Jwt");
        var issuer = jwt.GetValue<string>("Issuer") ?? throw new InvalidOperationException("Jwt:Issuer missing");
        var audience = jwt.GetValue<string>("Audience") ?? throw new InvalidOperationException("Jwt:Audience missing");
        var key = jwt.GetValue<string>("Key") ?? throw new InvalidOperationException("Jwt:Key missing");
        var expMinutes = jwt.GetValue<int?>("ExpiresMinutes") ?? 60;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, u.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, u.Email ?? string.Empty),
            new("name", u.Name ?? string.Empty),
            new(ClaimTypes.Role, u.Role ?? string.Empty)
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(expMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

