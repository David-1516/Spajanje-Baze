using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using Connecting_database.Models;

namespace Connecting_database.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        [Route("CreateStudent")]
        public IActionResult CreateStudent(Student student, int[] majorIds)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    using (var transaction = connection.BeginTransaction())
                    {
                        string studentQuery = @"INSERT INTO Student (Name, Surname, Age, DateCreated) 
                                                VALUES (@Name, @Surname, @Age, @DateCreated)
                                                RETURNING Id";

                        int studentId;
                        using (var studentCommand = new NpgsqlCommand(studentQuery, connection))
                        {
                            studentCommand.Parameters.AddWithValue("@Name", student.Name);
                            studentCommand.Parameters.AddWithValue("@Surname", student.Surname);
                            studentCommand.Parameters.AddWithValue("@Age", student.Age);
                            studentCommand.Parameters.AddWithValue("@DateCreated", student.DateCreated);

                            studentId = (int)studentCommand.ExecuteScalar();
                        }

                        foreach (var majorId in majorIds)
                        {
                            string studentMajorQuery = @"INSERT INTO StudentMajor (StudentId, MajorId) 
                                                         VALUES (@StudentId, @MajorId)";

                            using (var studentMajorCommand = new NpgsqlCommand(studentMajorQuery, connection))
                            {
                                studentMajorCommand.Parameters.AddWithValue("@StudentId", studentId);
                                studentMajorCommand.Parameters.AddWithValue("@MajorId", majorId);
                                studentMajorCommand.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                }

                return Ok("Student created successfully.");
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
