using System;
using System.ComponentModel.DataAnnotations;

namespace Metrics.Web.Models.Paginition;

public class PaginationViewModel
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "PageNumber must be present and greater than 0")]
    public int PageNumber { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "PageSize must be present and greater than 0")]
    public int PageSize { get; set; }

    public PaginationViewModel() { }

    public PaginationViewModel(int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
