namespace SAPWEbAPI_SOL.Models;

public class Lesson
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Type { get; set; }

    public int ModuleId { get; set; }
    public Module Module { get; set; }
}