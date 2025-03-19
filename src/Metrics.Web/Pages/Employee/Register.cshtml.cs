using Metrics.Application.DTOs;
using Metrics.Application.Services.IServices;
using Metrics.Domain.Entities;
using Metrics.Web.ViewModels.DepartmentViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Metrics.Web.Pages.Employee
{
    public class CreateModel : PageModel
    {
        private readonly IUserAccountService _userAccountService;
        private readonly IDepartmentService _departmentService;

        public CreateModel(IUserAccountService userAccountService, IDepartmentService departmentService)
        {
            _userAccountService = userAccountService;
            _departmentService = departmentService;
            FormInput = new FormInputModel();
            DepartmentListItems = [];
        }

        // ===== Models =====


        public class UserAccount
        {
            public string Username { get; set; } = null!;
        }

        [BindProperty]
        public FormInputModel FormInput { get; set; }

        // [BindProperty]
        // public List<DepartmentGetViewModel> DepartmentList { get; set; }

        // [BindProperty]
        // public List<DepartmentModel> Departments { get; set; }

        [BindProperty]
        public List<SelectListItem> DepartmentListItems { get; set; }

        [BindProperty]
        public long SelectedDepartmentId { get; set; }
        // [BindProperty]
        // public IList<UserAccount> UserAccountList { get; set; }


        public async Task<IActionResult> OnGetAsync()
        {
            var departments = await _departmentService.FindAllInsecure_Async();

            // 
            // Departments = departments.Select(e => new DepartmentModel
            // {
            //     Id = e.Id,
            //     DepartmentCode = e.DepartmentCode,
            //     DepartmentName = e.DepartmentName
            // }).ToList();
            DepartmentListItems = departments.Select(e => new SelectListItem
            {
                Value = e.Id.ToString(),
                Text = e.DepartmentName
            }).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Console.WriteLine($"SelectedDepartmentId: {SelectedDepartmentId}");
            if (!ModelState.IsValid)
            {
                return Page();

            }
            // load departments
            // load accounts
            // save employee
            try
            {
                var createDto = new UserAccountCreateDto
                {
                    // account
                    UserName = FormInput.UserName,
                    Email = FormInput.Email,
                    Password = FormInput.Password,
                    // profile
                    EmployeeCode = FormInput.EmployeeCode,
                    FullName = FormInput.FullName,
                    Address = FormInput.Address,
                    PhoneNumber = FormInput.PhoneNumber,
                    DepartmentId = FormInput.DepartmentId
                    // ApplicationUserId??? <- account not created then id unknown 
                };

                var result = await _userAccountService.RegisterUserAsync(createDto);
                if (result.Succeeded)
                    return RedirectToPage("Success");

                // ModelState.AddModelError("", "")
                // throw new Exception("Account registration failed!");
                return Page();
            }
            catch (System.Exception e)
            {
                // foreach (var error in result.Errors)
                // {
                //     ModelState.AddModelError(string.Empty, error.Description);
                // }
                ModelState.AddModelError(string.Empty, e.Message);

                return Page();
            }
        }

        public class DepartmentModel
        {
            public long Id { get; set; }

            [Required]
            // [StringLength(100)]
            public Guid DepartmentCode { get; set; }

            [Required]
            [StringLength(200)]
            public string DepartmentName { get; set; } = null!;

            // UI-specific properties
            // public bool IsSelected { get; set; }
            // ...
        }
        public class FormInputModel
        {
            [Required]
            [Display(Name = "Username")]
            public string UserName { get; set; } = null!; // Use username instead of email
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; } = null!;
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; } = null!;
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; } = null!;
            [Required]
            [Display(Name = "Employee Code")]
            public string EmployeeCode { get; set; } = null!;
            [Required]
            [Display(Name = "Full Name")]
            public string FullName { get; set; } = null!;
            public string? Address { get; set; } = string.Empty;
            public string? PhoneNumber { get; set; } = string.Empty;
            public long DepartmentId { get; set; }
            public string ApplicationUserId { get; set; } = new Guid().ToString();
        }

    }
}
