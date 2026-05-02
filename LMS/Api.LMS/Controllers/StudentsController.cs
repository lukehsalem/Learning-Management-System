using CLI.LMS.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.LMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentsController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(StudentService.Current.GetAll());
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var student = StudentService.Current.GetById(id);
            if (student == null) return NotFound();
            return Ok(student);
        }

        [HttpPost]
        public IActionResult Create([FromBody] CreateStudentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Code))
                return BadRequest("Name and Code are required.");

            var student = StudentService.Current.Add(request.Name, request.Code, request.Classification ?? "");
            return CreatedAtAction(nameof(GetById), new { id = student.Id }, student);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] UpdateStudentRequest request)
        {
            var student = StudentService.Current.GetById(id);
            if (student == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(request.Name)) student.Name = request.Name;
            if (!string.IsNullOrWhiteSpace(request.Code)) student.Code = request.Code;
            if (request.Classification != null) student.Classification = request.Classification;

            return Ok(student);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var student = StudentService.Current.GetById(id);
            if (student == null) return NotFound();

            foreach (var course in CourseService.Current.GetAll())
            {
                course.Roster.RemoveAll(s => s.Id == id);
                foreach (var assignment in course.Assignments)
                    assignment.Submissions.RemoveAll(s => s.StudentId == id);
            }

            StudentService.Current.Delete(student);
            return NoContent();
        }
    }

    public class CreateStudentRequest
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Classification { get; set; }
    }

    public class UpdateStudentRequest
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Classification { get; set; }
    }
}
