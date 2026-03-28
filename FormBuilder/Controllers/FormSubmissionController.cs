using FormBuilder.Dtos;
using FormBuilder.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FormBuilder.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/my/forms/{id}/submissions")]
    public class FormSubmissionController : ControllerBase
    {
        private readonly IFormSubmissionService _submissionService;

        public FormSubmissionController(IFormSubmissionService submissionService)
        {
            _submissionService = submissionService;
        }

        private Guid GetUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdString, out Guid userId))
                return userId;
            throw new UnauthorizedAccessException("Invalid user token.");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FormSubmissionDto>>> GetSubmissions(Guid id)
        {
            var submissions = await _submissionService.GetSubmissionsAsync(id, GetUserId());
            return Ok(submissions);
        }

        [HttpGet("{submissionId}")]
        public async Task<ActionResult<FormSubmissionDto>> GetSubmissionById(Guid id, Guid submissionId)
        {
            var submission = await _submissionService.GetSubmissionByIdAsync(id, submissionId, GetUserId());
            return Ok(submission);
        }
    }
}
