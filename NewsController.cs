using Microsoft.EntityFrameworkCore;
using MorsadBackend.Core.Entities;
using MorsadBackend.Core.Interfaces;
using MorsadBackend.Infrastructure.Data;

namespace MorsadBackend.Infrastructure.Repositories;

public class NewsRepository : INewsRepository
{
    private readonly MorsadDbContext _db;
    public NewsRepository(MorsadDbContext db) => _db = db;

    public async Task<IEnumerable<NewsArticle>> GetTodayArticlesAsync(DateTime from, DateTime to)
        => await _db.NewsArticles
            .Include(a => a.Source)
            .Include(a => a.Tags).ThenInclude(t => t.CategoryMinor)
            .Include(a => a.Tags).ThenInclude(t => t.SentimentMinor)
            .Where(a => a.PublishedAt >= from && a.PublishedAt <= to)
            .OrderByDescending(a => a.PublishedAt)
            .ToListAsync();

    public async Task<IEnumerable<NewsArticle>> GetUnprocessedAsync(int limit = 50)
        => await _db.NewsArticles
            .Include(a => a.Source)
            .Where(a => !a.IsProcessed)
            .OrderBy(a => a.FetchedAt)
            .Take(limit)
            .ToListAsync();

    public async Task<bool> ExistsByLinkAsync(string link)
        => await _db.NewsArticles.AnyAsync(a => a.Link == link);

    public async Task AddArticlesAsync(IEnumerable<NewsArticle> articles)
        => await _db.NewsArticles.AddRangeAsync(articles);

    public async Task MarkAsProcessedAsync(IEnumerable<int> ids)
        => await _db.NewsArticles
            .Where(a => ids.Contains(a.Id))
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.IsProcessed, true));

    public async Task<int> SaveChangesAsync()
        => await _db.SaveChangesAsync();
}

public class LookupRepository : ILookupRepository
{
    private readonly MorsadDbContext _db;
    public LookupRepository(MorsadDbContext db) => _db = db;

    public async Task<LookupMinor?> GetMinorByCodeAsync(string code)
        => await _db.LookupMinors
            .Include(m => m.Major)
            .FirstOrDefaultAsync(m => m.Code == code && m.IsActive);

    public async Task<IEnumerable<LookupMinor>> GetMinorsByMajorCodeAsync(string majorCode)
        => await _db.LookupMinors
            .Include(m => m.Major)
            .Where(m => m.Major.Code == majorCode && m.IsActive)
            .OrderBy(m => m.SortOrder)
            .ToListAsync();

    public async Task<IEnumerable<NewsSource>> GetActiveSourcesAsync()
        => await _db.NewsSources
            .Include(s => s.TypeMinor)
            .Where(s => s.IsActive)
            .OrderBy(s => s.Id)
            .ToListAsync();

    public async Task UpdateSourceLastFetchedAsync(int sourceId, DateTime fetchedAt)
        => await _db.NewsSources
            .Where(s => s.Id == sourceId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.LastFetched, fetchedAt));
}

public class RiskRepository : IRiskRepository
{
    private readonly MorsadDbContext _db;
    public RiskRepository(MorsadDbContext db) => _db = db;

    public async Task<IEnumerable<RiskAssessment>> GetLatestAssessmentsAsync()
    {
        var latest = await _db.RiskAssessments
            .MaxAsync(r => (DateTime?)r.AssessedAt);
        if (latest == null) return Enumerable.Empty<RiskAssessment>();

        return await _db.RiskAssessments
            .Include(r => r.RiskTypeMinor)
            .Include(r => r.TrendMinor)
            .Where(r => r.AssessedAt == latest)
            .ToListAsync();
    }

    public async Task AddAssessmentsAsync(IEnumerable<RiskAssessment> assessments)
        => await _db.RiskAssessments.AddRangeAsync(assessments);

    public async Task<int> SaveChangesAsync()
        => await _db.SaveChangesAsync();
}
