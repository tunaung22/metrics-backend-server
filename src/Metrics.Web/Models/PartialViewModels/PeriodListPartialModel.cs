namespace Metrics.Web.Models.PartialViewModels;

public class PeriodListPartialModel
{
    public string TableId { get; set; } = "dataTable";
    public string LinkBasePage { get; set; } = string.Empty;
    public bool IncludeScripts { get; set; } = true;
    public List<PeriodListButton> Buttons { get; set; } = new();
    public IEnumerable<KpiPeriodViewModel> Periods { get; set; } = Enumerable.Empty<KpiPeriodViewModel>();
}
