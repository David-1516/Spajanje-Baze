namespace Connecting_database.Models
{
    public class Major
    {
        public int Id { get; set; }
        public string? Subject { get; set; }
        public string? Teacher { get; set; }
        public List<StudentMajor> StudentMajors { get; set; } = new List<StudentMajor>();
    }
}
