using Metrics.Application.Domains;
using Metrics.Application.Exceptions;
using Metrics.Application.Interfaces.IRepositories;
using Metrics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Metrics.Infrastructure.Repositories;

public class CaseFeedbackSubmissionRepository : ICaseFeedbackSubmissionRepository
{
    private readonly MetricsDbContext _context;

    public CaseFeedbackSubmissionRepository(MetricsDbContext context)
    {
        _context = context;
    }

    public void Add(CaseFeedbackSubmission submission)
    {
        _context.CaseFeedbackSubmissions.Add(submission);
    }

    public async Task UpdateAsync(string lookupId, CaseFeedbackSubmission entity)
    {
        var targetSubmission = await FindByLookupIdAsync(lookupId);
        if (targetSubmission == null)
            throw new MetricsNotFoundException("Case feedback submission to update does not found.");

        // update
        targetSubmission.IncidentAt = entity.IncidentAt;
        targetSubmission.CaseDepartmentId = entity.CaseDepartmentId;
        targetSubmission.NegativeScoreValue = entity.NegativeScoreValue;
        targetSubmission.WardName = entity.WardName;
        targetSubmission.CPINumber = entity.CPINumber;
        targetSubmission.PatientName = entity.PatientName;
        targetSubmission.RoomNumber = entity.RoomNumber;
        targetSubmission.Description = entity.Description;
        targetSubmission.Comments = entity.Comments;

        _context.Entry(targetSubmission).State = EntityState.Modified;
    }

    public void UpdateAsync(CaseFeedbackSubmission target, CaseFeedbackSubmission newEntity)
    {
        target.IncidentAt = newEntity.IncidentAt;
        target.CaseDepartmentId = newEntity.CaseDepartmentId;
        target.NegativeScoreValue = newEntity.NegativeScoreValue;
        target.WardName = newEntity.WardName;
        target.CPINumber = newEntity.CPINumber;
        target.PatientName = newEntity.PatientName;
        target.RoomNumber = newEntity.RoomNumber;
        target.Description = newEntity.Description;
        target.Comments = newEntity.Comments;

        _context.Entry(target).State = EntityState.Modified;
    }

    public async Task<CaseFeedbackSubmission?> FindByLookupIdAsync(string lookupId)
    {
        if (!Guid.TryParse(lookupId, out _))
            throw new ArgumentException("Invalid LookupId format.");

        var submission = await _context.CaseFeedbackSubmissions
            .Include(e => e.CaseDepartment)
            .Include(e => e.SubmittedBy)
                .ThenInclude(e => e.Department)
            .Include(e => e.SubmittedBy.UserTitle)
            .Include(e => e.TargetPeriod)
            .Where(e => e.LookupId == Guid.Parse(lookupId))
            .FirstOrDefaultAsync();

        return submission;
    }

    /// <summary>
    /// Returns all submissions for a specific KPI period and submitter, 
    /// ordered by submission date and department.
    /// </summary>
    public async Task<List<CaseFeedbackSubmission>> FindByKpiPeriodAndSubmitterAsync(
        long periodId,
        string userId)
    {
        var submissions = await _context.CaseFeedbackSubmissions
            .Where(e => e.KpiSubmissionPeriodId == periodId)
            .OrderBy(e => e.SubmissionDate)
                .ThenBy(e => e.CaseDepartment.DepartmentName)
            .Include(e => e.CaseDepartment)
            .Include(e => e.SubmittedBy)
                .ThenInclude(e => e.Department)
            .Include(e => e.SubmittedBy.UserTitle)
            .Include(e => e.TargetPeriod)
            .ToListAsync();

        return submissions ?? [];
    }

    /// <summary>
    /// Returns all submissions for a specific KPI period, 
    /// ordered by submitter name, department, and submission date.
    /// </summary>
    public async Task<List<CaseFeedbackSubmission>> FindAllByKpiPeriodAsync(long periodId)
    {
        var submissions = await _context.CaseFeedbackSubmissions
                .Where(e => e.KpiSubmissionPeriodId == periodId)
                .OrderBy(e => e.SubmittedBy.FullName)
                    .ThenBy(e => e.CaseDepartment.DepartmentName)
                    .ThenBy(e => e.SubmissionDate)
                .Include(e => e.CaseDepartment)
                .Include(e => e.SubmittedBy)
                    .ThenInclude(e => e.Department)
                .Include(e => e.SubmittedBy.UserTitle)
                .Include(e => e.TargetPeriod)
                .ToListAsync();

        return submissions ?? [];
    }

    /// <summary>
    /// Returns all submissions, 
    /// ordered by submitter name, department, and submission date.
    /// </summary>
    public async Task<List<CaseFeedbackSubmission>> FindAllAsync()
    {
        var result = await _context.CaseFeedbackSubmissions
            .OrderBy(e => e.SubmittedBy.FullName)
                .ThenBy(e => e.CaseDepartment.DepartmentName)
                .ThenBy(e => e.SubmissionDate)
            .Include(e => e.CaseDepartment)
            .Include(e => e.SubmittedBy)
                .ThenInclude(e => e.Department)
            .Include(e => e.SubmittedBy.UserTitle)
            .ToListAsync();

        return result ?? [];
    }


    // ==========QUERYABLE=======================
    public IQueryable<CaseFeedbackSubmission> FindAllQueryable()
    {
        return _context.CaseFeedbackSubmissions
            .OrderBy(e => e.SubmissionDate)
            .ThenBy(e => e.SubmittedBy.FullName)
            .ThenBy(e => e.CaseDepartment.DepartmentName)
            .AsQueryable();
    }
}
