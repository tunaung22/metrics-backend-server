using Metrics.Application.DTOs;
using Metrics.Application.DTOs.DepartmentDtos;
using Metrics.Application.Exceptions;
using Metrics.Application.Interfaces.IServices;
using Metrics.Application.Mappers.DtoMappers;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Metrics.Web.Controllers;

[Route("api/departments")]
[ApiController]
public class DepartmentController : ControllerBase
{
    private readonly IDepartmentService _departmentService;

    public DepartmentController(IDepartmentService service)
    {
        _departmentService = service;
    }

    // GET: api/<DepartmentsController>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var departments = await _departmentService.FindAllAsync();
        var result = departments.Select(d => new DepartmentGetDto
        {
            DepartmentCode = d.DepartmentCode,
            DepartmentName = d.DepartmentName
        }).ToList();

        return Ok(result);
    }

    // GET api/<DepartmentsController>/ICU
    [HttpGet("{departmentCode}")]
    public async Task<IActionResult> GetAsync(string departmentCode)
    {
        //var result = await _departmentService.FindByIdAsync(id);
        var department = await _departmentService.FindByDepartmentCodeAsync(departmentCode);
        if (department != null)
        {
            var result = new DepartmentGetDto
            {
                DepartmentCode = department.DepartmentCode,
                DepartmentName = department.DepartmentName
            };
            return Ok(result);
        }

        return NotFound();
    }

    // POST api/<DepartmentsController>
    [HttpPost]
    public async Task<ActionResult<DepartmentGetDto>> PostAsync(DepartmentCreateDto createDto)
    {
        var entity = createDto.ToEntity();
        var newDepartment = await _departmentService.CreateAsync(entity);

        if (newDepartment == null)
        {
            return BadRequest();
        }
        var uri = Url.Action(nameof(PostAsync), new { id = newDepartment.DepartmentCode });

        return Created(uri, newDepartment);
    }

    // PUT api/<DepartmentsController>/5
    [HttpPut("{departmentCode}")]
    public async Task<IActionResult> PutAsync(string departmentCode, DepartmentUpdateDto updateDto)
    {
        // if (departmentCode == updateDto.DepartmentCode.ToString())
        //     return BadRequest();

        try
        {
            var entity = updateDto.ToEntity();
            var updatedDepartment = await _departmentService.UpdateAsync(departmentCode, entity);

            return Ok(updatedDepartment);
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (DuplicateContentException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception)
        {
            return NotFound();
        }
    }

    //[HttpPatch("{departmentCode}")]
    //public async Task<IActionResult> Patch(string departmentCode, JsonPatchDocument<DepartmentPatchDto> patchDto)
    //{
    //var department = await _departmentService.GetByIdAsync(id);
    //if (department == null)
    //    return NotFound();

    //var departmentToPatch = UserMapper.ToPatchDto(department);
    //patchDoc.ApplyTo(departmentToPatch, ModelState);

    //if (!ModelState.IsValid)
    //    return BadRequest(ModelState);

    //var updatedDepartment = await _departmentService.PatchAsync(id, departmentToPatch);
    //return Ok(updatedDepartment);

    //}

    // DELETE api/<DepartmentsController>/5
    [HttpDelete("{departmentCode}")]
    public async Task<IActionResult> DeleteAsync(string departmentCode)
    {
        try
        {
            await _departmentService.DeleteAsync(departmentCode);
            return NoContent();

        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            //throw new ArgumentNullException(nameof(departmentCode), "Department code argument must be provided.");
            return StatusCode(500, ex.Message);
        }

    }

}
