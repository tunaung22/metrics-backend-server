using Metrics.Application.Domains;

namespace Metrics.Application.Interfaces.IServices;

public interface ICaseFeedbackSubmissionService
{
    Task SaveAsync(CaseFeedbackSubmission submission);

    Task UpdateAsync(string lookupId, CaseFeedbackSubmission entity);

    Task<CaseFeedbackSubmission?> FindByLookupIdAsync(string lookupId);

    // Task<List<CaseFeedbackSubmission?>> FindByIncidentDate(DateTimeOffset incidentAt);
    Task<List<CaseFeedbackSubmission>> FindByKpiPeriodAndSubmitterAsync(long periodId, string userId);

    Task<List<CaseFeedbackSubmission>> FindByKpiPeriodAsync(long periodId);

    Task<List<CaseFeedbackSubmission>> FindAllAsync();

    IQueryable<CaseFeedbackSubmission> FindAllAsQueryable();
}
