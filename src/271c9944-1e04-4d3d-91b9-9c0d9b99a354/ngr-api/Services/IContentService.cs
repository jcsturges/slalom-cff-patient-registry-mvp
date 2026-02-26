using NgrApi.Models;

namespace NgrApi.Services;

public interface IContentService
{
    Task<IEnumerable<Content>> GetContentAsync(string? category = null);
    Task<Content?> GetContentBySlugAsync(string slug);
    Task<Content> CreateContentAsync(Content content);
    Task<Content?> UpdateContentAsync(int id, Content content);
    Task<bool> DeleteContentAsync(int id);
}
