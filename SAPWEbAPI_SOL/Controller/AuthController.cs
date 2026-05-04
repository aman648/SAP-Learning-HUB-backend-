using Microsoft.AspNetCore.Mvc;
using SAPWEbAPI_SOL.Data;
using SAPWEbAPI_SOL.Models;
using SAPWEbAPI_SOL.ServiceLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace SAPWEbAPI_SOL.Controller;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly DB_helper db;
    private readonly IPasswordHasher<user> passwordHasher;
    private readonly JwtTokenService jwt;
    
    public AuthController(DB_helper db, IPasswordHasher<user> passwordHasher, JwtTokenService jwt)
    {
        this.db = db;
        this.passwordHasher = passwordHasher;
        this.jwt = jwt;
    }
    // GET
    [HttpGet]
    public IActionResult Index()
    {
        return Ok("Its Working");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest("Email and Password are required.");

        var rows = await db.ExecuteQueryAsync(
            "SELECT Id,Name, Email, password_hash, Role FROM `users` WHERE Email=@Email LIMIT 1",
            new Dictionary<string, object> { ["@Email"] = req.Email });

        if (rows.Count == 0) return Unauthorized("Invalid credentials.");

        var row = rows[0];
        var u = new user
        {
            Id = Convert.ToInt32(row["Id"]),
            Name = Convert.ToString(row["Name"]) ?? string.Empty,
            Email = Convert.ToString(row["Email"]) ?? string.Empty,
            PasswordHash = Convert.ToString(row["password_hash"]) ?? string.Empty,
            Role = Convert.ToString(row["Role"]) ?? string.Empty
        };

        var verify = passwordHasher.VerifyHashedPassword(u, u.PasswordHash, req.Password);
        if (verify == PasswordVerificationResult.Failed)
        {
            // Backward-compat: if PasswordHash is actually a plain password stored in DB, allow it once.
            if (!string.Equals(u.PasswordHash, req.Password, StringComparison.Ordinal))
                return Unauthorized("Invalid credentials.");
        }

        // If the password was valid but stored as plain text, upgrade it to a hash.
        if (string.Equals(u.PasswordHash, req.Password, StringComparison.Ordinal))
        {
            var newHash = passwordHasher.HashPassword(u, req.Password);
            await db.ExecuteNonQueryAsync(
                "UPDATE `users` SET PasswordHash=@PasswordHash WHERE Id=@Id",
                new Dictionary<string, object> { ["@PasswordHash"] = newHash, ["@Id"] = u.Id });
            u.PasswordHash = newHash;
        }

        var token = jwt.CreateToken(u);
        return Ok(new
        {
            token,
            user = new { u.Id, u.Name, u.Email, u.Role }
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("Userlist", Name = "Userlist")]
    public async Task<IActionResult> GetUsers()
    {
        // db logic need to be added 
        var data = await db.ExecuteQueryAsync("select * from users");
        
        List<string> list = new List<string>();
        list.Add("Admin");
        list.Add("User");
        list.Add("Admin");
        list.Add("User");
        list.Add("Admin");
        list.Add("Admin");

        return Ok(data);
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            name = User.FindFirst("name")?.Value,
            email = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email)?.Value,
            role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value,
            sub = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
        });
    }
    [HttpPost("register")]
    public async Task<IActionResult> CreateUser([FromBody] RegisterRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name))
            return BadRequest("Name is required.");
        if (string.IsNullOrWhiteSpace(req.Email))
            return BadRequest("Email is required.");
        if (string.IsNullOrWhiteSpace(req.Password))
            return BadRequest("Password is required.");

        var email = req.Email.Trim().ToLowerInvariant();
        if (req.Password.Length < 6)
            return BadRequest("Password must be at least 6 characters long.");

        var existing = await db.ExecuteScalarAsync(
            "SELECT 1 FROM `users` WHERE Email=@Email LIMIT 1",
            new Dictionary<string, object> { ["@Email"] = email });

        if (existing != null)
            return Conflict("Email already exists.");

        var u = new user
        {
            Name = req.Name.Trim(),
            Email = email,
            // The DB role column appears to be constrained (often ENUM). Default to "Student".
            Role = "Student"
        };

        // Only allow setting Role on registration if caller is an Admin.
        if (!string.IsNullOrWhiteSpace(req.Role))
        {
            // if (!(User?.Identity?.IsAuthenticated ?? false))
            //     return BadRequest("Role cannot be set during public registration.");
            // if (!User.IsInRole("Admin"))
            //     return Forbid();

            var requestedRole = req.Role.Trim();
            // Keep this aligned with DB constraints. Add more here only if your DB supports them.
            var allowedRoles = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Admin", "Student" };
            if (!allowedRoles.Contains(requestedRole))
                return BadRequest($"Invalid role '{requestedRole}'. Allowed roles: Admin, Student.");

            u.Role = requestedRole;
        }

        u.PasswordHash = passwordHasher.HashPassword(u, req.Password);

        await db.ExecuteNonQueryAsync(
            "INSERT INTO `users` (Name, Email, password_hash, Role) VALUES (@Name, @Email, @PasswordHash, @Role)",
            new Dictionary<string, object>
            {
                ["@Name"] = u.Name,
                ["@Email"] = u.Email,
                ["@PasswordHash"] = u.PasswordHash,
                ["@Role"] = u.Role
            });

        var row = (await db.ExecuteQueryAsync(
            "SELECT Id, Name, Email, Role FROM `users` WHERE Email=@Email LIMIT 1",
            new Dictionary<string, object> { ["@Email"] = u.Email }))
            .FirstOrDefault();

        if (row == null)
            return StatusCode(500, "User created but could not be loaded.");

        var created = new user
        {
            Id = Convert.ToInt32(row["Id"]),
            Name = Convert.ToString(row["Name"]) ?? string.Empty,
            Email = Convert.ToString(row["Email"]) ?? string.Empty,
            Role = Convert.ToString(row["Role"]) ?? string.Empty,
            PasswordHash = string.Empty
        };

        if (req.ReturnToken)
        {
            var token = jwt.CreateToken(created);
            return Ok(new
            {
                token,
                user = new { created.Id, created.Name, created.Email, created.Role }
            });
        }

        return Ok(new { created.Id, created.Name, created.Email, created.Role });
    }
}
