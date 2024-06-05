using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Collage.Service;
using Connecting_database.Models;
using System;
using System.Threading.Tasks;
using Collage.Common;

namespace Connecting_database.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly StudentService _studentService;

        public HomeController(IConfiguration configuration, StudentService studentService)
        {
            _configuration = configuration;
            _studentService = studentService;
        }

        [HttpPost]
        [Route("CreateStudent")]
        public async Task<IActionResult> CreateStudentAsync([FromBody] Student student, [FromQuery] int[] majorIds)
        {
            try
            {
                int studentId = await _studentService.CreateStudentAsync(student, majorIds);
                return Ok("Student created successfully with ID: " + studentId);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("GetStudent")]
        public async Task<IActionResult> GetStudentAsync(int studentId)
        {
            try
            {
                var student = await _studentService.GetStudentByIdAsync(studentId);
                return Ok(student);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut]
        [Route("UpdateStudent")]
        public async Task<IActionResult> UpdateStudentAsync([FromBody] Student student, [FromQuery] int[] majorIds)
        {
            try
            {
                await _studentService.UpdateStudentAsync(student, majorIds);
                return Ok("Student updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete]
        [Route("DeleteStudent")]
        public async Task<IActionResult> DeleteStudentAsync(int studentId)
        {
            try
            {
                await _studentService.DeleteStudentAsync(studentId);
                return Ok("Student deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("GetStudents")]
        public async Task<IActionResult> GetStudents( [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] int studentId = 1, [FromQuery] string searchQuery = "", [FromQuery] string sortOrder = "ASC", [FromQuery] string orderBy ="Newest", [FromQuery] int rppPageSize = 10, [FromQuery] int pageNumber = 1)
        {
            try
            {
                var filtering = new Filtering
                {
                    StudentId = studentId,
                    FromDate = fromDate,
                    ToDate = toDate,
                    SearchQery = searchQuery
                };

                var sorting = new Sorting
                {
                    SortOrder = sortOrder,
                    OrderBy = orderBy
                };

                var paging = new Paging
                {
                    RppPageSize = rppPageSize,
                    PageNumber = pageNumber
                };

                var students = await _studentService.GetStudentsAsync(filtering, sorting, paging);
                return Ok(students);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
