using Metrics.Application.DTOs;
using Metrics.Web.Models;
using Metrics.Web.Models.ExcelExportModels;

namespace Metrics.Web.Common.Mappers;


public static class CaseFeedbackMapper
{

    public static CaseFeedbackDto MapToDto(this CaseFeedbackViewModel model)
    {
        return new CaseFeedbackDto(
            Id: model.Id,
            LookupId: model.LookupId,
            KpiSubmissionPeriodId: model.KpiSubmissionPeriodId,
            TargetPeriod: model.TargetPeriod.MapToDto(),
            SubmittedAt: model.SubmittedAt,
            SubmissionDate: model.SubmissionDate,
            FeedbackSubmitterId: model.FeedbackSubmitterId,
            FeedbackSubmittedBy: model.FeedbackSubmittedBy.MapToDto(),
            CaseDepartmentId: model.CaseDepartmentId,
            CaseDepartment: model.CaseDepartment.MapToDto(),
            WardName: model.WardName,
            CPINumber: model.CPINumber,
            PatientName: model.PatientName,
            RoomNumber: model.RoomNumber,
            IncidentAt: model.IncidentAt,
            Description: model.Description,
            IsDeleted: model.IsDeleted,
            CreatedAt: model.CreatedAt,
            ModifiedAt: model.ModifiedAt
        );
    }

    public static CaseFeedbackViewModel MapToViewModel(this CaseFeedbackDto dto)
    {
        return new CaseFeedbackViewModel
        {
            Id = dto.Id,
            LookupId = dto.LookupId,
            KpiSubmissionPeriodId = dto.KpiSubmissionPeriodId,
            TargetPeriod = dto.TargetPeriod.MapToViewModel(),
            SubmittedAt = dto.SubmittedAt,
            SubmissionDate = dto.SubmissionDate,
            FeedbackSubmitterId = dto.FeedbackSubmitterId,
            FeedbackSubmittedBy = dto.FeedbackSubmittedBy.MapToViewModel(),
            CaseDepartmentId = dto.CaseDepartmentId,
            CaseDepartment = dto.CaseDepartment.MapToViewModel(),
            WardName = dto.WardName,
            CPINumber = dto.CPINumber,
            PatientName = dto.PatientName,
            RoomNumber = dto.RoomNumber,
            IncidentAt = dto.IncidentAt,
            Description = dto.Description,
            IsDeleted = dto.IsDeleted,
            CreatedAt = dto.CreatedAt,
            ModifiedAt = dto.ModifiedAt
        };
    }


}
