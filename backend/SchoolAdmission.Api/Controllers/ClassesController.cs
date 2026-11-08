using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.Api.DTOs;
using SchoolAdmission.Api.Services.Interfaces;

namespace SchoolAdmission.Api.Controllers;

[ApiController]
[Route("classes")]
[Produces("application/json")]
public sealed class ClassesController(IClassService classes) : ControllerBase
{
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType<ClassCreatedResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateClassRequest request, CancellationToken ct)
    {
        var created = await classes.CreateAsync(request, ct);
        return Created($"/classes/{created.Id}", created.ToCreatedResponse());
    }

    [HttpGet]
    [ProducesResponseType<IEnumerable<ClassResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var items = await classes.ListAsync(ct);
        return Ok(items.Select(x => x.ToResponse()));
    }

    [HttpGet("{classId:guid}")]
    [ProducesResponseType<ClassResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid classId, CancellationToken ct)
    {
        var item = await classes.GetAsync(classId, ct);
        return Ok(item.ToResponse());
    }
}
