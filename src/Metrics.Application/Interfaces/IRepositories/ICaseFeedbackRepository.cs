using Metrics.Application.Domains;

namespace Metrics.Application.Interfaces.IRepositories;

public interface ICaseFeedbackRepository
{
    void Add(CaseFeedback submission);
    Task UpdateAsync(string lookupId, CaseFeedback entity);
    void UpdateAsync(CaseFeedback target, CaseFeedback newEntity);
    Task<CaseFeedback?> FindByLookupIdAsync(string lookupId);
    // Task<List<CaseFeedback>> FindByKpiPeriodAndSubmitterAsync(long periodId, string userId);
    Task<List<CaseFeedback>> FindActiveBySubmitterAsync(string userId);
    // Task<List<CaseFeedback>> FindAllByKpiPeriodAsync(long periodId);
    Task<List<CaseFeedback>> FindAllAsync();
    Task<List<CaseFeedback>> FindAllActiveAsync();
    Task<List<CaseFeedback>> FindAllActiveAsync(DateTimeOffset startDate, DateTimeOffset endDate);
    // QUERYABLE
    IQueryable<CaseFeedback> FindAllQueryable();
}
