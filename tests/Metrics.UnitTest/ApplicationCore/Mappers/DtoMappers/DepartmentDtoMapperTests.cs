using Metrics.Application.Common.Mappers;
using Metrics.Application.Domains;
using Metrics.Application.DTOs.Department;
using Metrics.Web.Mappers.ViewModelMappers;

namespace Metrics.UnitTest.ApplicationCore.Mappers.DtoMappers;

public class DepartmentDtoMapperTests
{

    [Fact]
    public void ToGetDto_IEnumberableEntities_ReturnsListDepartmentGetDto()
    {
        var id_1 = Guid.NewGuid();
        var id_2 = Guid.NewGuid();
        var department = new List<Department>
        {
            new Department { DepartmentCode = id_1, DepartmentName= "Human Resource" },
            new Department { DepartmentCode = id_2, DepartmentName= "Finance" },
        };
        var result = department.Select(d => d.MapToDto()).ToList();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(id_1, result[0].DepartmentCode);
        Assert.Equal("Human Resource", result[0].DepartmentName);
        Assert.Equal(id_2, result[1].DepartmentCode);
        Assert.Equal("Finance", result[1].DepartmentName);
    }

    [Fact]
    public void ToGetDto_Null_ReturnsEmptyList()
    {
        IEnumerable<Department>? departments = [];
        var result = departments.Select(d => d.MapToDto());

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void ToGetDto_Single_ReturnsGetDto()
    {
        var code = new Guid();
        var department = new Department
        {
            DepartmentCode = code,
            DepartmentName = "Clinic Department"
        };
        var result = department.MapToDto();

        Assert.NotNull(result);
        Assert.Equal(code, result.DepartmentCode);
        Assert.Equal("Clinic Department", result.DepartmentName);
    }

    [Fact]
    public void ToEntity_CreateDto_ReturnsCorrectEntity()
    {
        var code = new Guid();
        var dto = new DepartmentCreateDto
        {
            DepartmentCode = code,
            DepartmentName = "Clinic Department"
        };
        var result = dto.MapToEntity();

        Assert.NotNull(result);
        Assert.Equal(code, result.DepartmentCode);
        Assert.Equal("Clinic Department", result.DepartmentName);
    }

    [Fact]
    public void ToEntity_UpdateDto_ReturnsCorrectEntity()
    {
        var code = new Guid();
        var dto = new DepartmentUpdateDto
        {
            // DepartmentCode = code,
            DepartmentName = "Clinic Department"
        };
        var result = dto.MapToEntity();

        Assert.NotNull(result);
        Assert.Equal(code, result.DepartmentCode);
        Assert.Equal("Clinic Department", result.DepartmentName);
    }

    // [Fact]
    // public void ToGetDto_FromCreateDto_ReturnsCorrectDto()
    // {
    //     var code = new Guid();
    //     var dto = new DepartmentCreateDto
    //     {
    //         DepartmentCode = code,
    //         DepartmentName = "Clinic Department"
    //     };
    //     var result = dto.ToGetDto();

    //     Assert.NotNull(result);
    //     Assert.Equal(code, result.DepartmentCode);
    //     Assert.Equal("Clinic Department", result.DepartmentName);
    // }

}
