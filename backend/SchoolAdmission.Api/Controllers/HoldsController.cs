using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.Api.DTOs;
using SchoolAdmission.Api.Services.Interfaces;

namespace SchoolAdmission.Api.Controllers;

[ApiController]
[Produces("application/json")]
public sealed class HoldsController(IHoldService holds, TimeProvider clock) : ControllerBase
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

    [HttpGet("holds/{holdId:guid}")]
    [ProducesResponseType<HoldResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid holdId, CancellationToken ct)
    {
        var hold = await holds.GetAsync(holdId, ct);
        return Ok(hold.ToResponse(clock.GetUtcNow()));
    }

    [HttpPost("holds/{holdId:guid}/confirm")]
    [ProducesResponseType<HoldConfirmedResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Confirm(
        Guid holdId,
        [FromHeader(Name = StudentIdHeader)]
        [Required(ErrorMessage = StudentIdRequiredMessage)]
        [RegularExpression(StudentIdPattern, ErrorMessage = StudentIdFormatMessage)]
        string studentId,
        CancellationToken ct)
    {
        var hold = await holds.ConfirmAsync(holdId, studentId, ct);
        return Ok(hold.ToConfirmedResponse());
    }

    [HttpDelete("holds/{holdId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(
        Guid holdId,
        [FromHeader(Name = StudentIdHeader)]
        [Required(ErrorMessage = StudentIdRequiredMessage)]
        [RegularExpression(StudentIdPattern, ErrorMessage = StudentIdFormatMessage)]
        string studentId,
        CancellationToken ct)
    {
        await holds.CancelAsync(holdId, studentId, ct);
        return NoContent();
    }
}
