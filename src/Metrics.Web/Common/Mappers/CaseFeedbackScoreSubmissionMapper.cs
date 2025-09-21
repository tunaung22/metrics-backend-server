using Metrics.Application.DTOs.CaseFeedbackScoreSubmission;
using Metrics.Web.Models;
using Metrics.Web.Models.ExcelExportModels;

namespace Metrics.Web.Common.Mappers;

public static class CaseFeedbackScoreSubmissionMapper
{
    // ViewModel -> Dto
    public static CaseFeedbackScoreSubmissionDto MapToDto(
        this CaseFeedbackScoreSubmissionViewModel model)
    {
        return new CaseFeedbackScoreSubmissionDto(
            Id: model.Id,
            LookupId: model.LookupId,
            SubmittedAt: model.SubmittedAt,
            SubmissionDate: model.SubmissionDate,
            NegativeScoreValue: model.NegativeScoreValue,
            Comments: model.Comments,
            SubmitterId: model.SubmitterId,
            SubmittedBy: model.SubmittedBy.MapToDto(),
            CaseFeedbackId: model.CaseFeedbackId,
            CaseFeedback: model.CaseFeedback.MapToDto(),
            IsDeleted: model.IsDeleted,
            CreatedAt: model.CreatedAt,
            ModifiedAt: model.ModifiedAt
        );
    }

    // Dto -> ViewModel 
    public static CaseFeedbackScoreSubmissionViewModel MapToViewModel(
        this CaseFeedbackScoreSubmissionDto dto)
    {
        return new CaseFeedbackScoreSubmissionViewModel
        {
            Id = dto.Id,
            LookupId = dto.LookupId,
            SubmittedAt = dto.SubmittedAt,
            SubmissionDate = dto.SubmissionDate,
            NegativeScoreValue = dto.NegativeScoreValue,
            Comments = dto.Comments,
            SubmitterId = dto.SubmitterId,
            SubmittedBy = dto.SubmittedBy.MapToViewModel(),
            CaseFeedbackId = dto.CaseFeedbackId,
            CaseFeedback = dto.CaseFeedback.MapToViewModel(),
            IsDeleted = dto.IsDeleted,
            CreatedAt = dto.CreatedAt,
            ModifiedAt = dto.ModifiedAt
        };
    }

    public static CaseFeedbackExcelViewModel MapToExcelViewModel(this CaseFeedbackScoreSubmissionDto dto)
    {
        return new CaseFeedbackExcelViewModel
        {
            // PeriodName = dto.CaseFeedback.TargetPeriod.PeriodName,
            SubmittedBy = dto.SubmittedBy.FullName,

            CaseDepartment = dto.CaseFeedback.CaseDepartment.DepartmentName,
            IncidentTime = dto.CaseFeedback.IncidentAt.LocalDateTime,
            Score = dto.NegativeScoreValue,
            WardName = dto.CaseFeedback.WardName,
            CPINumber = dto.CaseFeedback.CPINumber,
            PatientName = dto.CaseFeedback.PatientName,
            RoomNumber = dto.CaseFeedback.RoomNumber,
            Description = dto.CaseFeedback.Description ?? string.Empty,
            Comments = dto.Comments ?? string.Empty,
        };
    }
}
