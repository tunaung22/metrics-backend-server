
using Metrics.Application.DTOs.SeedingDtos;

namespace Metrics.Application.Interfaces.IServices;

public interface ISeedingService
{
    Task SeedInitialUser(DefaultUserCreateDto createDto);

}
