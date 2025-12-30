using Metrics.Application.Domains;
using Metrics.Infrastructure.Data;
using Metrics.Infrastructure.Repositories;
using Metrics.Shared.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;

namespace Metrics.UnitTest.Infrastructure.Repositories;

public class DepartmentRepositoryTests
{
    private readonly DbContextOptions<MetricsDbContext> _options;
    private readonly IOptions<PostgresDbConfig> _mockPgConfig;
    readonly Guid deaprtment_1_code = Guid.NewGuid();
    readonly Guid deaprtment_2_code = Guid.NewGuid();
    readonly Guid deaprtment_3_code = Guid.NewGuid();

    public DepartmentRepositoryTests()
    {
        // Setup in-memory database
        _options = new DbContextOptionsBuilder<MetricsDbContext>()
            .UseInMemoryDatabase($"metricsdb_{Guid.NewGuid()}")
            .Options;

        // Create mock configuration
        var mockPgConfig = new PostgresDbConfig
        {
            PgSchema = "test_schema",
            PgPoolSize = 10
        };
        _mockPgConfig = Mock.Of<IOptions<PostgresDbConfig>>(opt =>
            opt.Value == mockPgConfig);

        // Seed initial test data

        using var context = new MetricsDbContext(_options, _mockPgConfig);
        context.Departments.AddRangeAsync(
            new Department { Id = 1, DepartmentCode = deaprtment_1_code, DepartmentName = "Admin" },
            new Department { Id = 2, DepartmentCode = deaprtment_2_code, DepartmentName = "Clinic" },
            new Department { Id = 3, DepartmentCode = deaprtment_3_code, DepartmentName = "HR" }
        );
        context.SaveChangesAsync();
    }

    // [Fact]
    // public async Task FindAll_ShouldReturn_WhenExist()
    // {
    //     // Arrange
    //     using var context = new MetricsDbContext(_options, _mockPgConfig);
    //     var repository = new DepartmentRepository(context);
    //     // Act
    //     // Assert
    // }

    [Fact]
    public async Task Create_ShouldReturn_WhenExist()
    {
        // Arrange
        using var context = new MetricsDbContext(_options, _mockPgConfig);
        var repository = new DepartmentRepository(context);
        var newDepartment = new Department { DepartmentCode = Guid.NewGuid(), DepartmentName = "Finance" };
        // Act
        repository.Create(newDepartment);
        await context.SaveChangesAsync();
        // Assert
        var createdDepartment = await context.Departments.FindAsync(newDepartment.Id);
        Assert.NotNull(createdDepartment);
        Assert.Equal("Finance", createdDepartment.DepartmentName);
    }



    [Fact]
    public async Task Update_ShouldUpdateExistingDepartment()
    {
        // Arrange
        using var context = new MetricsDbContext(_options, _mockPgConfig);
        var repository = new DepartmentRepository(context);
        var department = await context.Departments.FindAsync(1L);
        // Act
        repository.Update(department!);
        department!.DepartmentName = "OT";
        await context.SaveChangesAsync();
        // Assert
        var updatedDepartment = await context.Departments.FindAsync(1L);
        Assert.Equal("OT", updatedDepartment!.DepartmentName);
    }

    [Fact]
    public async Task Delete_ShouldReturn_WhenExist()
    {
        // Arrange
        using var context = new MetricsDbContext(_options, _mockPgConfig);
        var repository = new DepartmentRepository(context);
        var department = await context.Departments
            .FindAsync(1L);

        // Act
        Assert.NotNull(department);
        repository.Delete(department);
        await context.SaveChangesAsync();

        // Assert
        var result = await context.Departments.ToListAsync();
        Assert.Equal(2, result.Count);
        Assert.DoesNotContain(result, d => d.Id == 1);
    }

