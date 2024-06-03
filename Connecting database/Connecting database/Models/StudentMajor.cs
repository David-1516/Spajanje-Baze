namespace Connecting_database.Models
{
    public class StudentMajor
    {
        public int StudentId { get; set; }
        public Student? Student { get; set; }

        public int MajorId { get; set; }
        public Major? Major { get; set; }
    }
}
