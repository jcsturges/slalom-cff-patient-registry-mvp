using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NgrApi.DTOs;
using NgrApi.Services;

namespace NgrApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ContactController : ControllerBase
{
    private readonly IContactRequestService _service;
    private readonly ILogger<ContactController> _logger;

    public ContactController(IContactRequestService service, ILogger<ContactController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>Submit a contact/support request</summary>
    [HttpPost]
    public async Task<ActionResult<ContactRequestDto>> Submit([FromBody] CreateContactRequestDto dto)
    {
        var result = await _service.SubmitAsync(dto, User, programNumber: null);
        return CreatedAtAction(null, new { id = result.Id }, result);
    }
}
