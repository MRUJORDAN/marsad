using MorsadBackend.Core.Entities;

namespace MorsadBackend.Core.Interfaces;

public interface INewsRepository
{
    Task<IEnumerable<NewsArticle>> GetTodayArticlesAsync(DateTime from, DateTime to);
    Task<IEnumerable<NewsArticle>> GetUnprocessedAsync(int limit = 50);
    Task<bool> ExistsByLinkAsync(string link);
    Task AddArticlesAsync(IEnumerable<NewsArticle> articles);
    Task MarkAsProcessedAsync(IEnumerable<int> ids);
    Task<int> SaveChangesAsync();
}

public interface ILookupRepository
{
    Task<LookupMinor?> GetMinorByCodeAsync(string code);
    Task<IEnumerable<LookupMinor>> GetMinorsByMajorCodeAsync(string majorCode);
    Task<IEnumerable<NewsSource>> GetActiveSourcesAsync();
    Task UpdateSourceLastFetchedAsync(int sourceId, DateTime fetchedAt);
}

public interface IRiskRepository
{
    Task<IEnumerable<RiskAssessment>> GetLatestAssessmentsAsync();
    Task AddAssessmentsAsync(IEnumerable<RiskAssessment> assessments);
    Task<int> SaveChangesAsync();
}
