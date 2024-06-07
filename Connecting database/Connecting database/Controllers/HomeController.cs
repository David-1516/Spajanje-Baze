using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Connecting_database.Models;
using Collage.Common;
using Collage.Service;

namespace Collage.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public HomeController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        [HttpGet("students")]
        public async Task<IActionResult> GetStudents([FromQuery] int studentId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, [FromQuery] string searchQuery, [FromQuery] string sortOrder, [FromQuery] string orderBy, [FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            var filtering = new Filtering
            {
                StudentId = studentId,
                FromDate = fromDate,
                ToDate = toDate,
                SearchQuery = searchQuery
            };

            var sorting = new Sorting
            {
                SortOrder = sortOrder,
                OrderBy = orderBy
            };

            var paging = new Paging
            {
                PageNumber = pageNumber,
                RppPageSize = pageSize
            };

            var students = await _studentService.GetStudentsAsync(filtering, sorting, paging);
            return Ok(students);
        }

        [HttpPost("student")]
        public async Task<IActionResult> CreateStudent([FromBody] Student student, [FromQuery] int[] majorIds)
        {
            await _studentService.CreateStudentAsync(student, majorIds);
            return CreatedAtAction(nameof(GetStudent), new { studentId = student.Id }, student);
        }

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetStudent(int studentId)
        {
            var student = await _studentService.GetStudentAsync(studentId);
            if (student == null)
            {
                return NotFound();
            }

            return Ok(student);
        }

        [HttpPut("student/{studentId}")]
        public async Task<IActionResult> UpdateStudent(int studentId, [FromBody] Student student, [FromQuery] int[] majorIds)
        {
            if (studentId != student.Id)
            {
                return BadRequest();
            }

            await _studentService.UpdateStudentAsync(student, majorIds);
            return NoContent();
        }

        [HttpDelete("student/{studentId}")]
        public async Task<IActionResult> DeleteStudent(int studentId)
        {
            await _studentService.DeleteStudentAsync(studentId);
            return NoContent();
        }
    }
}
