using Metrics.Application.Domains;

namespace Metrics.Application.Interfaces.IRepositories;

public interface ICaseFeedbackSubmissionRepository
{
    void Add(CaseFeedbackSubmission submission);
    Task UpdateAsync(string lookupId, CaseFeedbackSubmission entity);
    void UpdateAsync(CaseFeedbackSubmission target, CaseFeedbackSubmission newEntity);
    Task<CaseFeedbackSubmission?> FindByLookupIdAsync(string lookupId);
    Task<List<CaseFeedbackSubmission>> FindByKpiPeriodAndSubmitterAsync(long periodId, string userId);
    Task<List<CaseFeedbackSubmission>> FindAllByKpiPeriodAsync(long periodId);
    Task<List<CaseFeedbackSubmission>> FindAllAsync();
    // QUERYABLE
    IQueryable<CaseFeedbackSubmission> FindAllQueryable();
}
