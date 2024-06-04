using Collage.Repository;
using Collage.Repository.Interface;
using Connecting_database.Models;
using System.Collections.Generic;

namespace Collage.Service
{
    public class StudentService
    {
        private readonly StudentRepository _studentRepository;

        public StudentService(StudentRepository studentRepository)
        {
            _studentRepository = studentRepository;
        }

        public int CreateStudent(Student student, int[] majorIds)
        {
            int studentId = _studentRepository.CreateStudent(student);

            foreach (var majorId in majorIds)
            {
                var studentMajor = new StudentMajor
                {
                    StudentId = studentId,
                    MajorId = majorId
                };

                // Add logic to insert into StudentMajor table
            }

            return studentId;
        }

        public Student GetStudentById(int studentId)
        {
            return _studentRepository.GetStudentById(studentId);
        }

        public void UpdateStudent(Student student, int[] majorIds)
        {
            _studentRepository.UpdateStudent(student);

            foreach (var majorId in majorIds)
            {
                var studentMajor = new StudentMajor
                {
                    StudentId = student.Id,
                    MajorId = majorId
                };

                // Add logic to update StudentMajor table
            }
        }

        public void DeleteStudent(int studentId)
        {
            _studentRepository.DeleteStudent(studentId);
        }
    }
}
