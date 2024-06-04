using Connecting_database.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;


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
        public IActionResult CreateStudent([FromBody] Student student, [FromQuery] int[] majorIds)
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

        [HttpGet]
        [Route("GetStudent")]
        public IActionResult GetStudent(int studentId)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using (var connection = new NpgsqlConnection())
                {
                    connection.Open();

                    string query = @"
                        SELECT s.Id, s.Name, s.Surname, s.Age, s.DateCreated, 
                               m.Id as MajorId, m.Subject, m.Teacher
                        FROM Student s
                        LEFT JOIN StudentMajor sm ON s.Id = sm.StudentId
                        LEFT JOIN Major m ON sm.MajorId = m.Id
                        WHERE s.Id = @StudentId";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@StudentId", studentId);

                        using (var reader = command.ExecuteReader())
                        {
                            var student = new Student();
                            var majors = new List<Major>();

                            while (reader.Read())
                            {
                                if (student.Id == 0)
                                {
                                    student.Id = reader.GetInt32(0);
                                    student.Name = reader.GetString(1);
                                    student.Surname = reader.GetString(2);
                                    student.Age = reader.GetString(3);
                                    student.DateCreated = reader.GetDateTime(4);
                                }

                                if (!reader.IsDBNull(5))
                                {
                                    var major = new Major
                                    {
                                        Id = reader.GetInt32(5),
                                        Subject = reader.GetString(6),
                                        Teacher = reader.GetString(7)
                                    };
                                    majors.Add(major);
                                }
                            }

                            student.StudentMajors = majors.Select(m => new StudentMajor { Major = m, MajorId = m.Id, StudentId = student.Id, Student = student }).ToList();
                            return Ok(student);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }

        [HttpPut]
        [Route("UpdateStudent")]
        public IActionResult UpdateStudent([FromBody] Student student, [FromQuery]int[] majorIds)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    using (var transaction = connection.BeginTransaction())
                    {
                        string studentQuery = @"UPDATE Student 
                                        SET Name = @Name, Surname = @Surname, Age = @Age, DateCreated = @DateCreated 
                                        WHERE Id = @Id";

                        using (var studentCommand = new NpgsqlCommand(studentQuery, connection))
                        {
                            studentCommand.Parameters.AddWithValue("@Name", student.Name);
                            studentCommand.Parameters.AddWithValue("@Surname", student.Surname);
                            studentCommand.Parameters.AddWithValue("@Age", student.Age);
                            studentCommand.Parameters.AddWithValue("@DateCreated", student.DateCreated);
                            studentCommand.Parameters.AddWithValue("@Id", student.Id);

                            studentCommand.ExecuteNonQuery();
                        }

                        string deleteMajorsQuery = @"DELETE FROM StudentMajor WHERE StudentId = @StudentId";
                        using (var deleteCommand = new NpgsqlCommand(deleteMajorsQuery, connection))
                        {
                            deleteCommand.Parameters.AddWithValue("@StudentId", student.Id);
                            deleteCommand.ExecuteNonQuery();
                        }

                        foreach (var majorId in majorIds)
                        {
                            string studentMajorQuery = @"INSERT INTO StudentMajor (StudentId, MajorId) 
                                                 VALUES (@StudentId, @MajorId)";

                            using (var studentMajorCommand = new NpgsqlCommand(studentMajorQuery, connection))
                            {
                                studentMajorCommand.Parameters.AddWithValue("@StudentId", student.Id);
                                studentMajorCommand.Parameters.AddWithValue("@MajorId", majorId);
                                studentMajorCommand.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                }

                return Ok("Student updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete]
        [Route("DeleteStudent")]
        public IActionResult DeleteStudent(int studentId)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    using (var transaction = connection.BeginTransaction())
                    {
                        string deleteStudentMajorsQuery = @"DELETE FROM StudentMajor WHERE StudentId = @StudentId";
                        using (var deleteStudentMajorsCommand = new NpgsqlCommand(deleteStudentMajorsQuery, connection))
                        {
                            deleteStudentMajorsCommand.Parameters.AddWithValue("@StudentId", studentId);
                            deleteStudentMajorsCommand.ExecuteNonQuery();
                        }

                        string deleteStudentQuery = @"DELETE FROM Student WHERE Id = @Id";
                        using (var deleteStudentCommand = new NpgsqlCommand(deleteStudentQuery, connection))
                        {
                            deleteStudentCommand.Parameters.AddWithValue("@Id", studentId);
                            deleteStudentCommand.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                }

                return Ok("Student deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


    }
}
