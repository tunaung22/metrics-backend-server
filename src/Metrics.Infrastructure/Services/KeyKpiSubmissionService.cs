using Metrics.Application.Domains;
using Metrics.Application.DTOs;
using Metrics.Application.Interfaces.IServices;
using Metrics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Metrics.Infrastructure.Services;

public class KeyKpiSubmissionService : IKeyKpiSubmissionService
{

    private readonly ILogger<KeyKpiSubmissionService> _logger;
    private readonly MetricsDbContext _context;

    public KeyKpiSubmissionService(
        ILogger<KeyKpiSubmissionService> logger,
        MetricsDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<List<KeyKpiSubmission>> FindBySubmitterByPeriodByDepartmentListAsync(
        ApplicationUser candidate,
        long kpiPeriodId,
        List<long> departmentIdList)
    {
        try
        {
            // var foundSubmissions = await _keyMetricSubmissionRepository.FindAllAsQueryable()
            //     .Include(e => e.TargetPeriod)
            //     .Include(e => e.TargetDepartment)
            //     .Include(e => e.SubmittedBy)
            //     .Include(e => e.KeyKpiSubmissionItems)
            //     .Where(e => e.SubmittedBy.Id == candidate.Id
            //         && e.ScoreSubmissionPeriodId == kpiPeriodId
            //         && departmentIdList.Any(d => d == e.DepartmentId))
            //     .ToListAsync();
            // REMOVE: Repository 
            var query = _context.KeyKpiSubmissions
                .Where(e => e.SubmittedBy.Id == candidate.Id
                    && e.ScoreSubmissionPeriodId == kpiPeriodId
                    // && departmentIdList.Any(departmentId => departmentId == e.DepartmentId))
                    && departmentIdList.Contains(e.DepartmentId))
                .OrderBy(e => e.SubmittedAt)
                .Include(e => e.TargetPeriod)
                .Include(e => e.TargetDepartment)
                .Include(e => e.SubmittedBy)
                .Include(e => e.KeyKpiSubmissionItems);
            var foundSubmissions = await query.ToListAsync();

            if (foundSubmissions.Count > 0)
            {
                return foundSubmissions;
                // return foundSubmissions.Select(s => new KeyKpiSubmission
                // {
                //     SubmittedAt = s.SubmittedAt,
                //     SubmissionDate = s.SubmissionDate,
                //     ScoreSubmissionPeriodId = s.ScoreSubmissionPeriodId,
                //     DepartmentId = s.DepartmentId,
                //     ApplicationUserId = s.ApplicationUserId,
                //     SubmittedBy = s.SubmittedBy,
                //     TargetDepartment = s.TargetDepartment,
                //     TargetPeriod = s.TargetPeriod,
                //     KeyKpiSubmissionItems = s.KeyKpiSubmissionItems
                // }).ToList();
            }

            return [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying key metric submissions by submittter by period by department list.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<long> FindCountByUserByPeriodAsync(
        string currentUserId,
        long kpiPeriodId)
    {
        try
        {
            // return await _keyMetricSubmissionRepository
            //     .FindCountByUserByPeriodAsync(currentUserId, kpiPeriodId);
            // REMOVE: Repository 
            return await _context.KeyKpiSubmissions
                .Where(s =>
                    s.ApplicationUserId == currentUserId
                    && s.ScoreSubmissionPeriodId == kpiPeriodId)
                .CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while counting submissions.");
            throw new Exception("An unexpected error occurred. Please try again later.");

        }
    }

    public async Task<bool> SubmitScoreAsync(List<KeyKpiSubmissionCreateDto> createDtos)
    {
        // await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // 1. insert parent
            // 2. insert child item (use parent's id)
            // ...... METHOD 1 vs METHOD 2
            // ...... METHOD 1 - save all parents, then save all children??? (better roundtrips)
            // ...... METHOD 2 - save one parent, then save its children??? (more database roundtrips)
            // ...... **both run inside transaction (no data inconsistency)
            // REMOVE: Repository 
            // METHOD 1
            // var submittedAt = DateTimeOffset.UtcNow;
            // var parentItems = new List<KeyKpiSubmission>();
            // var childItems = new List<KeyKpiSubmissionItem>();

            // foreach (var dto in createDtos)
            // {
            //     parentItems.Add(new KeyKpiSubmission
            //     {
            //         SubmittedAt = submittedAt,
            //         DepartmentId = dto.DepartmentId,
            //         ScoreSubmissionPeriodId = dto.ScoreSubmissionPeriodId,
            //         ApplicationUserId = dto.ApplicationUserId
            //     });
            // }
            // _context.KeyKpiSubmissions.AddRange(parentItems);
            // await _context.SaveChangesAsync();

            // var a = parentItems.Zip(createDtos, (p, d) =>
            // {
            //     return (p, d);
            // });
            // foreach (var (parent, dto) in parentItems.Zip(createDtos, (p, d) => (p, d)))
            // {
            //     var childItem = dto.keyKpiSubmissionItemDtos
            //         .Select(item => new KeyKpiSubmissionItem
            //         {
            //             KeyKpiSubmissionId = parent.Id,
            //             DepartmentKeyMetricId = item.DepartmentKeyMetricsId,
            //             ScoreValue = item.ScoreValue,
            //             Comments = item.Comments
            //         });
            //     childItems.AddRange(childItem);
            // }
            // _context.KeyKpiSubmissionItems.AddRange(childItems);
            // await _context.SaveChangesAsync();

            // METHOD 2
            // var submittedAt = DateTime.UtcNow;
            // foreach (var dto in createDtos)
            // {
            //     var parentItem = _context.KeyKpiSubmissions.Add(new KeyKpiSubmission
            //     {
            //         SubmittedAt = submittedAt,
            //         DepartmentId = dto.DepartmentId,
            //         ScoreSubmissionPeriodId = dto.ScoreSubmissionPeriodId,
            //         ApplicationUserId = dto.ApplicationUserId
            //     });
            //     await _context.SaveChangesAsync();
            //     // save child items
            //     /*
            //     foreach (var item in dto.keyKpiSubmissionItemDtos)
            //     {
            //         _context.KeyKpiSubmissionItems.Add(new KeyKpiSubmissionItem
            //         {
            //             KeyKpiSubmissionId = parentItem.Entity.Id,
            //             DepartmentKeyMetricId = item.DepartmentKeyMetricsId,
            //             ScoreValue = item.ScoreValue,
            //             Comments = item.Comments
            //         });
            //         await _context.SaveChangesAsync();
            //     } */
            //     var childItems = dto.keyKpiSubmissionItemDtos
            //         .Select(item => new KeyKpiSubmissionItem
            //         {
            //             KeyKpiSubmissionId = parentItem.Entity.Id,
            //             DepartmentKeyMetricId = item.DepartmentKeyMetricsId,
            //             ScoreValue = item.ScoreValue,
            //             Comments = item.Comments
            //         }).ToList();
            //     _context.AddRange(childItems);
            //     await _context.SaveChangesAsync();
            // }

            // METHOD 3
            // **Note:: need to check parent item exist or not
            //  IF Exist -> insert Parent + insert Child Items
            //  ELSE     -> skip   Parent + insert Child Items
            var submittedAt = DateTimeOffset.UtcNow;

            // DTO to Entity
            var parentEntities = createDtos.Select(dto =>
            {
                var parentItem = new KeyKpiSubmission
                {
                    SubmittedAt = submittedAt,
                    DepartmentId = dto.DepartmentId,
                    ScoreSubmissionPeriodId = dto.ScoreSubmissionPeriodId,
                    ApplicationUserId = dto.ApplicationUserId,
                    KeyKpiSubmissionItems = dto.keyKpiSubmissionItemDtos
                        .Select(item => new KeyKpiSubmissionItem
                        {
                            DepartmentKeyMetricId = item.DepartmentKeyMetricId,
                            ScoreValue = item.ScoreValue,
                            Comments = item.Comments
                        }).ToList()
                };
                return parentItem;
            }).ToList();

            // _context.KeyKpiSubmissions.AddRange(parents);
            foreach (var item in parentEntities)
            {
                // Check each parent already exist by period, submitter, department
                var existingParent = _context.KeyKpiSubmissions
                    .Where(s =>
                        s.ScoreSubmissionPeriodId == item.ScoreSubmissionPeriodId
                        && s.ApplicationUserId == item.ApplicationUserId
                        && s.DepartmentId == item.DepartmentId)
                    .FirstOrDefault();

                if (existingParent != null)
                {
                    // Only Child
                    // update child entity with parentId
                    var childItems = item.KeyKpiSubmissionItems.Select(i => new KeyKpiSubmissionItem
                    {
                        KeyKpiSubmissionId = existingParent.Id,
                        DepartmentKeyMetricId = i.DepartmentKeyMetricId,
                        ScoreValue = i.ScoreValue,
                        Comments = i.Comments
                    }).ToList();

                    await _context.KeyKpiSubmissionItems.AddRangeAsync(childItems);
                }
                else
                {
                    // Parent + Child
                    await _context.KeyKpiSubmissions.AddRangeAsync(item);
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            // await transaction.RollbackAsync();
            _logger.LogError(ex, "Unexpected error while creating submission.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }
}
