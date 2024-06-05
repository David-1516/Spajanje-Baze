using System;
using System.Threading.Tasks;
using Collage.Common;
using Collage.Repository.Interface;
using Connecting_database.Models;

namespace Collage.Service
{
    public class StudentService
    {
        private readonly IStudentRepository _studentRepository;

        public StudentService(IStudentRepository studentRepository)
        {
            _studentRepository = studentRepository;
        }

        public async Task<int> CreateStudentAsync(Student student, int[] majorIds)
        {
            int studentId = await _studentRepository.CreateStudentAsync(student);
            await _studentRepository.AddStudentMajorsAsync(studentId, majorIds);
            return studentId;
        }

        public async Task<Student> GetStudentByIdAsync(int studentId)
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
        public async Task<IEnumerable<Student>> GetStudentsAsync(Filtering filtering, Sorting sorting, Paging paging)
        {
            return await _studentRepository.GetStudentsAsync(filtering, sorting, paging);
        }
    }
}
