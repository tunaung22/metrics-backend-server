using System;
using System.ComponentModel.DataAnnotations;

namespace Metrics.Application.DTOs.AccountDtos;

public class UserUpdateDto
{
    [Required]
    public string UserCode { get; set; } = null!;
    [Required]
    public long DepartmentId { get; set; }
    [Required]
    public long UserTitleId { get; set; }
    [Required]
    public List<string> RoleNames { get; set; } = [];
}
