using Metrics.Application.DTOs.KpiPeriodDtos;
using Metrics.Application.DTOs.KpiSubmissionDtos;
using Metrics.Application.Services;
using Metrics.Application.Services.IServices;
using Metrics.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

namespace Metrics.Web.Pages.Kpi.Submissions
{
    public class NewModel : PageModel
    {
        private readonly MetricsDbContext _context;
        private readonly IKpiSubmissionService _kpiSubmissionService;
        private readonly IDepartmentService _departmentService;
        private readonly IKpiPeriodService _kpiPeriodService;

        public NewModel(MetricsDbContext context,
            IDepartmentService departmentService,
            IKpiPeriodService kpiPeriodService,
            IKpiSubmissionService kpiSubmissionService)
        {
            _context = context;
            _departmentService = departmentService;
            _kpiPeriodService = kpiPeriodService;
            _kpiSubmissionService = kpiSubmissionService;
        }

        // ========== Model Bindings ====================================
        [BindProperty]
        public bool SubmissionAvaiable { get; set; }

        [BindProperty]
        public FormInputModel FormInput { get; set; } = new();

        [BindProperty]
        public List<DepartmentModel> Departments { get; set; } = [];

        [BindProperty]
        public List<SelectListItem> KpiPeriodListItems { get; set; } = [];

        // ========== Handlers ====================================
        public async Task<IActionResult> OnGet()
        {
            var currentDateTime = DateTimeOffset.UtcNow;

            // =============== Load Departments =========================
            var departments = await _departmentService.FindAllInsecure_Async();
            Departments = departments.Select(e => new DepartmentModel
            {
                Id = e.Id,
                DepartmentCode = e.DepartmentCode,
                DepartmentName = e.DepartmentName
            }).ToList();
            // =============== Load KPI Periods =========================
            var kpiPeriods = await _kpiPeriodService.FindAllByValidDate_Async(currentDateTime);
            // calculate and filter out only valid KpiPeriods
            KpiPeriodListItems = kpiPeriods.Select(e => new SelectListItem
            {
                Value = e.Id.ToString(),
                Text = e.PeriodName
            }).ToList();
            // =============== Submission Availability ==================
            SubmissionAvaiable = (KpiPeriodListItems.Count <= 0) ? false : true;

            // FormInput.KpiScoreEntries = departments.Select(e => new KpiScoreEntry()).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var createDtos = new List<KpiSubmissionCreateDto>();

            foreach (var entry in FormInput.KpiScoreEntries)
            {
                var createDto = new KpiSubmissionCreateDto
                {
                    EmployeeId = FormInput.CandidateId,
                    SubmissionTime = FormInput.SubmissionTime,
                    KpiPeriodId = FormInput.KpiPeriodId,
                    DepartmentId = entry.TargetDepartmentId,
                    KpiScore = entry.KpiScore,
                    Comments = entry.Comments
                };

                createDtos.Add(createDto);
            }

            try
            {
                var dto = await _kpiSubmissionService.CreateRange_Async(createDtos);

                return RedirectToPage();
            }
            catch (Exception)
            {
                // throw;
            }


            return Page();
        }

        public class FormInputModel
        {
            public long CandidateId { get; set; }
            public DateTimeOffset SubmissionTime { get; set; } = DateTimeOffset.Now;
            public long KpiPeriodId { get; set; }
            public List<KpiScoreEntry> KpiScoreEntries { get; set; } = new List<KpiScoreEntry>();
        }

        public class KpiScoreEntry
        {
            public long TargetDepartmentId { get; set; }
            public decimal KpiScore { get; set; }
            public string Comments { get; set; } = string.Empty;
        }

        public class SubmissionModel
        {
            // public long Id { get; set; }
            public DateTimeOffset SubmissionTime { get; set; }
            public decimal KpiScore { get; set; }
            public string? Comments { get; set; }

            // Foreign Keys
            public long KpiPeriodId { get; set; }
            public long DepartmentId { get; set; }
            public long EmployeeId { get; set; }

            // Reference Navigational Properties
            // public KpiPeriod KpiPeriod { get; set; } = null!;
            // public Department TargetDepartment { get; set; } = null!;
            // public Employee Candidate { get; set; } = null!;

        }

        public class DepartmentModel
        {
            public long Id { get; set; }
            public Guid DepartmentCode { get; set; }
            public string DepartmentName { get; set; } = null!;
        }
    }
}
