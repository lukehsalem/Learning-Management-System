using CLI.LMS.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.LMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoursesController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(CourseService.Current.GetAll());
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var course = CourseService.Current.GetById(id);
            if (course == null) return NotFound();
            return Ok(course);
        }

        [HttpPost]
        public IActionResult Create([FromBody] CreateCourseRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Code))
                return BadRequest("Name and Code are required.");

            var course = CourseService.Current.Add(request.Name, request.Code, request.Description ?? "");
            course.Semester = request.Semester;
            course.Section = request.Section;
            return CreatedAtAction(nameof(GetById), new { id = course.Id }, course);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] UpdateCourseRequest request)
        {
            var course = CourseService.Current.GetById(id);
            if (course == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(request.Name)) course.Name = request.Name;
            if (!string.IsNullOrWhiteSpace(request.Code)) course.Code = request.Code;
            if (request.Description != null) course.Description = request.Description;
            if (request.Semester != null) course.Semester = request.Semester;
            if (request.Section != null) course.Section = request.Section;

            return Ok(course);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var course = CourseService.Current.GetById(id);
            if (course == null) return NotFound();

            CourseService.Current.Delete(course);
            return NoContent();
        }

        // ISSUE-53: Search students in a course roster
        [HttpGet("{id}/students")]
        public IActionResult SearchStudents(int id, [FromQuery] string search)
        {
            var course = CourseService.Current.GetById(id);
            if (course == null) return NotFound();

            var roster = course.Roster.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.Trim().ToLower();
                roster = roster.Where(s =>
                    s.Name.ToLower().Contains(q) ||
                    s.Code.ToLower().Contains(q));
            }

            return Ok(roster.ToList());
        }

        // ISSUE-54: Update semester start/stop dates
        [HttpPut("{id}/semester")]
        public IActionResult UpdateSemester(int id, [FromBody] SemesterRequest request)
        {
            var course = CourseService.Current.GetById(id);
            if (course == null) return NotFound();

            course.StartDate = request.StartDate;
            course.EndDate = request.EndDate;

            return Ok(course);
        }
    }

    public class CreateCourseRequest
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string Semester { get; set; }
        public string Section { get; set; }
    }

    public class UpdateCourseRequest
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string Semester { get; set; }
        public string Section { get; set; }
    }

    public class SemesterRequest
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
