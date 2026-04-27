namespace SAPWEbAPI_SOL.Models;

public class Module
{
    public int Id { get; set; }
    public string Title { get; set; }

    public int CourseId { get; set; }
    public Course Course { get; set; }

    public List<Lesson> Lessons { get; set; }
}