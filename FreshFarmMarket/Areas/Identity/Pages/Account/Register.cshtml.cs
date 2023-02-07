// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using FreshFarmMarket.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using System.Drawing;

namespace FreshFarmMarket.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<FreshFarmMarketUser> _signInManager;
        private readonly UserManager<FreshFarmMarketUser> _userManager;
        private readonly IUserStore<FreshFarmMarketUser> _userStore;
        private readonly IUserEmailStore<FreshFarmMarketUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private IWebHostEnvironment _environment;
        private IEmailSender _emailSender;

        public RegisterModel(
            UserManager<FreshFarmMarketUser> userManager,
            IUserStore<FreshFarmMarketUser> userStore,
            SignInManager<FreshFarmMarketUser> signInManager,
            ILogger<RegisterModel> logger,
            IWebHostEnvironment environment,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _environment = environment;
            _emailSender = emailSender;
        }

        [BindProperty]
        public IFormFile? Upload { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }


        public string ReturnUrl { get; set; }


        public IList<AuthenticationScheme> ExternalLogins { get; set; }


        public class InputModel
        {
            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "FullName")]
            public string FullName { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.CreditCard)]
            [StringLength(16, ErrorMessage ="Credit Card Number should only have 16 characters")]
            [MinLength(16, ErrorMessage = "Credit Card Number should have 16 characters")]
            [Display(Name = "CreditCardNo")]
            public string CreditCardNo { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Gender")]
            public string Gender { get; set; } = string.Empty;
            [Required]
            [DataType(DataType.PhoneNumber)]
            [StringLength(8, ErrorMessage = "Phone Number should only have 8 numbers")]
            [MinLength(8, ErrorMessage = "Phone Number should have 8 numbers")]
            [Display(Name = "PhoneNumber")]
            public string PhoneNumber { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "DeliveryAddress")]
            public string DeliveryAddress { get; set; } = string.Empty;

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required]
            [DataType(DataType.Upload)]
            [Display(Name = "Photo")]
            public string Photo { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "AboutMe")]
            public string AboutMe { get; set; } = string.Empty;
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            if (ModelState.IsValid)
            {
                if (Upload != null)
                {
                    if (Upload.Length > 2 * 1024 * 1024)
                    {
                        ModelState.AddModelError("Upload",
                        "File size cannot exceed 2MB.");
                        return Page();
                    }
                    var uploadsFolder = "uploads";
                    var imageFile = Guid.NewGuid() + Path.GetExtension(
                    Upload.FileName);
                    var imagePath = Path.Combine(_environment.ContentRootPath,
                    "wwwroot", uploadsFolder, imageFile);
                    using var fileStream = new FileStream(imagePath,
                    FileMode.Create);
                    await Upload.CopyToAsync(fileStream);
                    Input.Photo = string.Format("/{0}/{1}", uploadsFolder,
                    imageFile);
                }
                var user = CreateUser();

                user.FullName = Input.FullName;
                user.CreditCardNo = Input.CreditCardNo;
                user.Gender = Input.Gender;
                user.PhoneNumber = Input.PhoneNumber;
                user.DeliveryAddress = Input.DeliveryAddress;
                user.Photo = Input.Photo;
                user.AboutMe = Input.AboutMe;

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private FreshFarmMarketUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<FreshFarmMarketUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(FreshFarmMarketUser)}'. " +
                    $"Ensure that '{nameof(FreshFarmMarketUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<FreshFarmMarketUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<FreshFarmMarketUser>)_userStore;
        }
    }
}
