using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.Api.DTOs;
using SchoolAdmission.Api.Services.Interfaces;

namespace SchoolAdmission.Api.Controllers;

[ApiController]
[Produces("application/json")]
public sealed class HoldsController(IHoldService holds) : ControllerBase
{
    public const string StudentIdHeader = "X-Student-Id";
    // [0-9] (not \d) so only ASCII digits pass. Repeated headers bind as "a,b" and fail too.
    public const string StudentIdPattern = "^[0-9]{17}$";
    public const string StudentIdRequiredMessage = "X-Student-Id header is required.";
    public const string StudentIdFormatMessage = "X-Student-Id must be exactly 17 digits.";

    [HttpPost("classes/{classId:guid}/holds")]
    [ProducesResponseType<HoldCreatedResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        Guid classId,
        [FromHeader(Name = StudentIdHeader)]
        [Required(ErrorMessage = StudentIdRequiredMessage)]
        [RegularExpression(StudentIdPattern, ErrorMessage = StudentIdFormatMessage)]
        string studentId,
        CancellationToken ct)
    {
        var hold = await holds.CreateAsync(classId, studentId, ct);
        return Created($"/holds/{hold.Id}", hold.ToCreatedResponse());
    }
}
