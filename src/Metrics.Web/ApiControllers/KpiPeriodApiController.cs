using Metrics.Application.DTOs.KpiPeriodDtos;
using Metrics.Application.Mappers.DtoMappers;
using Metrics.Application.Results;
using Metrics.Application.Services.IServices;
using Metrics.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using System;

namespace Metrics.Web.ApiControllers;

[ApiController]
[Route("api/kpi/periods")]
public class KpiPeriodApiController : ControllerBase
{

    private readonly IKpiPeriodService _kpiPeriodService;

    public KpiPeriodApiController(IKpiPeriodService kpiPeriodService)
    {
        _kpiPeriodService = kpiPeriodService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync()
    {
        var result = await _kpiPeriodService.FindAllAsync();

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
        var result = await _kpiPeriodService.CreateAsync(createDto.ToEntity());
        if (!result.Success)
        {
            switch (result.ErrorType)
            {
                case ErrorType.NotFound:
                    return NotFound(result.ErrorMessage);
                case ErrorType.ConcurrencyConflict:
                    return Conflict(result.ErrorMessage);
                case ErrorType.DatabaseError:
                    return StatusCode(500, result.ErrorMessage);
                case ErrorType.ValidationError:
                    return BadRequest(result.ErrorMessage);
                default:
                    return StatusCode(500, "An unexpected error occurred.");
            }

            // return result.ErrorType switch
            // {
            //     ErrorType.NotFound => NotFound(result.ErrorMessage),
            //     ErrorType.ConcurrencyConflict => Conflict(result.ErrorMessage),
            //     ErrorType.DatabaseError => StatusCode(500, result.ErrorMessage),
            //     ErrorType.ValidationError => BadRequest(result.ErrorMessage),
            //     _ => StatusCode(500, "An unexpected error occurred.")
            // };
        }

        var uri = Url.Action(nameof(PostAsync), new { periodName = result.Data.PeriodName });

        return Created(uri, result.Data);
    }

    [HttpPut("{periodName}")]
    public async Task<IActionResult> UpdateAsync(string periodName, KpiPeriodUpdateDto updateDto)
    {
        if (periodName == updateDto.PeriodName)
            return BadRequest();

        var entity = new KpiPeriod
        {
            PeriodName = updateDto.PeriodName,
            SubmissionStartDate = updateDto.SubmissionStartDate,
            SubmissionEndDate = updateDto.SubmissionEndDate
        };

        try
        {
            var updatedData = await _kpiPeriodService.UpdateAsync(entity);

            return Ok(updatedData.Data);
        }
        catch (System.Exception)
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
        catch (System.Exception)
        {
            throw;
        }
    }


}
