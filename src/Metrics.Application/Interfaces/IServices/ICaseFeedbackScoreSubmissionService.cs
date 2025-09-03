using Metrics.Application.Domains;
using Metrics.Application.DTOs.CaseFeedbackScoreSubmission;
using Metrics.Application.Results;

namespace Metrics.Application.Interfaces.IServices;

public interface ICaseFeedbackScoreSubmissionService
{
    Task SaveAsync(CaseFeedbackScoreSubmission submission);

    // Task SaveRangeAsync(List<CaseFeedbackScoreSubmission> submissions);
    Task SaveRangeAsync(List<CaseFeedbackScoreSubmissionCreateDto> submissions);
    Task<Result> UpsertRangeAsync(List<CaseFeedbackScoreSubmissionUpsertDto> submissions);
    Task<Result> UpdateRangeAsync(List<CaseFeedbackScoreSubmission> entities);
    Task UpdateAsync(string lookupId, CaseFeedbackScoreSubmission entity);

    Task<CaseFeedbackScoreSubmission?> FindByLookupIdAsync(string lookupId);

    // Task<List<CaseFeedbackScoreSubmission>> FindByKpiPeriodAndSubmitterAsync(long periodId, string userId);
    Task<ResultT<List<CaseFeedbackScoreSubmissionDto>>> FindByKpiPeriodAndSubmitterAsync(long periodId, string userId);

    // Task<Result<List<CaseFeedbackScoreSubmission>>> FindByKpiPeriodAndSubmitterAsync_V2(long periodId, string userId);

    // Task<List<CaseFeedbackScoreSubmission>> FindByKpiPeriodAsync(long periodId);
    Task<ResultT<List<CaseFeedbackScoreSubmissionDto>>> FindByKpiPeriodAsync(long periodId);


    Task<List<CaseFeedbackScoreSubmission>> FindAllAsync();
}
