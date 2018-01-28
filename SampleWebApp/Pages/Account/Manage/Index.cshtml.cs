using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SampleWebApp.Data;
using SampleWebApp.Attributes;
using SampleWebApp.Services;
using System.IO;

namespace SampleWebApp.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        public string Username { get; set; }

        public bool IsEmailConfirmed { get; set; }

        public string ProfileImageSrc { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

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

            //[FileExtensions(Extensions = "jpg,jpeg,png,gif", ErrorMessage = "Invalid image format. Must be jpg, jpeg, png, or gif.")]
            [DataType(DataType.Upload)]
            [Display(Name = "Profile Image")]
            public IFormFile ProfileImage { get; set; }

            [Required]
            [MinimumAge(18, ErrorMessage = "Must be at least 18 years of age.")]
            [Display(Name = "Date of Birth")]
            [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
            public DateTime? DateOfBirth { get; set; }

            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            Username = user.UserName;
            ProfileImageSrc = $"/api/ProfileImage/{user.Id}";

            Input = new InputModel
            {
                AccountType = user.AccountType,
                CompanyName = user.CompanyName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                DateOfBirth = user.DateOfBirth,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };

            IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if(Input.AccountType != user.AccountType)
            {
                user.AccountType = Input.AccountType;
                var updateAccountTypeResult = await _userManager.UpdateAsync(user);
                if(!updateAccountTypeResult.Succeeded)
                {
                    throw new ApplicationException($"Unexpected error occurred while updating the account type for user with ID '{user.Id}'.");
                }
            }

            if (Input.CompanyName != user.CompanyName)
            {
                user.CompanyName = Input.CompanyName;
                var updateCompanyNameResult = await _userManager.UpdateAsync(user);
                if (!updateCompanyNameResult.Succeeded)
                {
                    throw new ApplicationException($"Unexpected error occurred while updating the company name for user with ID '{user.Id}'.");
                }
            }

            if (Input.FirstName != user.FirstName)
            {
                user.FirstName = Input.FirstName;
                var updateFirstNameResult = await _userManager.UpdateAsync(user);
                if (!updateFirstNameResult.Succeeded)
                {
                    throw new ApplicationException($"Unexpected error occurred while updating the first name for user with ID '{user.Id}'.");
                }
            }

            if (Input.LastName != user.LastName)
            {
                user.LastName = Input.LastName;
                var updateLastNameResult = await _userManager.UpdateAsync(user);
                if (!updateLastNameResult.Succeeded)
                {
                    throw new ApplicationException($"Unexpected error occurred while updating the last name for user with ID '{user.Id}'.");
                }
            }

            if (Input.ProfileImage != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await Input.ProfileImage.CopyToAsync(memoryStream);
                    var profileImage = memoryStream.ToArray();

                    if (profileImage != user.ProfileImage)
                    {
                        user.ProfileImage = profileImage;
                        var updateProfileImageResult = await _userManager.UpdateAsync(user);
                        if (!updateProfileImageResult.Succeeded)
                        {
                            throw new ApplicationException($"Unexpected error occurred while updating the profile image for user with ID '{user.Id}'.");
                        }
                    }
                }
            }

            if (Input.DateOfBirth != user.DateOfBirth)
            {
                user.DateOfBirth = Input.DateOfBirth;
                var updateDateOfBirthResult = await _userManager.UpdateAsync(user);
                if (!updateDateOfBirthResult.Succeeded)
                {
                    throw new ApplicationException($"Unexpected error occurred while updating the date of birth for user with ID '{user.Id}'.");
                }
            }

            if (Input.Email != user.Email)
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, Input.Email);
                if (!setEmailResult.Succeeded)
                {
                    throw new ApplicationException($"Unexpected error occurred setting email for user with ID '{user.Id}'.");
                }
            }

            if (Input.PhoneNumber != user.PhoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    throw new ApplicationException($"Unexpected error occurred setting phone number for user with ID '{user.Id}'.");
                }
            }

            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
        public async Task<IActionResult> OnPostSendVerificationEmailAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);
            await _emailSender.SendEmailConfirmationAsync(user.Email, callbackUrl);

            StatusMessage = "Verification email sent. Please check your email.";
            return RedirectToPage();
        }
    }
}
