using Collage.Common;
using Collage.Repository.Interface;
using Connecting_database.Models;
using Npgsql;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            int studentId;
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = @"INSERT INTO Student (Name, Surname, Age, DateCreated) 
                              VALUES (@Name, @Surname, @Age, @DateCreated) 
                              RETURNING Id";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", student.Name is null);
                    command.Parameters.AddWithValue("@Surname", student.Surname is null);
                    command.Parameters.AddWithValue("@Age", student.Age is null);
                    command.Parameters.AddWithValue("@DateCreated", student.DateCreated.HasValue ? (object)student.DateCreated.Value : DBNull.Value);
                    studentId = (int)await command.ExecuteScalarAsync();
                }
            }

            return studentId;
        }

        public async Task<Student> GetStudentByIdAsync(int studentId)
        {
            Student student = null;
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = @"SELECT Id, Name, Surname, Age, DateCreated 
                              FROM Student WHERE Id = @StudentId";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@StudentId", studentId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            student = new Student
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Surname = reader.GetString(2),
                                Age = reader.GetString(3),
                                DateCreated = reader.GetDateTime(4)
                            };
                        }
                    }
                }
            }

            return student;
        }

        public async Task UpdateStudentAsync(Student student, int[] majorIds)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    var query = @"UPDATE Student 
                                  SET Name = @Name, Surname = @Surname, Age = @Age, DateCreated = @DateCreated 
                                  WHERE Id = @Id";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Name", student.Name is null);
                        command.Parameters.AddWithValue("@Surname", student.Surname is null);
                        command.Parameters.AddWithValue("@Age", student.Age is null);
                        command.Parameters.AddWithValue("@DateCreated", student.DateCreated.HasValue ? (object)student.DateCreated.Value : DBNull.Value);
                        command.Parameters.AddWithValue("@Id", student.Id);
                        await command.ExecuteNonQueryAsync();
                    }

                    var deleteMajorsQuery = @"DELETE FROM StudentMajor WHERE StudentId = @StudentId";
                    using (var deleteCommand = new NpgsqlCommand(deleteMajorsQuery, connection))
                    {
                        deleteCommand.Parameters.AddWithValue("@StudentId", student.Id);
                        await deleteCommand.ExecuteNonQueryAsync();
                    }

                    await AddStudentMajorsAsync(student.Id, majorIds);

                    await transaction.CommitAsync();
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
                    var deleteStudentMajorsQuery = @"DELETE FROM StudentMajor WHERE StudentId = @StudentId";
                    using (var deleteCommand = new NpgsqlCommand(deleteStudentMajorsQuery, connection))
                    {
                        deleteCommand.Parameters.AddWithValue("@StudentId", studentId);
                        await deleteCommand.ExecuteNonQueryAsync();
                    }

                    var deleteStudentQuery = @"DELETE FROM Student WHERE Id = @Id";
                    using (var deleteCommand = new NpgsqlCommand(deleteStudentQuery, connection))
                    {
                        deleteCommand.Parameters.AddWithValue("@Id", studentId);
                        await deleteCommand.ExecuteNonQueryAsync();
                    }

                    await transaction.CommitAsync();
                }
            }
        }

        public async Task AddStudentMajorsAsync(int studentId, int[] majorIds)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                foreach (var majorId in majorIds)
                {
                    var query = @"INSERT INTO StudentMajor (StudentId, MajorId) 
                                  VALUES (@StudentId, @MajorId)";
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@StudentId", studentId);
                        command.Parameters.AddWithValue("@MajorId", majorId);
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
        }

        public async Task<List<Student>> GetStudentsAsync(Filtering filtering, Sorting sorting, Paging paging)
        {
            var students = new List<Student>();
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = @"SELECT Id, Name, Surname, Age, DateCreated 
                              FROM Student 
                              WHERE (@StudentId IS NULL OR Id = @StudentId)
                              AND (@FromDate IS NULL OR DateCreated >= @FromDate)
                              AND (@ToDate IS NULL OR DateCreated <= @ToDate)
                              AND (@SearchQuery IS NULL OR (Name LIKE '%' || @SearchQuery || '%' OR Surname LIKE '%' || @SearchQuery || '%'))
                              ORDER BY 
                              CASE WHEN @OrderBy = 'Name' AND @SortOrder = 'asc' THEN Name END ASC,
                              CASE WHEN @OrderBy = 'Name' AND @SortOrder = 'desc' THEN Name END DESC,
                              CASE WHEN @OrderBy = 'DateCreated' AND @SortOrder = 'asc' THEN DateCreated END ASC,
                              CASE WHEN @OrderBy = 'DateCreated' AND @SortOrder = 'desc' THEN DateCreated END DESC
                              LIMIT @PageSize OFFSET @PageSize * (@PageNumber - 1)";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@StudentId", filtering.StudentId == 0 ? (object)DBNull.Value : filtering.StudentId);
                    command.Parameters.AddWithValue("@FromDate", filtering.FromDate ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@ToDate", filtering.ToDate ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@SearchQuery", filtering.SearchQuery ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@OrderBy", sorting.OrderBy ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@SortOrder", sorting.SortOrder ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@PageSize", paging.RppPageSize);
                    command.Parameters.AddWithValue("@PageNumber", paging.PageNumber);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var student = new Student
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Surname = reader.GetString(2),
                                Age = reader.GetString(3),
                                DateCreated = reader.GetDateTime(4)
                            };
                            students.Add(student);
                        }
                    }
                }
            }

            return students;
        }
    }
}
