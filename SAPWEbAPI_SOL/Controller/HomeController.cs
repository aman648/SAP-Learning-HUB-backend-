using Microsoft.AspNetCore.Mvc;
namespace SAPWEbAPI_SOL.Controller;



[ApiController]
[Route("[controller]")]
public class HomeController:ControllerBase
{
    // Support both:
    // GET /Home
    // GET /Home/Index
    [HttpGet]
    [HttpGet("Index", Name = "Index")]
    public IActionResult Index()
    {
        return Ok("Hello World!");
        
    }

    [HttpGet("make", Name = "make")]
    
    public IActionResult make()
    {
        return Ok("Hello World!1");
    }
    
    
}
