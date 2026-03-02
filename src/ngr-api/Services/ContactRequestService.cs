using System.Security.Claims;
using NgrApi.Data;
using NgrApi.DTOs;
using NgrApi.Models;

namespace NgrApi.Services;

public interface IContactRequestService
{
    Task<ContactRequestDto> SubmitAsync(CreateContactRequestDto dto, ClaimsPrincipal user, string? programNumber);
}

public class ContactRequestService : IContactRequestService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ContactRequestService> _logger;

    public ContactRequestService(ApplicationDbContext context, ILogger<ContactRequestService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ContactRequestDto> SubmitAsync(CreateContactRequestDto dto, ClaimsPrincipal user, string? programNumber)
    {
        var email = user.FindFirstValue(ClaimTypes.Email)
                    ?? user.FindFirstValue("email")
                    ?? "unknown";
        var firstName = user.FindFirstValue(ClaimTypes.GivenName)
                        ?? user.FindFirstValue("given_name")
                        ?? "";
        var lastName = user.FindFirstValue(ClaimTypes.Surname)
                       ?? user.FindFirstValue("family_name")
                       ?? "";
        var name = $"{firstName} {lastName}".Trim();
        if (string.IsNullOrEmpty(name)) name = email;

        var referenceId = $"CR-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

        var request = new ContactRequest
        {
            ReferenceId = referenceId,
            Name = name,
            Email = email,
            ProgramNumber = programNumber,
            Subject = dto.Subject,
            Message = dto.Message,
            Status = "New",
            CreatedAt = DateTime.UtcNow,
        };

        _context.ContactRequests.Add(request);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Contact request {ReferenceId} submitted by {Email}", referenceId, email);

        return new ContactRequestDto
        {
            Id = request.Id,
            ReferenceId = request.ReferenceId,
            Name = request.Name,
            Email = request.Email,
            ProgramNumber = request.ProgramNumber,
            Subject = request.Subject,
            Message = request.Message,
            Status = request.Status,
            CreatedAt = request.CreatedAt,
        };
    }
}
