using Collage.Repository.Interface;

using Connecting_database.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Collage.Common;

namespace Collage.Service
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _studentRepository;

        public StudentService(IStudentRepository studentRepository)
        {
            _studentRepository = studentRepository;
        }

        public async Task CreateStudentAsync(Student student, int[] majorIds)
        {
            var studentId = await _studentRepository.CreateStudentAsync(student);
            if (majorIds != null && majorIds.Length > 0)
            {
                await _studentRepository.AddStudentMajorsAsync(studentId, majorIds);
            }
        }

        public async Task<Student> GetStudentAsync(int studentId)
        {
            return await _studentRepository.GetStudentByIdAsync(studentId);
        }

        public async Task UpdateStudentAsync(Student student, int[] majorIds)
        {
            await _studentRepository.UpdateStudentAsync(student, majorIds);
        }

        public async Task DeleteStudentAsync(int studentId)
        {
            await _studentRepository.DeleteStudentAsync(studentId);
        }

        public async Task<List<Student>> GetStudentsAsync(Filtering filtering, Sorting sorting, Paging paging)
        {
            return await _studentRepository.GetStudentsAsync(filtering, sorting, paging);
        }
    }
}
