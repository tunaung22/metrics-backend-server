using Metrics.Application.DTOs.DepartmentDtos;
using Metrics.Application.Mappers.DtoMappers;
using Metrics.Application.Services.IServices;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Metrics.Web.ApiControllers
{
    [Route("api/departments")]
    [ApiController]
    public class DepartmentsApiController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentsApiController(IDepartmentService service)
        {
            _departmentService = service;
        }

        // GET: api/<DepartmentsController>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _departmentService.FindAllAsync();

            return Ok(result);
        }

        // GET api/<DepartmentsController>/ICU
        [HttpGet("{departmentCode}")]
        public async Task<IActionResult> Get(string departmentCode)
        {
            //var result = await _departmentService.FindByIdAsync(id);
            var result = await _departmentService.FindByDepartmentCodeAsync(departmentCode);
            if (result != null)
            {
                return Ok(result);
            }

            return NotFound();
        }

        // POST api/<DepartmentsController>
        [HttpPost]
        public async Task<ActionResult<DepartmentGetDto>> Post(DepartmentCreateDto createDto)
        {
            var entity = createDto.ToEntity();
            var newDepartment = await _departmentService.CreateAsync(entity);

            //var createdDepartment = DepartmentDtoMapper.ToGetDto(createdEntity);
            var uri = Url.Action(nameof(Post), new { id = newDepartment.Data.DepartmentCode });

            return Created(uri, newDepartment);
        }

        // PUT api/<DepartmentsController>/5
        [HttpPut("{departmentCode}")]
        public async Task<IActionResult> Put(string departmentCode, DepartmentUpdateDto updateDto)
        {
            // if (departmentCode == updateDto.DepartmentCode.ToString())
            //     return BadRequest();

            try
            {
                var entity = updateDto.ToEntity();
                var updatedDepartment = await _departmentService.UpdateAsync(entity);
                //var department = DepartmentDtoMapper.ToEntity(updateDto);
                //var updatedEntity = await _departmentService.UpdateAsync(departmentCode, entity);
                //var updatedDepartment = DepartmentDtoMapper.ToGetDto(updatedEntity);

                return Ok(updatedDepartment);
            }
            catch (Exception ex)
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
        public async Task<IActionResult> Delete(string departmentCode)
        {
            try
            {
                await _departmentService.DeleteAsync(departmentCode);
                return NoContent();

            }
            catch (Exception ex)
            {
                //throw new ArgumentNullException(nameof(departmentCode), "Department code argument must be provided.");
                return NotFound();
            }

        }

    }
}
