using Metrics.Application.Domains;

namespace Metrics.Application.Interfaces.IServices;

public interface ICaseFeedbackService
{
    Task SaveAsync(CaseFeedback submission);

    Task UpdateAsync(string lookupId, CaseFeedback entity);

    Task<CaseFeedback?> FindByLookupIdAsync(string lookupId);

    // Task<List<CaseFeedbackSubmission?>> FindByIncidentDate(DateTimeOffset incidentAt);
    Task<List<CaseFeedback>> FindByKpiPeriodAndSubmitterAsync(long periodId, string userId);

    Task<List<CaseFeedback>> FindByKpiPeriodAsync(long periodId);

    Task<List<CaseFeedback>> FindAllAsync();

}
