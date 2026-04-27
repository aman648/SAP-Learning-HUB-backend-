using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using SAPWEbAPI_SOL.Data;
using System.Data;

namespace SAPWEbAPI_SOL.Controller;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly DB_helper db;
    
    public AuthController(DB_helper db)
    {
        this.db = db;
    }
    // GET
    [HttpGet]
    public IActionResult Index()
    {
        return Ok("Its Working");
    }

    [HttpGet("login", Name = "Login")]
 //name of the url is in first place an then Action name
    public IActionResult Login()
    {
        return Ok("Login  Working");
        
    }

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
}