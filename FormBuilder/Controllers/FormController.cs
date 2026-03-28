using FormBuilder.Dtos;
using FormBuilder.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FormBuilder.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/my/forms")]
    public class FormController : ControllerBase
    {
        private readonly IFormService _formService;

        public FormController(IFormService formService)
        {
            _formService = formService;
        }

        private Guid GetUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdString, out Guid userId))
                return userId;
            throw new UnauthorizedAccessException("Invalid user token.");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FormDto>>> GetAll()
        {
            var forms = await _formService.GetFormsByUserAsync(GetUserId());
            return Ok(forms);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FormDto>> GetById(Guid id)
        {
            var form = await _formService.GetFormByIdAsync(id, GetUserId());
            return Ok(form);
        }

        [HttpPost]
        public async Task<ActionResult<FormDto>> Create([FromBody] CreateFormRequest request)
        {
            var form = await _formService.CreateFormAsync(request, GetUserId());
            return Ok(form);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<FormDto>> Update(Guid id, [FromBody] UpdateFormRequest request)
        {
            var form = await _formService.UpdateFormAsync(id, request, GetUserId());
            return Ok(form);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _formService.DeleteFormAsync(id, GetUserId());
            return NoContent();
        }

        [HttpPost("{id}/publish")]
        public async Task<IActionResult> Publish(Guid id)
        {
            await _formService.PublishFormAsync(id, GetUserId());
            return Ok(new { message = "Form published." });
        }

        [HttpPost("{id}/unpublish")]
        public async Task<IActionResult> Unpublish(Guid id)
        {
            await _formService.UnpublishFormAsync(id, GetUserId());
            return Ok(new { message = "Form unpublished." });
        }
    }
}
