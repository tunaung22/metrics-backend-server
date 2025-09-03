using Metrics.Application.Domains;

namespace Metrics.Application.Interfaces.IRepositories;

public interface ICaseFeedbackRepository
{
    void Add(CaseFeedback submission);
    Task UpdateAsync(string lookupId, CaseFeedback entity);
    void UpdateAsync(CaseFeedback target, CaseFeedback newEntity);
    Task<CaseFeedback?> FindByLookupIdAsync(string lookupId);
    Task<List<CaseFeedback>> FindByKpiPeriodAndSubmitterAsync(long periodId, string userId);
    Task<List<CaseFeedback>> FindAllByKpiPeriodAsync(long periodId);
    Task<List<CaseFeedback>> FindAllAsync();
    // QUERYABLE
    IQueryable<CaseFeedback> FindAllQueryable();
}
