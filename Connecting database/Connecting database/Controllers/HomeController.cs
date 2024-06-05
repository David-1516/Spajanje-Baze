using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Collage.Service;
using Connecting_database.Models;
using System;
using System.Threading.Tasks;

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
        public async Task<IActionResult> CreateStudent([FromBody] Student student, [FromQuery] int[] majorIds)
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
        public async Task<IActionResult> GetStudent(int studentId)
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
        public async Task<IActionResult> UpdateStudent([FromBody] Student student, [FromQuery] int[] majorIds)
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
        public async Task<IActionResult> DeleteStudent(int studentId)
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
    }
}
