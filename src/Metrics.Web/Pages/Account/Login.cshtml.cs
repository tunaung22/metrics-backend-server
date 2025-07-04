// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Metrics.Application.Domains;
using Metrics.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Metrics.Web.Pages.Account;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly ILogger<LoginModel> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IUserService _userService;
    private readonly IUserTitleService _userTitleService;

    public LoginModel(
        ILogger<LoginModel> logger,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IUserService userService,
        IUserTitleService userTitleService)
    {
        _logger = logger;
        _userManager = userManager;
        _signInManager = signInManager;
        _userService = userService;
        _userTitleService = userTitleService;
    }

    [BindProperty]
    public bool LoggedIn { get; set; }
    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public IList<AuthenticationScheme> ExternalLogins { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public string ReturnUrl { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [TempData]
    public string ErrorMessage { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InputModel
    {
        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        // [Required]
        // [EmailAddress]
        // public string Email { get; set; }
        [Required]
        // [MaxLength(13)]
        // [MinLength(5)]
        public string Username { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(string returnUrl = null)
    {
        if (User.Identity.IsAuthenticated)
        {
            // Redirect to the home page if logged in
            return RedirectToPage("/Index"); // Adjust the path as needed
        }

        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            ModelState.AddModelError(string.Empty, ErrorMessage);
        }

        returnUrl ??= Url.Content("~/");

        // Clear the existing external cookie to ensure a clean login process
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        ReturnUrl = returnUrl;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
        // returnUrl ??= Url.Content("~/");

        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        if (ModelState.IsValid)
        {
            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, set lockoutOnFailure: true
            var result = await _signInManager.PasswordSignInAsync(
                Input.Username,
                Input.Password,
                Input.RememberMe,
                lockoutOnFailure: false);
            if (result.Succeeded)
            {
                var user = await _userService.FindByUsernameAsync(Input.Username);
                if (user != null)
                {
                    // // Retrieve existing claims
                    // var existingClaims = await _userManager.GetClaimsAsync(user);
                    // // Remove existing claims (optional: you can choose to remove specific claims)
                    // foreach (var claim in existingClaims)
                    // {
                    //     await _userManager.RemoveClaimAsync(user, claim);
                    // }
                    // var claims = new List<Claim>
                    // {
                    // new Claim(ClaimTypes.NameIdentifier, user.Id),
                    // new Claim(ClaimTypes.Name, user.UserName),
                    // new Claim(ClaimTypes.Email, user.Email),
                    // new Claim("FullName", user.FullName ?? string.Empty),
                    // new Claim("UserCode", user.UserCode ?? string.Empty),
                    // new Claim("ContactAddress", user.ContactAddress ?? string.Empty),
                    // new Claim("PhoneNumber", user.PhoneNumber ?? string.Empty),
                    // new Claim("DepartmentName", user.Department?.DepartmentName ?? string.Empty),
                    // new Claim("TitleName", userTitle?.TitleName ?? string.Empty)
                    // };
                    // var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    // var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                    // await _signInManager.SignInAsync(user, isPersistent: Input.RememberMe);

                    // await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                    // var claims = new List<Claim>
                    // {
                    //     new Claim("UserGroup", user.UserTitle.TitleName.ToString() ?? string.Empty)
                    // };


                    // Check for missing claims
                    var claims = await _userManager.GetClaimsAsync(user);

                    if (!claims.Any(c => c.Type == "UserGroup"))
                    {
                        await _userManager.AddClaimAsync(user, new Claim("UserGroup", user.UserTitle.TitleName.ToString()));
                    }

                    await _signInManager.SignInWithClaimsAsync(user, Input.RememberMe, claims);

                    var roles = await _userManager.GetRolesAsync(user);
                    if (roles.Contains("Admin"))
                        returnUrl ??= Url.Content("~/Dashboard");
                    else
                        returnUrl ??= Url.Content("~/");

                    LoggedIn = true;

                    _logger.LogInformation("User logged in.");
                    return LocalRedirect(returnUrl);
                }
            }
            if (result.RequiresTwoFactor)
            {
                return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
            }
            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Account is locked! Please contact Administrator.");
                _logger.LogWarning("User account locked out.");
                ViewData["Message"] = "<div>Locked!</div><div>Your account is locked. Please contact Administrator.</div>";
                // **Workaround
                // return RedirectToPage("./Locked/Index");
                return Page();
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Username or password invalid! Try again.");
                return Page();
            }
        }

        return Page();
    }
}
