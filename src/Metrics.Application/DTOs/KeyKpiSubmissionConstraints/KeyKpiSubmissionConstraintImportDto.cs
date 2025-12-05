namespace Metrics.Application.DTOs.KeyKpiSubmissionConstraints;

public record KeyKpiSubmissionConstraintImportDto
{
    public bool IsDeleted { get; init; }
    public long CandidateDepartmentId { get; init; }
    // public long DepartmentKeyMetricId { get; init; }
    //
    public long KeyMetricId { get; init; }
    public long KeyIssueDepartmentId { get; init; }

}
