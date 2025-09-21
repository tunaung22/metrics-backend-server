using Metrics.Application.Domains;
using Metrics.Application.DTOs;

namespace Metrics.Application.Common.Mappers;

public static class CaseFeedbackMapper
{
    public static CaseFeedback MapToEntity(this CaseFeedbackDto dto)
    {
        return CaseFeedback.Create(
            dto.SubmittedAt,
            dto.FeedbackSubmitterId,
            dto.CaseDepartmentId,
            dto.WardName,
            dto.CPINumber,
            dto.PatientName,
            dto.RoomNumber,
            dto.IncidentAt,
            dto.Description,
            dto.IsDeleted);
    }

    public static CaseFeedbackDto MapToDto(this CaseFeedback entity)
    {
        return new CaseFeedbackDto(
            Id: entity.Id,
            LookupId: entity.LookupId,
            // KpiSubmissionPeriodId: entity.KpiSubmissionPeriodId,
            // TargetPeriod: entity.TargetPeriod.MapToDto(),
            SubmittedAt: entity.SubmittedAt,
            SubmissionDate: entity.SubmissionDate,
            FeedbackSubmitterId: entity.SubmitterId,
            FeedbackSubmittedBy: entity.SubmittedBy.MapToDto(),
            CaseDepartmentId: entity.CaseDepartmentId,
            CaseDepartment: entity.CaseDepartment.MapToDto(),
            WardName: entity.WardName,
            CPINumber: entity.CPINumber,
            PatientName: entity.PatientName,
            RoomNumber: entity.RoomNumber,
            IncidentAt: entity.IncidentAt,
            Description: entity.Description,
            IsDeleted: entity.IsDeleted,
            CreatedAt: entity.CreatedAt,
            ModifiedAt: entity.ModifiedAt);
    }
}
