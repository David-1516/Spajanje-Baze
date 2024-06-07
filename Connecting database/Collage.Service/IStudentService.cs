using Collage.Common;
using Connecting_database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collage.Service
{
    public interface IStudentService
    {
        Task CreateStudentAsync(Student student, int[] majorIds);
        Task<Student> GetStudentAsync(int studentId);
        Task UpdateStudentAsync(Student student, int[] majorIds);
        Task DeleteStudentAsync(int studentId);
        Task<List<Student>> GetStudentsAsync(Filtering filtering, Sorting sorting, Paging paging);
    }
}
