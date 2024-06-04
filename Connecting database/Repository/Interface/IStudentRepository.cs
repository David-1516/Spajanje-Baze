using Connecting_database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collage.Repository.Interface
{
    internal interface IStudentRepository
    {
        int CreateStudent(Student student);
        Student GetStudentById(int studentId);
        void UpdateStudent(Student student);
        void DeleteStudent(int studentId);
    }
}
