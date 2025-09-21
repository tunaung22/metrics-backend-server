using Metrics.Application.Domains;
using Metrics.Application.Interfaces.IRepositories;
using Metrics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Metrics.Infrastructure.Repositories;

public class CaseFeedbackScoreSubmissionRepository(MetricsDbContext context)
    : ICaseFeedbackScoreSubmissionRepository
{
    private readonly MetricsDbContext _context = context;

    public void Add(CaseFeedbackScoreSubmission submission)
    {
        _context.CaseFeedbackScoreSubmissions.Add(submission);
    }

    public void AddRange(List<CaseFeedbackScoreSubmission> submissions)
    {
        _context.CaseFeedbackScoreSubmissions.AddRange(submissions);
    }
    public async Task AddRangeAsync(List<CaseFeedbackScoreSubmission> submissions)
    {
        await _context.CaseFeedbackScoreSubmissions.AddRangeAsync(submissions);
    }

    public async Task UpdateRangeAsync(List<CaseFeedbackScoreSubmission> submissions)
    {
        // _context.CaseFeedbackScoreSubmissions.UpdateRange(submissions);
        var IDs = submissions.Select(s => s.Id).ToList();
        var entitiesToUpdate = await _context.CaseFeedbackScoreSubmissions
            .Where(e => IDs.Contains(e.Id))
            .ToListAsync();
        foreach (var entity in entitiesToUpdate)
        {
            var updated = submissions.First(u => u.Id == entity.Id);
            entity.NegativeScoreValue = updated.NegativeScoreValue;
            entity.Comments = updated.Comments;
        }
    }

    public Task UpdateAsync(string lookupId, CaseFeedbackScoreSubmission entity)
    {
        throw new NotImplementedException();
    }

    public void UpdateAsync(CaseFeedbackScoreSubmission target, CaseFeedbackScoreSubmission newEntity)
    {
        target.NegativeScoreValue = newEntity.NegativeScoreValue;
        target.Comments = newEntity.Comments ?? string.Empty;
    }

    public async Task<List<CaseFeedbackScoreSubmission>> FindByIdAsync(List<long> IDs)
    {
        var submissions = await _context.CaseFeedbackScoreSubmissions
            .Include(e => e.SubmittedBy)
            .Include(e => e.SubmittedBy).ThenInclude(e => e.Department)
            .Include(e => e.Feedback)
            .Include(e => e.Feedback).ThenInclude(e => e.CaseDepartment)
            // .Include(e => e.Feedback).ThenInclude(e => e.TargetPeriod)
            .Where(e => IDs.Contains(e.Id))
            .OrderBy(e => e.SubmittedBy.FullName)
                .ThenBy(e => e.Feedback.SubmissionDate)
                .ThenBy(e => e.Feedback.CaseDepartment.DepartmentName)
            .ToListAsync();

        return submissions ?? [];
    }

    public async Task<CaseFeedbackScoreSubmission?> FindByLookupIdAsync(
        string lookupId)
    {
        if (!Guid.TryParse(lookupId, out _))
            throw new ArgumentException("Invalid Lookup ID format.");

        var submission = await _context.CaseFeedbackScoreSubmissions
            .Include(e => e.SubmittedBy).ThenInclude(e => e.Department)
            .Include(e => e.Feedback).ThenInclude(e => e.CaseDepartment)
            // .Include(e => e.Feedback).ThenInclude(e => e.TargetPeriod)
            .Where(e => e.LookupId == Guid.Parse(lookupId))
            .FirstOrDefaultAsync();

        return submission;
    }

    public async Task<List<CaseFeedbackScoreSubmission>> FindByKpiPeriodAndSubmitterAsync(
        long periodId,
        string userId)
    {
        var submissions = await _context.CaseFeedbackScoreSubmissions
            .Include(e => e.SubmittedBy)
            .Include(e => e.SubmittedBy).ThenInclude(e => e.Department)
            .Include(e => e.Feedback)
            .Include(e => e.Feedback).ThenInclude(e => e.CaseDepartment)
            // .Include(e => e.Feedback).ThenInclude(e => e.TargetPeriod)
            .Where(e =>
                // e.Feedback.KpiSubmissionPeriodId == periodId &&
                e.SubmitterId == userId)
            .OrderBy(e => e.SubmittedBy.FullName)
                .ThenBy(e => e.Feedback.SubmissionDate)
                .ThenBy(e => e.Feedback.CaseDepartment.DepartmentName)
            .ToListAsync();

        return submissions ?? [];
    }

    public async Task<List<CaseFeedbackScoreSubmission>> FindAllByKpiPeriodAsync(
        long periodId)
    {
        var submissions = await _context.CaseFeedbackScoreSubmissions
            .Include(e => e.SubmittedBy).ThenInclude(e => e.Department)
            .Include(e => e.SubmittedBy).ThenInclude(e => e.UserTitle)
            .Include(e => e.Feedback).ThenInclude(e => e.CaseDepartment)
            // .Include(e => e.Feedback).ThenInclude(e => e.TargetPeriod)
            // .Where(e => e.Feedback.KpiSubmissionPeriodId == periodId)
            .OrderBy(e => e.SubmittedBy.FullName)
                .ThenBy(e => e.Feedback.SubmissionDate)
                .ThenBy(e => e.Feedback.CaseDepartment.DepartmentName)
            .ToListAsync();

        return submissions ?? [];
    }

    public async Task<List<CaseFeedbackScoreSubmission>> FindAllAsync()
    {
        var result = await _context.CaseFeedbackScoreSubmissions
            .Include(e => e.SubmittedBy)
                .ThenInclude(e => e.Department)
            .Include(e => e.Feedback)
                .ThenInclude(e => e.CaseDepartment)
            .OrderBy(e => e.SubmittedBy.FullName)
                .ThenBy(e => e.Feedback.CaseDepartment.DepartmentName)
            .ToListAsync();

        return result ?? [];
    }

    // ==========QUERYABLE=======================
    public IQueryable<CaseFeedbackScoreSubmission> FindAllQueryable()
    {
        return _context.CaseFeedbackScoreSubmissions
            .Include(e => e.SubmittedBy)
            .Include(e => e.Feedback)
            .OrderBy(e => e.SubmissionDate)
                .ThenBy(e => e.SubmittedBy.FullName)
                .ThenBy(e => e.Feedback.CaseDepartment.DepartmentName)
            .AsQueryable();
    }


}
