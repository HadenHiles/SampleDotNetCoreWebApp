using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SampleWebApp.Data;
using SampleWebApp.Attributes;
using SampleWebApp.Services;
using System.IO;

namespace SampleWebApp.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<LoginModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Account Type")]
            public string AccountType { get; set; }

            // This is nice but isn't supported client side automatically
            [RequiredIf("AccountType == \"business\"", ErrorMessage = "Company Name is required.")]
            [StringLength(60, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
            [Display(Name = "Company Name")]
            public string CompanyName { get; set; }
            
            [RequiredIf("AccountType == \"personal\"", ErrorMessage = "First Name is required.")]
            [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }
            
            [RequiredIf("AccountType == \"personal\"", ErrorMessage = "Last Name is required.")]
            [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [FileExtensions(Extensions = "jpg,jpeg,png,gif", ErrorMessage = "Invalid image format. Must be jpg, jpeg, png, or gif.")]
            [DataType(DataType.Upload)]
            [Display(Name = "Profile Image")]
            public IFormFile ProfileImage { get; set; }

            [Required]
            [MinimumAge(18, ErrorMessage = "Must be at least 18 years of age.")]
            [Display(Name = "Date of Birth")]
            [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
            public DateTime? DateOfBirth { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public IActionResult OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl;

            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToPage("/Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            if (ModelState.IsValid)
            {
                using (var memoryStream = new MemoryStream())
                {
                    var user = new ApplicationUser
                    {
                        AccountType = Input.AccountType,
                        CompanyName = Input.CompanyName,
                        FirstName = Input.FirstName,
                        LastName = Input.LastName,
                        DateOfBirth = Input.DateOfBirth,
                        UserName = Input.Email,
                        Email = Input.Email
                    };

                    if (Input.ProfileImage != null)
                    {
                        await Input.ProfileImage.CopyToAsync(memoryStream);
                        byte[] profileImage = memoryStream.ToArray();
                        user.ProfileImage = profileImage;
                    }
                    
                    var result = await _userManager.CreateAsync(user, Input.Password);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User created a new account with password.");

                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        var callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);
                        await _emailSender.SendEmailConfirmationAsync(Input.Email, callbackUrl);

                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(Url.GetLocalUrl(returnUrl));
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
