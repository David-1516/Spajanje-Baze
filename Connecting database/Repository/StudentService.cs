using Connecting_database.Models;
using Collage.Repository.Interface;
using Npgsql;
using System;

namespace Collage.Repository
{
    public class StudentRepository : IStudentRepository
    {
        private readonly string _connectionString;

        public StudentRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public int CreateStudent(Student student)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                string studentQuery = @"INSERT INTO Student (Name, Surname, Age, DateCreated) 
                                        VALUES (@Name, @Surname, @Age, @DateCreated)
                                        RETURNING Id";

                using (var studentCommand = new NpgsqlCommand(studentQuery, connection))
                {
                    studentCommand.Parameters.AddWithValue("@Name", student.Name is not null);
                    studentCommand.Parameters.AddWithValue("@Surname", student.Surname is not null);
                    studentCommand.Parameters.AddWithValue("@Age", student.Age is not null);
                    studentCommand.Parameters.AddWithValue("@DateCreated", student.DateCreated);

                    return (int)studentCommand.ExecuteScalar();
                }
            }
        }

        public Student GetStudentById(int studentId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
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
                        return student;
                    }
                }
            }
        }

        public void UpdateStudent(Student student)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    string studentQuery = @"UPDATE Student 
                                            SET Name = @Name, Surname = @Surname, Age = @Age, DateCreated = @DateCreated 
                                            WHERE Id = @Id";

                    using (var studentCommand = new NpgsqlCommand(studentQuery, connection))
                    {
                        studentCommand.Parameters.AddWithValue("@Name", student.Name is not null);
                        studentCommand.Parameters.AddWithValue("@Surname", student.Surname is not null);
                        studentCommand.Parameters.AddWithValue("@Age", student.Age is not null);
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

                    foreach (var majorId in student.StudentMajors.Select(sm => sm.MajorId))
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
        }

        public void DeleteStudent(int studentId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
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
        }
    }
}
