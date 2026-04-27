namespace SAPWEbAPI_SOL.Models;

public class Course
{
    
        public int Id { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }

        public int CreatedBy { get; set; }

        public List<Module> Modules { get; set; }
}