using Metrics.Application.DTOs.KpiPeriodDtos;
using Metrics.Application.Domains;
using Metrics.Application.Interfaces.IServices;
using Metrics.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Metrics.Web.Controllers;

[ApiController]
[Route("api/kpi/periods")]
public class KpiPeriodController : ControllerBase
{

    private readonly IKpiSubmissionPeriodService _kpiPeriodService;

    public KpiPeriodController(IKpiSubmissionPeriodService kpiPeriodService)
    {
        _kpiPeriodService = kpiPeriodService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync(PaginationModel pagination)
    {
        var result = await _kpiPeriodService.FindAllAsync(pagination.Page, pagination.Display);

        return Ok(result);
    }

    [HttpGet("{periodName}")]
    public async Task<IActionResult> GetAsync(string periodName)
    {
        var result = await _kpiPeriodService.FindByKpiPeriodNameAsync(periodName);
        if (result != null)
            return Ok(result);

        return NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync(KpiPeriodCreateDto createDto)
    {
        var entity = new KpiSubmissionPeriod
        {
            PeriodName = createDto.PeriodName,
            SubmissionStartDate = createDto.SubmissionStartDate,
            SubmissionEndDate = createDto.SubmissionEndDate
        };
        var newKpiPeriod = await _kpiPeriodService.CreateAsync(entity);
        // case ErrorType.NotFound:
        //     return NotFound(result.ErrorMessage);
        // case ErrorType.ConcurrencyConflict:
        //     return Conflict(result.ErrorMessage);
        // case ErrorType.DatabaseError:
        //     return StatusCode(500, result.ErrorMessage);
        // case ErrorType.ValidationError:
        //     return BadRequest(result.ErrorMessage);
        // default:
        //     return StatusCode(500, "An unexpected error occurred.");

        // return result.ErrorType switch
        // {
        //     ErrorType.NotFound => NotFound(result.ErrorMessage),
        //     ErrorType.ConcurrencyConflict => Conflict(result.ErrorMessage),
        //     ErrorType.DatabaseError => StatusCode(500, result.ErrorMessage),
        //     ErrorType.ValidationError => BadRequest(result.ErrorMessage),
        //     _ => StatusCode(500, "An unexpected error occurred.")
        // };
        var uri = Url.Action(nameof(PostAsync), new { periodName = newKpiPeriod.PeriodName });

        return Created(uri, newKpiPeriod);

    }

    [HttpPut("{periodName}")]
    public async Task<IActionResult> UpdateAsync(string periodName, KpiPeriodUpdateDto updateDto)
    {
        if (periodName == updateDto.PeriodName)
            return BadRequest();

        var entity = new KpiSubmissionPeriod
        {
            PeriodName = updateDto.PeriodName,
            SubmissionStartDate = updateDto.SubmissionStartDate,
            SubmissionEndDate = updateDto.SubmissionEndDate
        };

        try
        {
            var updatedData = await _kpiPeriodService.UpdateAsync(periodName, entity);

            return Ok(updatedData);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<IActionResult> DeleteAsync(string periodName)
    {
        try
        {
            await _kpiPeriodService.DeleteAsync(periodName);

            return NoContent();
        }
        catch (Exception)
        {
            throw;
        }
    }


}
