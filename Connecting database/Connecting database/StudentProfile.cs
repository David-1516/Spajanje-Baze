using AutoMapper;
using Connecting_database.Models;
using Connecting_database.RestModels;

public class StudentProfile : Profile
{
    public StudentProfile()
    {
        CreateMap<Student, StudentDto>().ReverseMap();
        CreateMap<Major, MajorDto>().ReverseMap();
        CreateMap<StudentMajor, StudentMajorDto>().ReverseMap();
        CreateMap<CreateStudentRequest, Student>();
        CreateMap<UpdateStudentRequest, Student>();
    }
}
