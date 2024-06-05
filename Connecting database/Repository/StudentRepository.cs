using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Collage.Repository.Interface;
using Connecting_database.Models;
using Npgsql;

namespace Collage.Repository
{
    public class StudentRepository : IStudentRepository
    {
        private readonly string _connectionString;

        public StudentRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<int> CreateStudentAsync(Student student)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        string studentQuery = @"INSERT INTO Student (Name, Surname, Age, DateCreated) 
                                                VALUES (@Name, @Surname, @Age, @DateCreated)
                                                RETURNING Id";

                        using (var studentCommand = new NpgsqlCommand(studentQuery, connection))
                        {
                            studentCommand.Parameters.AddWithValue("@Name", student.Name);
                            studentCommand.Parameters.AddWithValue("@Surname", student.Surname);
                            studentCommand.Parameters.AddWithValue("@Age", student.Age);
                            studentCommand.Parameters.AddWithValue("@DateCreated", student.DateCreated);

                            var studentId = (int)await studentCommand.ExecuteScalarAsync();
                            await transaction.CommitAsync();
                            return studentId;
                        }
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }

        public async Task AddStudentMajorsAsync(int studentId, int[] majorIds)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        foreach (var majorId in majorIds)
                        {
                            string studentMajorQuery = @"INSERT INTO StudentMajor (StudentId, MajorId) 
                                                         VALUES (@StudentId, @MajorId)";

                            using (var studentMajorCommand = new NpgsqlCommand(studentMajorQuery, connection))
                            {
                                studentMajorCommand.Parameters.AddWithValue("@StudentId", studentId);
                                studentMajorCommand.Parameters.AddWithValue("@MajorId", majorId);
                                await studentMajorCommand.ExecuteNonQueryAsync();
                            }
                        }

                        await transaction.CommitAsync();
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }

        public async Task<Student> GetStudentByIdAsync(int studentId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
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

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        var student = new Student();
                        var majors = new List<Major>();

                        while (await reader.ReadAsync())
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

        public async Task UpdateStudentAsync(Student student, int[] majorIds)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
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

                            await studentCommand.ExecuteNonQueryAsync();
                        }

                        string deleteMajorsQuery = @"DELETE FROM StudentMajor WHERE StudentId = @StudentId";
                        using (var deleteCommand = new NpgsqlCommand(deleteMajorsQuery, connection))
                        {
                            deleteCommand.Parameters.AddWithValue("@StudentId", student.Id);
                            await deleteCommand.ExecuteNonQueryAsync();
                        }

                        await AddStudentMajorsAsync(student.Id, majorIds);

                        await transaction.CommitAsync();
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }

        public async Task DeleteStudentAsync(int studentId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        string deleteStudentMajorsQuery = @"DELETE FROM StudentMajor WHERE StudentId = @StudentId";
                        using (var deleteStudentMajorsCommand = new NpgsqlCommand(deleteStudentMajorsQuery, connection))
                        {
                            deleteStudentMajorsCommand.Parameters.AddWithValue("@StudentId", studentId);
                            await deleteStudentMajorsCommand.ExecuteNonQueryAsync();
                        }

                        string deleteStudentQuery = @"DELETE FROM Student WHERE Id = @Id";
                        using (var deleteStudentCommand = new NpgsqlCommand(deleteStudentQuery, connection))
                        {
                            deleteStudentCommand.Parameters.AddWithValue("@Id", studentId);
                            await deleteStudentCommand.ExecuteNonQueryAsync();
                        }

                        await transaction.CommitAsync();
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }
    }
}
