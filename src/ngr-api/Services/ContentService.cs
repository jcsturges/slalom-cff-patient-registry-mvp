using Microsoft.EntityFrameworkCore;
using NgrApi.Data;
using NgrApi.Models;

namespace NgrApi.Services;

public class ContentService : IContentService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ContentService> _logger;

    public ContentService(ApplicationDbContext context, ILogger<ContentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Content>> GetContentAsync(string? category = null)
    {
        var query = _context.Contents.AsQueryable();

        if (!string.IsNullOrEmpty(category))
            query = query.Where(c => c.Category == category);

        return await query.ToListAsync();
    }

    public async Task<Content?> GetContentBySlugAsync(string slug)
    {
        return await _context.Contents.FirstOrDefaultAsync(c => c.Slug == slug);
    }

    public async Task<Content> CreateContentAsync(Content content)
    {
        content.CreatedAt = DateTime.UtcNow;
        content.UpdatedAt = DateTime.UtcNow;
        _context.Contents.Add(content);
        await _context.SaveChangesAsync();
        return content;
    }

    public async Task<Content?> UpdateContentAsync(int id, Content content)
    {
        var existing = await _context.Contents.FindAsync(id);
        if (existing == null) return null;

        existing.Title = content.Title;
        existing.Body = content.Body;
        existing.Slug = content.Slug;
        existing.Category = content.Category;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteContentAsync(int id)
    {
        var existing = await _context.Contents.FindAsync(id);
        if (existing == null) return false;

        _context.Contents.Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }
}
