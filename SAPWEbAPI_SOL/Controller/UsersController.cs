using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using SAPWEbAPI_SOL.Data;
using SAPWEbAPI_SOL.Models;

namespace SAPWEbAPI_SOL.Controller;

[ApiController]
[Route("[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly DB_helper db;
    private readonly IPasswordHasher<user> passwordHasher;
    private const string Table = "users";

    public UsersController(DB_helper db, IPasswordHasher<user> passwordHasher)
    {
        this.db = db;
        this.passwordHasher = passwordHasher;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var rows = await db.ExecuteQueryAsync(
            $"SELECT Id, Name, Email, PasswordHash, Role FROM `{Table}`",
            parameters: null);
        return Ok(rows);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var rows = await db.ExecuteQueryAsync(
            $"SELECT Id, Name, Email, PasswordHash, Role FROM `{Table}` WHERE Id=@Id",
            new Dictionary<string, object> { ["@Id"] = id });
        return rows.Count == 0 ? NotFound() : Ok(rows[0]);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] user u)
    {
        if (string.IsNullOrWhiteSpace(u.Email) || string.IsNullOrWhiteSpace(u.PasswordHash))
            return BadRequest("Email and PasswordHash are required. (For now, send the plain password in PasswordHash.)");

        // Backward-compat: callers currently send plain password in PasswordHash. Hash it if it doesn't look like an Identity hash.
        if (u.PasswordHash.Length < 50)
        {
            u.PasswordHash = passwordHasher.HashPassword(u, u.PasswordHash);
        }

        var affected = await db.ExecuteNonQueryAsync(
            $"INSERT INTO `{Table}` (Name, Email, PasswordHash, Role) VALUES (@Name, @Email, @PasswordHash, @Role)",
            new Dictionary<string, object>
            {
                ["@Name"] = u.Name,
                ["@Email"] = u.Email,
                ["@PasswordHash"] = u.PasswordHash,
                ["@Role"] = u.Role
            });

        if (affected <= 0) return BadRequest("Insert failed.");

        var newIdObj = await db.ExecuteScalarAsync("SELECT LAST_INSERT_ID()");
        var newId = newIdObj == null ? 0 : Convert.ToInt32(newIdObj);
        return CreatedAtAction(nameof(GetById), new { id = newId }, new { Id = newId });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] user u)
    {
        if (u.Id != 0 && u.Id != id) return BadRequest("Id mismatch.");

        if (!string.IsNullOrWhiteSpace(u.PasswordHash) && u.PasswordHash.Length < 50)
        {
            u.PasswordHash = passwordHasher.HashPassword(u, u.PasswordHash);
        }

        var affected = await db.ExecuteNonQueryAsync(
            $"UPDATE `{Table}` SET Name=@Name, Email=@Email, PasswordHash=@PasswordHash, Role=@Role WHERE Id=@Id",
            new Dictionary<string, object>
            {
                ["@Id"] = id,
                ["@Name"] = u.Name,
                ["@Email"] = u.Email,
                ["@PasswordHash"] = u.PasswordHash,
                ["@Role"] = u.Role
            });

        return affected == 0 ? NotFound() : NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var affected = await db.ExecuteNonQueryAsync(
            $"DELETE FROM `{Table}` WHERE Id=@Id",
            new Dictionary<string, object> { ["@Id"] = id });

        return affected == 0 ? NotFound() : NoContent();
    }
}