    [Fact]
    public async Task FindByDepartmentCodeAsync_ShouldReturn_WhenExist()
    {
        // Arrange
        using var context = new MetricsDbContext(_options, _mockPgConfig);
        var repository = new DepartmentRepository(context);
        // Act
        var result = await repository.FindByDepartmentCodeAsync(deaprtment_3_code.ToString());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Id);
        Assert.Contains("HR", result.DepartmentName);
        Assert.DoesNotContain("Admin", result.DepartmentName);
    }

    [Fact]
    public async Task FindByDepartmentCodeAsync_ShouldReturnNull_WhenNotExist()
    {
        // Arrange
        using var context = new MetricsDbContext(_options, _mockPgConfig);
        var repository = new DepartmentRepository(context);
        // Act
        var result = await repository.FindByDepartmentCodeAsync("non-existing-department-code");

        // Assert
        // var exception = await Assert.ThrowsAsync<NotFoundException>(async () =>
        //      await repository.FindByDepartmentCodeAsync(deaprtment_3_code.ToString()));
        // Assert.Equal("Department not found.", exception.Message);
        Assert.Null(result);
    }

    [Fact]
    public async Task FindByDepartmentNameAsync_ShouldReturn_WhenExist()
    {
        // Arrange
        using var context = new MetricsDbContext(_options, _mockPgConfig);
        var repository = new DepartmentRepository(context);
        // Act
        var result = await repository.FindByDepartmentNameAsync("HR");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Id);
        Assert.Contains("HR", result.DepartmentName);
        Assert.DoesNotContain("Admin", result.DepartmentName);
    }

    [Fact]
    public async Task FindByDepartmentNameAsync_ShouldReturnNull_WhenNotExist()
    {
        // Arrange
        using var context = new MetricsDbContext(_options, _mockPgConfig);
        var repository = new DepartmentRepository(context);
        // Act
        var result = await repository.FindByDepartmentNameAsync("non-existing-name");

        // Assert  
        // var exception = await Assert.ThrowsAsync<NotFoundException>(async () =>
        //      await repository.FindByDepartmentNameAsync("HR"));
        // Assert.Equal("Department not found.", exception.Message);
        Assert.Null(result);
    }

    [Fact]
    public async Task FindByIdAsync_ShouldReturnDepartment_WhenExist()
    {
        // Arrange 
        using var context = new MetricsDbContext(_options, _mockPgConfig);
        var repository = new DepartmentRepository(context);
        // Act
        var result = await repository.FindByIdAsync(2);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Id);
        Assert.Contains("Clinic", result.DepartmentName);
        Assert.DoesNotContain("Admin", result.DepartmentName);
    }

    [Fact]
    public async Task FindByIdAsync_ShouldReturnNull_WhenNotExist()
    {
        // Arrange 
        using var context = new MetricsDbContext(_options, _mockPgConfig);
        var repository = new DepartmentRepository(context);
        // Act
        var result = await repository.FindByIdAsync(5);

        // Assert
        // var exception = await Assert.ThrowsAsync<NotFoundException>(async () =>
        //     await repository.FindByIdAsync(5));
        // Assert.Equal("Department not found.", exception.Message);
        Assert.Null(result);
    }

    [Fact]
    public async Task DepartmentExistsAsync_ShouldReturnTrue_WhenExist()
    {
        // Arrange
        using var context = new MetricsDbContext(_options, _mockPgConfig);
        var repository = new DepartmentRepository(context);
        // Act
        var result = await repository.DepartmentExistsAsync(deaprtment_1_code.ToString());
        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DepartmentExistsAsync_ShouldReturnFalse_WhenNotExist()
    {
        // Arrange
        using var context = new MetricsDbContext(_options, _mockPgConfig);
        var repository = new DepartmentRepository(context);
        // Act
        var result = await repository.DepartmentExistsAsync("non-existing-department-code".ToString());
        // Assert
        Assert.False(result);
    }


    [Fact]
    public async Task FindAllAsync_ShouldReturnAllDepartments()
    {
        // Arrange
        using var context = new MetricsDbContext(_options, _mockPgConfig);
        var repository = new DepartmentRepository(context);
        // Act
        var result = await repository.FindAllAsync();
        // Assert
        Assert.Equal(3, result.Count());
        Assert.Contains(result, d => d.DepartmentName == "Admin");
        Assert.Contains(result, d => d.DepartmentName == "Clinic");
        Assert.Contains(result, d => d.DepartmentName == "HR");
    }

    [Fact]
    public async Task Create_ShouldReturnEmptyList_WhenNotExist()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MetricsDbContext>()
            .UseInMemoryDatabase("EmptyDatabase")
            .Options;

        using var context = new MetricsDbContext(options, _mockPgConfig);
        var repository = new DepartmentRepository(context);

        // Act
        var result = await repository.FindAllAsync();
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact(Skip = "skip for now")]
    public async Task FindAllAsync_ShouldReturnPaginatedDepartments()
    {
        // Arrange
        using var context = new MetricsDbContext(_options, _mockPgConfig);
        var repository = new DepartmentRepository(context);

        // Act
        var pageNumber = 1;
        var pageSize = 5;
        var results = await repository.FindAllAsync(pageNumber, pageSize);
        // Assert
        // Assert.Equal();
        // Assert.Equal()
    }


    [Fact]
    public async Task FindCountAsync_ShouldReturnCount()
    {
        // Arrange
        using var context = new MetricsDbContext(_options, _mockPgConfig);
        var repository = new DepartmentRepository(context);
        // Act
        var departmentCount = await repository.FindCountAsync();

        // Assert
        Assert.Equal(3, departmentCount);
    }






}
