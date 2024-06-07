using AutoMapper;
using Connecting_database.Models;
using Connecting_database.RestModels;
using Microsoft.AspNetCore.Mvc;
using Collage.Service;
using Collage.Common;

namespace Connecting_database.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly IStudentService _studentService;
        private readonly IMapper _mapper;

        public HomeController(IStudentService studentService, IMapper mapper)
        {
            _studentService = studentService;
            _mapper = mapper;
        }

        [HttpPost]
        [Route("CreateStudent")]
        public async Task<IActionResult> CreateStudent([FromBody] CreateStudentRequest request)
        {
            var student = _mapper.Map<Student>(request);
            await _studentService.CreateStudentAsync(student, request.MajorIds);
            return Ok();
        }

        [HttpGet]
        [Route("GetStudent")]
        public async Task<IActionResult> GetStudent(int studentId)
        {
            var student = await _studentService.GetStudentAsync(studentId);
            if (student == null)
            {
                return NotFound();
            }
            var studentDto = _mapper.Map<StudentDto>(student);
            return Ok(studentDto);
        }

        [HttpPut]
        [Route("UpdateStudent")]
        public async Task<IActionResult> UpdateStudent([FromBody] UpdateStudentRequest request)
        {
            var student = _mapper.Map<Student>(request);
            await _studentService.UpdateStudentAsync(student, request.MajorIds);
            return Ok("Student updated successfully.");
        }

        [HttpDelete]
        [Route("DeleteStudent")]
        public async Task<IActionResult> DeleteStudent(int studentId)
        {
            await _studentService.DeleteStudentAsync(studentId);
            return Ok("Student deleted successfully.");
        }
        [HttpGet]
        [Route("GetStudents")]
        public async Task<IActionResult> GetStudents([FromQuery] Filtering filtering, [FromQuery] Sorting sorting, [FromQuery] Paging paging)
        {
            var students = await _studentService.GetStudentsAsync(filtering, sorting, paging);
            var studentDtos = _mapper.Map<List<StudentDto>>(students);
            return Ok(studentDtos);
        }
    }
}
