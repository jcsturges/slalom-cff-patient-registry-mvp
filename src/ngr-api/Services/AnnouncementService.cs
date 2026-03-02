using Microsoft.EntityFrameworkCore;
using NgrApi.Data;
using NgrApi.DTOs;
using NgrApi.Models;

namespace NgrApi.Services;

public interface IAnnouncementService
{
    Task<IEnumerable<AnnouncementDto>> GetAllAsync(bool includeInactive = false);
    Task<IEnumerable<AnnouncementDto>> GetActiveAsync();
    Task<AnnouncementDto?> GetByIdAsync(int id);
    Task<AnnouncementDto> CreateAsync(CreateAnnouncementDto dto, string createdBy);
    Task<AnnouncementDto?> UpdateAsync(int id, UpdateAnnouncementDto dto);
    Task<bool> DeleteAsync(int id);
}

public class AnnouncementService : IAnnouncementService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AnnouncementService> _logger;

    public AnnouncementService(ApplicationDbContext context, ILogger<AnnouncementService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<AnnouncementDto>> GetAllAsync(bool includeInactive = false)
    {
        var query = _context.Announcements.AsQueryable();
        if (!includeInactive)
            query = query.Where(a => a.IsActive);

        var announcements = await query
            .OrderByDescending(a => a.StartDate)
            .ToListAsync();

        return announcements.Select(MapToDto);
    }

    public async Task<IEnumerable<AnnouncementDto>> GetActiveAsync()
    {
        var now = DateTime.UtcNow;
        var announcements = await _context.Announcements
            .Where(a => a.IsActive && a.StartDate <= now && (a.EndDate == null || a.EndDate > now))
            .OrderByDescending(a => a.StartDate)
            .ToListAsync();

        return announcements.Select(MapToDto);
    }

    public async Task<AnnouncementDto?> GetByIdAsync(int id)
    {
        var ann = await _context.Announcements.FindAsync(id);
        return ann == null ? null : MapToDto(ann);
    }

    public async Task<AnnouncementDto> CreateAsync(CreateAnnouncementDto dto, string createdBy)
    {
        var announcement = new Announcement
        {
            Title = dto.Title,
            Message = dto.Content,
            StartDate = dto.EffectiveDate,
            EndDate = dto.ExpirationDate,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy,
        };

        _context.Announcements.Add(announcement);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Announcement {Id} created by {User}", announcement.Id, createdBy);
        return MapToDto(announcement);
    }

    public async Task<AnnouncementDto?> UpdateAsync(int id, UpdateAnnouncementDto dto)
    {
        var existing = await _context.Announcements.FindAsync(id);
        if (existing == null) return null;

        existing.Title = dto.Title;
        existing.Message = dto.Content;
        existing.StartDate = dto.EffectiveDate;
        existing.EndDate = dto.ExpirationDate;
        existing.IsActive = dto.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapToDto(existing);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _context.Announcements.FindAsync(id);
        if (existing == null) return false;

        _context.Announcements.Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }

    private static AnnouncementDto MapToDto(Announcement a) => new()
    {
        Id = a.Id,
        Title = a.Title,
        Content = a.Message,
        IsActive = a.IsActive,
        EffectiveDate = a.StartDate,
        ExpirationDate = a.EndDate,
        CreatedBy = a.CreatedBy,
        CreatedAt = a.CreatedAt,
        UpdatedAt = a.UpdatedAt,
    };
}
