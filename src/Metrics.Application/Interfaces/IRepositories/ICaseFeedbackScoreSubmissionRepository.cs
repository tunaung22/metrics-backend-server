using Metrics.Application.Domains;

namespace Metrics.Application.Interfaces.IRepositories;

public interface ICaseFeedbackScoreSubmissionRepository
{
    void Add(CaseFeedbackScoreSubmission submission);
    void AddRange(List<CaseFeedbackScoreSubmission> submissions);
    Task AddRangeAsync(List<CaseFeedbackScoreSubmission> submissions);
    Task UpdateRangeAsync(List<CaseFeedbackScoreSubmission> submissions);

    Task UpdateAsync(string lookupId, CaseFeedbackScoreSubmission entity);
    void UpdateAsync(CaseFeedbackScoreSubmission target, CaseFeedbackScoreSubmission newEntity);
    Task<List<CaseFeedbackScoreSubmission>> FindByIdAsync(List<long> IDs);
    Task<CaseFeedbackScoreSubmission?> FindByLookupIdAsync(string lookupId);
    Task<List<CaseFeedbackScoreSubmission>> FindByKpiPeriodAndSubmitterAsync(long periodId, string userId);
    Task<List<CaseFeedbackScoreSubmission>> FindAllByKpiPeriodAsync(long periodId);
    Task<List<CaseFeedbackScoreSubmission>> FindAllAsync();
    // QUERYABLE
    IQueryable<CaseFeedbackScoreSubmission> FindAllQueryable();
}
