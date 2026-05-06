using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SAPWEbAPI_SOL.Data;
using SAPWEbAPI_SOL.Models;

namespace SAPWEbAPI_SOL.Controller;

[ApiController]
[Route("[controller]")]
[Authorize]
public class UserPreferencesController : ControllerBase
{
    private readonly DB_helper db;

    // Adjust this if your real table name differs.
    private const string Table = "user_preferences";

    public UserPreferencesController(DB_helper db)
    {
    
        this.db = db;
    }

    [HttpGet("{userId:int}")]
    public async Task<IActionResult> GetByUserId(int userId)
    {
        var rows = await db.ExecuteQueryAsync(
            $"SELECT UserId, DarkMode, Language FROM `{Table}` WHERE UserId=@UserId",
            new Dictionary<string, object> { ["@UserId"] = userId });

        return rows.Count == 0 ? NotFound() : Ok(rows[0]);
    }

    // Upsert semantics: create if missing, update if exists.
    [HttpPut("{userId:int}")]
    public async Task<IActionResult> Upsert(int userId, [FromBody] UserPreference pref)
    {
        if (pref.UserId != 0 && pref.UserId != userId) return BadRequest("UserId mismatch.");

        var affected = await db.ExecuteNonQueryAsync(
            $"INSERT INTO `{Table}` (UserId, DarkMode, Language) VALUES (@UserId, @DarkMode, @Language) " +
            "ON DUPLICATE KEY UPDATE DarkMode=@DarkMode, Language=@Language",
            new Dictionary<string, object>
            {
                ["@UserId"] = userId,
                ["@DarkMode"] = pref.DarkMode,
                ["@Language"] = pref.Language
            });

        return affected == 0 ? NotFound() : NoContent();
    }

    [HttpDelete("{userId:int}")]
    public async Task<IActionResult> Delete(int userId)
    {
        var affected = await db.ExecuteNonQueryAsync(
            $"DELETE FROM `{Table}` WHERE UserId=@UserId",
            new Dictionary<string, object> { ["@UserId"] = userId });

        return affected == 0 ? NotFound() : NoContent();
    }
}
