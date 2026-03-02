using Microsoft.EntityFrameworkCore;
using NgrApi.Data;
using NgrApi.DTOs;
using NgrApi.Models;

namespace NgrApi.Services;

public interface IHelpPageService
{
    Task<IEnumerable<HelpPageDto>> GetAllAsync(bool includeUnpublished = false);
    Task<IEnumerable<HelpPageDto>> GetTreeAsync(bool includeUnpublished = false);
    Task<HelpPageDto?> GetByIdAsync(int id);
    Task<HelpPageDto?> GetBySlugAsync(string slug);
    Task<HelpPageDto?> GetByContextKeyAsync(string contextKey);
    Task<HelpPageDto> CreateAsync(CreateHelpPageDto dto, string createdBy);
    Task<HelpPageDto?> UpdateAsync(int id, UpdateHelpPageDto dto, string updatedBy);
    Task<bool> DeleteAsync(int id);
}

public class HelpPageService : IHelpPageService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HelpPageService> _logger;

    public HelpPageService(ApplicationDbContext context, ILogger<HelpPageService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<HelpPageDto>> GetAllAsync(bool includeUnpublished = false)
    {
        var query = _context.HelpPages.AsQueryable();
        if (!includeUnpublished)
            query = query.Where(p => p.IsPublished);

        var pages = await query.OrderBy(p => p.SortOrder).ThenBy(p => p.Title).ToListAsync();
        return pages.Select(MapToDto);
    }

    public async Task<IEnumerable<HelpPageDto>> GetTreeAsync(bool includeUnpublished = false)
    {
        var query = _context.HelpPages.AsQueryable();
        if (!includeUnpublished)
            query = query.Where(p => p.IsPublished);

        var allPages = await query.OrderBy(p => p.SortOrder).ThenBy(p => p.Title).ToListAsync();

        // Build tree: only return top-level pages with children nested
        var lookup = allPages.ToDictionary(p => p.Id);
        var tree = new List<HelpPageDto>();

        foreach (var page in allPages)
        {
            var dto = MapToDto(page);
            if (page.ParentId == null)
            {
                dto.Children = allPages
                    .Where(c => c.ParentId == page.Id)
                    .Select(MapToDto)
                    .ToList();
                tree.Add(dto);
            }
        }

        return tree;
    }

    public async Task<HelpPageDto?> GetByIdAsync(int id)
    {
        var page = await _context.HelpPages.FindAsync(id);
        return page == null ? null : MapToDto(page);
    }

    public async Task<HelpPageDto?> GetBySlugAsync(string slug)
    {
        var page = await _context.HelpPages.FirstOrDefaultAsync(p => p.Slug == slug);
        return page == null ? null : MapToDto(page);
    }

    public async Task<HelpPageDto?> GetByContextKeyAsync(string contextKey)
    {
        var page = await _context.HelpPages
            .FirstOrDefaultAsync(p => p.ContextKey == contextKey && p.IsPublished);
        return page == null ? null : MapToDto(page);
    }

    public async Task<HelpPageDto> CreateAsync(CreateHelpPageDto dto, string createdBy)
    {
        var page = new HelpPage
        {
            Title = dto.Title,
            Slug = dto.Slug,
            Content = dto.Content,
            ParentId = dto.ParentId,
            SortOrder = dto.SortOrder,
            IsPublished = dto.IsPublished,
            ContextKey = dto.ContextKey,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy,
            UpdatedBy = createdBy,
        };

        _context.HelpPages.Add(page);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Help page {Id} '{Title}' created by {User}", page.Id, page.Title, createdBy);
        return MapToDto(page);
    }

    public async Task<HelpPageDto?> UpdateAsync(int id, UpdateHelpPageDto dto, string updatedBy)
    {
        var existing = await _context.HelpPages.FindAsync(id);
        if (existing == null) return null;

        existing.Title = dto.Title;
        existing.Slug = dto.Slug;
        existing.Content = dto.Content;
        existing.ParentId = dto.ParentId;
        existing.SortOrder = dto.SortOrder;
        existing.IsPublished = dto.IsPublished;
        existing.ContextKey = dto.ContextKey;
        existing.UpdatedAt = DateTime.UtcNow;
        existing.UpdatedBy = updatedBy;

        await _context.SaveChangesAsync();
        return MapToDto(existing);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _context.HelpPages.FindAsync(id);
        if (existing == null) return false;

        _context.HelpPages.Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }

    private static HelpPageDto MapToDto(HelpPage p) => new()
    {
        Id = p.Id,
        Title = p.Title,
        Slug = p.Slug,
        Content = p.Content,
        ParentId = p.ParentId,
        SortOrder = p.SortOrder,
        IsPublished = p.IsPublished,
        ContextKey = p.ContextKey,
        CreatedBy = p.CreatedBy,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt,
    };
}
