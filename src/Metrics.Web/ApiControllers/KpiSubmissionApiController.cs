using Metrics.Application.DTOs.KpiSubmissionDtos;
using Metrics.Application.Domains;
using Metrics.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Mvc;

namespace Metrics.Web.ApiControllers;

[Route("api/[controller]")]
[ApiController]
public class KpiSubmissionApiController : ControllerBase
{
    private readonly IKpiSubmissionService _kpiSubmissionService;

    public KpiSubmissionApiController(IKpiSubmissionService kpiSubmissionService)
    {
        _kpiSubmissionService = kpiSubmissionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync()
    {
        var result = await _kpiSubmissionService.FindAllAsync();

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync(KpiSubmissionCreateDto createDto)
    {
        var submission = new KpiSubmission
        {
            KpiPeriodId = createDto.KpiPeriodId,
            EmployeeId = createDto.EmployeeId,
            DepartmentId = createDto.DepartmentId,
            KpiScore = createDto.KpiScore,
            SubmittedAt = createDto.SubmissionTime,
            Comments = createDto.Comments
        };
        var newSubmission = await _kpiSubmissionService.CreateAsync(submission);

        var uri = Url.Action(nameof(PostAsync), new { id = newSubmission.SubmissionDate });

        return Created(uri, newSubmission);
    }

    [HttpPost]
    public async Task<IActionResult> PostListAsync(List<KpiSubmissionCreateDto> createDtos)
    {
        var submissionList = createDtos.Select(dto => new KpiSubmission
        {
            KpiPeriodId = dto.KpiPeriodId,
            EmployeeId = dto.EmployeeId,
            DepartmentId = dto.DepartmentId,
            KpiScore = dto.KpiScore,
            SubmittedAt = dto.SubmissionTime,
            Comments = dto.Comments
        }).ToList();
        var newSubmission = await _kpiSubmissionService.CreateRangeAsync(submissionList);

        // var uri = Url.Action(nameof(PostAsync), new { id = newSubmission.SubmissionDate });

        // newSubmission.Select(s => Url.Action(nameof(GetAsync), new { id = s.id} ))
        return Created();
    }

    // [HttpGet]
    // public async Task<IActionResult> GetAsync()
    // {
    //     return Ok();
    // }
}
