using Metrics.Application.Domains;
using Metrics.Application.Exceptions;
using Metrics.Application.Interfaces.IRepositories;
using Metrics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;

namespace Metrics.Infrastructure.Repositories;

public class CaseFeedbackRepository : ICaseFeedbackRepository
{
    private readonly MetricsDbContext _context;

    public CaseFeedbackRepository(MetricsDbContext context)
    {
        _context = context;
    }

    public void Add(CaseFeedback submission)
    {
        _context.CaseFeedbacks.Add(submission);
    }

    public async Task UpdateAsync(string lookupId, CaseFeedback entity)
    {
        var targetSubmission = await FindByLookupIdAsync(lookupId);
        if (targetSubmission == null)
            throw new MetricsNotFoundException("Case feedback submission to update was not found.");

        // update
        targetSubmission.IncidentAt = entity.IncidentAt;
        targetSubmission.CaseDepartmentId = entity.CaseDepartmentId;
        // targetSubmission.NegativeScoreValue = entity.NegativeScoreValue;
        targetSubmission.WardName = entity.WardName;
        targetSubmission.CPINumber = entity.CPINumber;
        targetSubmission.PatientName = entity.PatientName;
        targetSubmission.RoomNumber = entity.RoomNumber;
        targetSubmission.Description = entity.Description;
        // targetSubmission.Comments = entity.Comments;
    }

    public void UpdateAsync(CaseFeedback target, CaseFeedback newEntity)
    {
        target.IncidentAt = newEntity.IncidentAt;
        target.CaseDepartmentId = newEntity.CaseDepartmentId;
        // target.NegativeScoreValue = newEntity.NegativeScoreValue;
        target.WardName = newEntity.WardName;
        target.CPINumber = newEntity.CPINumber;
        target.PatientName = newEntity.PatientName;
        target.RoomNumber = newEntity.RoomNumber;
        target.Description = newEntity.Description;
        // target.Comments = newEntity.Comments;

        _context.Entry(target).State = EntityState.Modified;
    }

    public async Task<CaseFeedback?> FindByLookupIdAsync(string lookupId)
    {
        if (!Guid.TryParse(lookupId, out _))
            throw new ArgumentException("Invalid LookupId format.");

        var submission = await _context.CaseFeedbacks
            .Include(e => e.CaseDepartment)
            .Include(e => e.SubmittedBy)
                .ThenInclude(e => e.Department)
            .Include(e => e.SubmittedBy.UserTitle)
            // .Include(e => e.TargetPeriod)
            .Where(e => e.LookupId == Guid.Parse(lookupId))
            .FirstOrDefaultAsync();

        return submission;
    }

    /// <summary>
    /// Returns all submissions for a specific KPI period and submitter, 
    /// ordered by submission date and department.
    /// </summary>
    // public async Task<List<CaseFeedback>> FindByKpiPeriodAndSubmitterAsync(
    //     long periodId,
    //     string userId)
    // {
    //     var submissions = await _context.CaseFeedbacks
    //         .Where(e => e.KpiSubmissionPeriodId == periodId
    //             && e.SubmitterId == userId)
    //         .OrderBy(e => e.SubmissionDate)
    //             .ThenBy(e => e.CaseDepartment.DepartmentName)
    //         .Include(e => e.CaseDepartment)
    //         .Include(e => e.SubmittedBy)
    //             .ThenInclude(e => e.Department)
    //         .Include(e => e.SubmittedBy.UserTitle)
    //         .Include(e => e.TargetPeriod)
    //         .ToListAsync();

    //     return submissions ?? [];
    // }

    /// <summary>
    /// Returns all active submissions by submitter, 
    /// ordered by submission date and department.
    /// </summary>
    public async Task<List<CaseFeedback>> FindActiveBySubmitterAsync(string userId)
    {
        var submissions = await _context.CaseFeedbacks
            .Where(e => e.SubmitterId == userId
                && e.Proceeded == false)
            .OrderBy(e => e.SubmissionDate)
                .ThenBy(e => e.CaseDepartment.DepartmentName)
            .Include(e => e.CaseDepartment)
            .Include(e => e.SubmittedBy)
                .ThenInclude(e => e.Department)
            .Include(e => e.SubmittedBy.UserTitle)
            .ToListAsync();

        return submissions ?? [];
    }

    /// <summary>
    /// Returns all submissions for a specific KPI period, 
    /// ordered by submitter name, department, and submission date.
    /// </summary>
    // public async Task<List<CaseFeedback>> FindAllByKpiPeriodAsync(long periodId)
    // {
    //     var submissions = await _context.CaseFeedbacks
    //             .Where(e => e.KpiSubmissionPeriodId == periodId)
    //             .OrderBy(e => e.SubmittedBy.FullName)
    //                 .ThenBy(e => e.CaseDepartment.DepartmentName)
    //                 .ThenBy(e => e.SubmissionDate)
    //             .Include(e => e.CaseDepartment)
    //             .Include(e => e.SubmittedBy)
    //                 .ThenInclude(e => e.Department)
    //             .Include(e => e.SubmittedBy.UserTitle)
    //             .Include(e => e.TargetPeriod)
    //             .ToListAsync();

    //     return submissions ?? [];
    // }

    /// <summary>
    /// Returns all submissions, 
    /// ordered by submitter name, department, and submission date.
    /// </summary>
    public async Task<List<CaseFeedback>> FindAllAsync()
    {
        var result = await _context.CaseFeedbacks
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

    /// <summary>
    /// Returns all active submissions, 
    /// ordered by submitter name, department, and submission date.
    /// </summary>
    /// <returns></returns>
    public async Task<List<CaseFeedback>> FindAllActiveAsync()
    {
        var result = await _context.CaseFeedbacks
            .Where(e => e.Proceeded == false)
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

    public async Task<List<CaseFeedback>> FindAllActiveAsync(DateTimeOffset startDate, DateTimeOffset endDate)
    {
        return await _context.CaseFeedbacks
            .Where(e =>
                e.Proceeded == false &&
                e.SubmittedAt >= startDate &&
                e.SubmittedAt <= endDate) // date >= startDate && date <= endDate
            .OrderBy(e => e.SubmittedBy.FullName)
                .ThenBy(e => e.CaseDepartment.DepartmentName)
                .ThenBy(e => e.SubmissionDate)
            .Include(e => e.CaseDepartment)
            .Include(e => e.SubmittedBy)
                .ThenInclude(e => e.Department)
            .Include(e => e.SubmittedBy.UserTitle)
            .ToListAsync();
    }


    // ==========QUERYABLE=======================
    public IQueryable<CaseFeedback> FindAllQueryable()
    {
        return _context.CaseFeedbacks
            .Include(e => e.SubmittedBy)
            .Include(e => e.CaseDepartment)
            .OrderBy(e => e.SubmissionDate)
                .ThenBy(e => e.SubmittedBy.FullName)
                .ThenBy(e => e.CaseDepartment.DepartmentName)
            .AsQueryable();
    }
}
