using Metrics.Application.Domains;
using Metrics.Application.DTOs;
using Metrics.Application.DTOs.CaseFeedbackScoreSubmission;

namespace Metrics.Application.Common.Mappers;

public static class CaseFeedbackScoreSubmissionMapper
{
    public static CaseFeedbackScoreSubmissionDto MapToDto(this CaseFeedbackScoreSubmission e)
    {
        return new CaseFeedbackScoreSubmissionDto(
            Id: e.Id,
            LookupId: e.LookupId,
            SubmittedAt: e.SubmittedAt,
            SubmissionDate: e.SubmissionDate,
            NegativeScoreValue: e.NegativeScoreValue,
            Comments: e.Comments,
            SubmitterId: e.SubmitterId,
            SubmittedBy: e.SubmittedBy.MapToDto(),
            CaseFeedbackId: e.CaseFeedbackId,
            CaseFeedback: e.Feedback.MapToDto(),
            IsDeleted: e.IsDeleted,
            CreatedAt: e.CreatedAt,
            ModifiedAt: e.ModifiedAt
        );
    }



    public static CaseFeedbackScoreSubmission MapToEntity(this CaseFeedbackScoreSubmissionDto dto)
    {
        // TODO: Use factory method
        return new CaseFeedbackScoreSubmission
        {
            SubmitterId = dto.SubmitterId,
            CaseFeedbackId = dto.CaseFeedbackId,
            NegativeScoreValue = dto.NegativeScoreValue,
            Comments = dto.Comments,
        };
    }

    /// <summary>
    /// MapToEntity
    /// CaseFeedbackScoreSubmissionUpsertDto to CaseFeedbackScoreSubmission
    /// </summary>
    /// <param name="upsertDto"></param>
    /// <returns></returns>
    [Obsolete("Use MapToCreate and MaptToUpdateDto respectivly instead.")]
    public static CaseFeedbackScoreSubmission MapToEntity(this CaseFeedbackScoreSubmissionUpsertDto upsertDto)
    {
        return new CaseFeedbackScoreSubmission
        {
            // TODO: 
            SubmitterId = upsertDto.ScoreSubmitterId,
            SubmittedAt = upsertDto.SubmittedAt,
            CaseFeedbackId = upsertDto.CaseFeedbackId,
            NegativeScoreValue = upsertDto.NegativeScoreValue,
            Comments = upsertDto.Comments
        };
    }

    /// <summary>
    /// MapToEntity
    /// CaseFeedbackScoreSubmissionCreateDto to CaseFeedbackScoreSubmission
    /// </summary>
    /// <param name="createDto"></param>
    /// <returns></returns>
    public static CaseFeedbackScoreSubmission MapToEntity(this CaseFeedbackScoreSubmissionCreateDto createDto)
    {
        return new CaseFeedbackScoreSubmission
        {
            SubmitterId = createDto.ScoreSubmitterId,
            SubmittedAt = createDto.SubmittedAt,
            CaseFeedbackId = createDto.CaseFeedbackId,
            NegativeScoreValue = createDto.NegativeScoreValue,
            Comments = createDto.Comments
        };
    }

    /// <summary>
    /// MapToEntity
    /// CaseFeedbackScoreSubmissionUpdateDto to CaseFeedbackScoreSubmission
    /// </summary>
    /// <param name="updateDto"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static CaseFeedbackScoreSubmission MapToEntity(this CaseFeedbackScoreSubmissionUpdateDto updateDto)
    {
        if (updateDto == null) throw new ArgumentException(nameof(updateDto));

        return new CaseFeedbackScoreSubmission
        {
            Id = updateDto.Id,
            NegativeScoreValue = updateDto.NegativeScoreValue,
            Comments = updateDto.Comments
        };
    }

    /// <summary>
    /// MapToUpdateDto
    /// CaseFeedbackScoreSubmissionUpsertDto to CaseFeedbackScoreSubmissionUpdateDto
    /// </summary>
    /// <param name="upsertDto"></param>
    /// <returns></returns>
    public static CaseFeedbackScoreSubmissionUpdateDto MapToUpdateDto(this CaseFeedbackScoreSubmissionUpsertDto upsertDto)
    {
        if (upsertDto == null) throw new ArgumentException(nameof(upsertDto));

        return new CaseFeedbackScoreSubmissionUpdateDto(
            Id: upsertDto.Id,
            NegativeScoreValue: upsertDto.NegativeScoreValue,
            Comments: upsertDto.Comments
        );
    }
    // 

    /// <summary>
    /// MapToCreateDto
    /// CaseFeedbackScoreSubmissionUpsertDto to CaseFeedbackScoreSubmissionCreateDto 
    /// </summary>
    /// <param name="upsertDto"></param>
    /// <returns></returns>
    public static CaseFeedbackScoreSubmissionCreateDto MapToCreateDto(this CaseFeedbackScoreSubmissionUpsertDto upsertDto)
    {
        if (upsertDto == null) throw new ArgumentException(nameof(upsertDto));

        return new CaseFeedbackScoreSubmissionCreateDto(
            ScoreSubmitterId: upsertDto.ScoreSubmitterId,
            SubmittedAt: upsertDto.SubmittedAt,
            CaseFeedbackId: upsertDto.CaseFeedbackId,
            NegativeScoreValue: upsertDto.NegativeScoreValue,
            Comments: upsertDto.Comments
        );
    }

}
