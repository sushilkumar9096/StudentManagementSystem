using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Student.Core.Common;
using Student.Core.DTOs;
using Student.Core.Interfaces;

namespace Student.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentsController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        /// <summary>
        /// Get all students (Supports search and filtering)
        /// </summary>
        /// <param name="searchTerm">Filter by name or email</param>
        /// <param name="course">Filter by course</param>
        /// <returns>List of students</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<StudentDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] string? searchTerm, [FromQuery] string? course)
        {
            var result = await _studentService.GetAllStudentsAsync(searchTerm, course);
            return Ok(result);
        }

        /// <summary>
        /// Get student by unique ID
        /// </summary>
        /// <param name="id">Student ID</param>
        /// <returns>Student details</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<StudentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _studentService.GetStudentByIdAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// Add a new student record
        /// </summary>
        /// <param name="createDto">Student creation data</param>
        /// <returns>Created student details</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<StudentDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] CreateStudentDto createDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.FailureResponse("Validation failed", errors, 400));
            }

            var result = await _studentService.CreateStudentAsync(createDto);
            return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
        }

        /// <summary>
        /// Update an existing student record
        /// </summary>
        /// <param name="id">Student ID</param>
        /// <param name="updateDto">Updated student data</param>
        /// <returns>Updated student details</returns>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<StudentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateStudentDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.FailureResponse("Validation failed", errors, 400));
            }

            var result = await _studentService.UpdateStudentAsync(id, updateDto);
            return Ok(result);
        }

        /// <summary>
        /// Delete a student by ID
        /// </summary>
        /// <param name="id">Student ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _studentService.DeleteStudentAsync(id);
            return Ok(result);
        }
    }
}
