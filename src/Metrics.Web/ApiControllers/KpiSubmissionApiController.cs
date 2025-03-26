using Metrics.Application.DTOs.KpiSubmissionDtos;
using Metrics.Application.Services.IServices;
using Microsoft.AspNetCore.Http;
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
        var result = await _kpiSubmissionService.FindAll_Async();

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync(KpiSubmissionCreateDto createDto)
    {
        var newSubmission = await _kpiSubmissionService.Create_Async(createDto);

        var uri = Url.Action(nameof(PostAsync), new { id = newSubmission.SubmissionDate });

        return Created(uri, newSubmission);
    }

    [HttpPost]
    public async Task<IActionResult> PostListAsync(List<KpiSubmissionCreateDto> createDtos)
    {
        var newSubmission = await _kpiSubmissionService.CreateRange_Async(createDtos);

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
