
using Metrics.Application.DTOs.SeedingDtos;

namespace Metrics.Application.Interfaces.IServices;

public interface ISeedingService
{
    [Obsolete("Use SeedSysadminUser instead.")]
    Task SeedInitialUser(DefaultUserCreateDto createDto);
    Task SeedSysadminUser(SeedUserCreateDto createDto);

}
