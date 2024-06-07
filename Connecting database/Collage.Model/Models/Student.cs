using System.Reflection.Metadata.Ecma335;

namespace Connecting_database.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string?  Surname { get; set; }
        public string?  Age { get; set; }
        public int MajorId { get; set; }
        public DateTime DateCreated { get; set; }

        public List<StudentMajor> StudentMajors { get; set; } = new List<StudentMajor>();
    }
}
