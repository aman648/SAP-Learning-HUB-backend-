using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SAPWEbAPI_SOL.Data;
using SAPWEbAPI_SOL.Models;

namespace SAPWEbAPI_SOL.Controller;

[ApiController]
[Route("[controller]")]
[Authorize]
public class CoursesController : ControllerBase
{
    private readonly DB_helper db;

    // Adjust this if your real table name differs.
    private const string Table = "courses";

    public CoursesController(DB_helper db)
    {
        this.db = db;
    }
    
    [HttpGet(template:"GetAll",Name = "GetAll")]
    public async Task<IActionResult> GetAll()
    {
        var rows = await db.ExecuteQueryAsync($"SELECT title,description,category,level  FROM `{Table}`");
        if (rows.Count == 0) return NotFound();
           
        return Ok(rows);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var rows = await db.ExecuteQueryAsync(
            $"SELECT Id, Title, Category, CreatedBy FROM `courses` WHERE Id = @Id",
            new Dictionary<string, object> { ["@Id"] = id });

        return rows.Count == 0 ? NotFound() : Ok(rows[0]);
    }
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Course course)
    {
        var affected = await db.ExecuteNonQueryAsync(
            $"INSERT INTO `{Table}` (Title, Category, CreatedBy) VALUES (@Title, @Category, @CreatedBy)",
            new Dictionary<string, object>
            {
                ["@Title"] = course.Title,
                ["@Category"] = course.Category,
                ["@CreatedBy"] = course.CreatedBy
            });

        if (affected <= 0) return BadRequest("Insert failed.");

        var newIdObj = await db.ExecuteScalarAsync("SELECT LAST_INSERT_ID()");
        var newId = newIdObj == null ? 0 : Convert.ToInt32(newIdObj);
        return CreatedAtAction(nameof(GetById), new { id = newId }, new { Id = newId });
    }
 
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Course course)
    {
        if (course.Id != 0 && course.Id != id) return BadRequest("Id mismatch.");

        var affected = await db.ExecuteNonQueryAsync(
            $"UPDATE `{Table}` SET Title=@Title, Category=@Category, CreatedBy=@CreatedBy WHERE Id=@Id",
            new Dictionary<string, object>
            {
                ["@Id"] = id,
                ["@Title"] = course.Title,
                ["@Category"] = course.Category,
                ["@CreatedBy"] = course.CreatedBy
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
