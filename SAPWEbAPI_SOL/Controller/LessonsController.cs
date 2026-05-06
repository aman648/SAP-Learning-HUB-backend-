using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SAPWEbAPI_SOL.Data;
using SAPWEbAPI_SOL.Models;

namespace SAPWEbAPI_SOL.Controller;

[ApiController]
[Route("[controller]")]
[Authorize]
public class LessonsController : ControllerBase
{
    private readonly DB_helper db;
    private const string Table = "lessons";

    public LessonsController(DB_helper db)
    {
        this.db = db;
    }

    // GET /Lessons?moduleId=456
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? moduleId)
    {
        if (moduleId.HasValue)
        {
            var filtered = await db.ExecuteQueryAsync(
                $"SELECT Id, Title, Type, ModuleId FROM `{Table}` WHERE ModuleId=@ModuleId",
                new Dictionary<string, object> { ["@ModuleId"] = moduleId.Value });
            return Ok(filtered);
        }

        var rows = await db.ExecuteQueryAsync(
            $"SELECT Id, Title, Type, ModuleId FROM `{Table}`",
            parameters: null);
        return Ok(rows);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var rows = await db.ExecuteQueryAsync(
            $"SELECT Id, Title, Type, ModuleId FROM `{Table}` WHERE Id=@Id",
            new Dictionary<string, object> { ["@Id"] = id });
        return rows.Count == 0 ? NotFound() : Ok(rows[0]);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Lesson lesson)
    {
        var affected = await db.ExecuteNonQueryAsync(
            $"INSERT INTO `{Table}` (Title, Type, ModuleId) VALUES (@Title, @Type, @ModuleId)",
            new Dictionary<string, object>
            {
                ["@Title"] = lesson.Title,
                ["@Type"] = lesson.Type,
                ["@ModuleId"] = lesson.ModuleId
            });

        if (affected <= 0) return BadRequest("Insert failed.");

        var newIdObj = await db.ExecuteScalarAsync("SELECT LAST_INSERT_ID()");
        var newId = newIdObj == null ? 0 : Convert.ToInt32(newIdObj);
        return CreatedAtAction(nameof(GetById), new { id = newId }, new { Id = newId });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Lesson lesson)
    {
        if (lesson.Id != 0 && lesson.Id != id) return BadRequest("Id mismatch.");

        var affected = await db.ExecuteNonQueryAsync(
            $"UPDATE `{Table}` SET Title=@Title, Type=@Type, ModuleId=@ModuleId WHERE Id=@Id",
            new Dictionary<string, object>
            {
                ["@Id"] = id,
                ["@Title"] = lesson.Title,
                ["@Type"] = lesson.Type,
                ["@ModuleId"] = lesson.ModuleId
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
