using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SAPWEbAPI_SOL.Data;
using SAPWEbAPI_SOL.Models;

namespace SAPWEbAPI_SOL.Controller;

[ApiController]
[Route("[controller]")]
[Authorize]
public class ModulesController : ControllerBase
{
    private readonly DB_helper db;
    private const string Table = "modules";

    public ModulesController(DB_helper db)
    {
        this.db = db;
    }

    // GET /Modules?courseId=123
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? courseId)
    {
        if (courseId.HasValue)
        {
            var filtered = await db.ExecuteQueryAsync(
                $"SELECT Id, Title, CourseId FROM `{Table}` WHERE CourseId=@CourseId",
                new Dictionary<string, object> { ["@CourseId"] = courseId.Value });
            return Ok(filtered);
        }

        var rows = await db.ExecuteQueryAsync(
            $"SELECT Id, Title, CourseId FROM `{Table}`",
            parameters: null);
        return Ok(rows);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var rows = await db.ExecuteQueryAsync(
            $"SELECT Id, Title, CourseId FROM `{Table}` WHERE Id=@Id",
            new Dictionary<string, object> { ["@Id"] = id });
        return rows.Count == 0 ? NotFound() : Ok(rows[0]);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Module module)
    {
        var affected = await db.ExecuteNonQueryAsync(
            $"INSERT INTO `{Table}` (Title, CourseId) VALUES (@Title, @CourseId)",
            new Dictionary<string, object>
            {
                ["@Title"] = module.Title,
                ["@CourseId"] = module.CourseId
            });

        if (affected <= 0) return BadRequest("Insert failed.");

        var newIdObj = await db.ExecuteScalarAsync("SELECT LAST_INSERT_ID()");
        var newId = newIdObj == null ? 0 : Convert.ToInt32(newIdObj);
        return CreatedAtAction(nameof(GetById), new { id = newId }, new { Id = newId });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Module module)
    {
        if (module.Id != 0 && module.Id != id) return BadRequest("Id mismatch.");

        var affected = await db.ExecuteNonQueryAsync(
            $"UPDATE `{Table}` SET Title=@Title, CourseId=@CourseId WHERE Id=@Id",
            new Dictionary<string, object>
            {
                ["@Id"] = id,
                ["@Title"] = module.Title,
                ["@CourseId"] = module.CourseId
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
