namespace Connecting_database.RestModels
{
    public class CreateStudentRequest
    {
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Age { get; set; }
        public DateTime? DateCreated { get; set; }
        public int[]? MajorIds { get; set; }
    }
}
