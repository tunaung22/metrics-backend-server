using System.ComponentModel.DataAnnotations;

namespace Metrics.Web.Models;

public class PaginationModel
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Page must be present and greater than 0")]
    public int Page { get; set; } = 1;

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Display must be present and greater than 0")]
    public int Display { get; set; } = 10;

    public PaginationModel() { }

    public PaginationModel(int page, int display)
    {
        Page = page;
        Display = display;
    }

}
