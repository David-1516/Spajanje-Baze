using System.Collections.Generic;
using System.Threading.Tasks;
using Connecting_database.Models;

namespace Collage.Repository.Interface
{
    public interface IStudentRepository
    {
        Task<int> CreateStudentAsync(Student student);
        Task AddStudentMajorsAsync(int studentId, int[] majorIds);
        Task<Student> GetStudentByIdAsync(int studentId);
        Task UpdateStudentAsync(Student student, int[] majorIds);
        Task DeleteStudentAsync(int studentId);
    }
}
