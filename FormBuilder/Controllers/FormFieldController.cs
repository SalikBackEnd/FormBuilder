using FormBuilder.Dtos;
using FormBuilder.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FormBuilder.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/my/forms/{formId}/fields")]
    public class FormFieldController : ControllerBase
    {
        private readonly IFormFieldService _fieldService;

        public FormFieldController(IFormFieldService fieldService)
        {
            _fieldService = fieldService;
        }

        private Guid GetUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdString, out Guid userId))
                return userId;
            throw new UnauthorizedAccessException("Invalid user token.");
        }

        [HttpPost]
        public async Task<ActionResult<FormFieldDto>> AddField(Guid formId, [FromBody] CreateFormFieldRequest request)
        {
            var field = await _fieldService.AddFieldAsync(formId, request, GetUserId());
            return Ok(field);
        }

        [HttpPut("{fieldId}")]
        public async Task<ActionResult<FormFieldDto>> UpdateField(Guid formId, Guid fieldId, [FromBody] UpdateFormFieldRequest request)
        {
            var field = await _fieldService.UpdateFieldAsync(formId, fieldId, request, GetUserId());
            return Ok(field);
        }

        [HttpDelete("{fieldId}")]
        public async Task<IActionResult> DeleteField(Guid formId, Guid fieldId)
        {
            await _fieldService.DeleteFieldAsync(formId, fieldId, GetUserId());
            return NoContent();
        }

        [HttpPost("reorder")]
        public async Task<IActionResult> ReorderFields(Guid formId, [FromBody] ReorderFieldsRequest request)
        {
            await _fieldService.ReorderFieldsAsync(formId, request, GetUserId());
            return Ok(new { message = "Fields reordered." });
        }
    }
}
