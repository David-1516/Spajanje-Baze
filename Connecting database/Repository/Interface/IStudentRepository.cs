using System.Collections.Generic;
using System.Net.NetworkInformation;
using Collage.Common;
using System.Threading.Tasks;
using Connecting_database.Models;

namespace Collage.Repository.Interface
{
    public interface IStudentRepository
    {
        Task<int> CreateStudentAsync(Student student);
        Task<Student> GetStudentByIdAsync(int studentId);
        Task UpdateStudentAsync(Student student, int[] majorIds);
        Task DeleteStudentAsync(int studentId);
        Task AddStudentMajorsAsync(int studentId, int[] majorIds);
        Task<List<Student>> GetStudentsAsync(Filtering filtering, Sorting sorting, Paging paging);
    }
}
