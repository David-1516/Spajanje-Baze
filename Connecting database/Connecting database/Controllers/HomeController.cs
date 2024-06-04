using Connecting_database.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using Npgsql;
using Collage.Service;


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
        public IActionResult CreateStudent([FromBody] Student student, [FromQuery] int[] majorIds)
        {
            try
            {
                int studentId = _studentService.CreateStudent(student, majorIds);
                return Ok("Student created successfully with ID: " + studentId);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet]
        [Route("GetStudent")]
        public IActionResult GetStudent(int studentId)
        {
            try
            {
                var student = _studentService.GetStudentById(studentId);
                return Ok(student);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut]
        [Route("UpdateStudent")]
        public IActionResult UpdateStudent([FromBody] Student student, [FromQuery] int[] majorIds)
        {
            try
            {
                _studentService.UpdateStudent(student, majorIds);
                return Ok("Student updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete]
        [Route("DeleteStudent")]
        public IActionResult DeleteStudent(int studentId)
        {
            try
            {
                _studentService.DeleteStudent(studentId);
                return Ok("Student deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}


    
