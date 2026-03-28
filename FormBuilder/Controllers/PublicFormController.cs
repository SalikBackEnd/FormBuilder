using FormBuilder.Dtos;
using FormBuilder.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FormBuilder.Controllers
{
    [ApiController]
    [Route("api/public/forms")]
    public class PublicFormController : ControllerBase
    {
        private readonly IFormService _formService;
        private readonly IFormSubmissionService _submissionService;

        public PublicFormController(IFormService formService, IFormSubmissionService submissionService)
        {
            _formService = formService;
            _submissionService = submissionService;
        }

        [HttpGet("{slug}")]
        public async Task<ActionResult<FormDto>> GetBySlug(string slug)
        {
            var form = await _formService.GetFormBySlugAsync(slug);
            return Ok(form);
        }

        [HttpPost("{slug}/submit")]
        public async Task<ActionResult<object>> Submit(string slug, [FromBody] SubmitFormRequest request)
        {
            await _submissionService.SubmitFormAsync(slug, request);
            return Ok(new { message = "Form submitted successfully." });
        }
    }
}
