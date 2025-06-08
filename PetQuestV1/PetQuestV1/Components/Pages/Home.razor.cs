using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using PetQuestV1.Data;
using PetQuestV1.Components.Account;
using PetQuestV1.Components.Account.Shared;


namespace PetQuestV1.Components.Pages
{
    // Make sure your namespace matches your project structure
    public class HomeBase : ComponentBase
    {
        [Inject]
        protected SignInManager<ApplicationUser> SignInManager { get; set; } = default!;
        [Inject]
        protected ILogger<HomeBase> Logger { get; set; } = default!;
        [Inject]
        protected NavigationManager NavigationManager { get; set; } = default!;
        [Inject]
        private IdentityRedirectManager RedirectManager { get; set; } = default!; // Changed to private

        protected string? errorMessage;

        [CascadingParameter]
        protected HttpContext HttpContext { get; set; } = default!;

        [SupplyParameterFromForm]
        protected InputModel Input { get; set; } = new();

        [SupplyParameterFromQuery]
        protected string? ReturnUrl { get; set; }

        protected override async Task OnInitializedAsync()
        {
            if (HttpMethods.IsGet(HttpContext.Request.Method))
            {
                // Clear the existing external cookie to ensure a clean login process
                await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            }
        }

        public async Task LoginUser()
        {
            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, set lockoutOnFailure: true
            var result = await SignInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                Logger.LogInformation("User logged in.");

                // Check if ReturnUrl is set and if it's a local URL
                if (!string.IsNullOrEmpty(ReturnUrl) && IsLocalUrl(ReturnUrl))
                {
                    RedirectManager.RedirectTo(ReturnUrl);
                }
                else
                {
                    RedirectManager.RedirectTo("/mypets"); // Redirect to /mypets by default
                }
            }
            else if (result.RequiresTwoFactor)
            {
                RedirectManager.RedirectTo(
                    "Account/LoginWith2fa",
                    new() { ["returnUrl"] = ReturnUrl, ["rememberMe"] = Input.RememberMe });
            }
            else if (result.IsLockedOut)
            {
                Logger.LogWarning("User account locked out.");
                RedirectManager.RedirectTo("Account/Lockout");
            }
            else
            {
                errorMessage = "Error: Invalid login attempt.";
            }
        }

        // Helper method to check if a URL is local
        private bool IsLocalUrl(string? url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }
            // Fix for CA1866: Changed "/" to '/'
            return (url.StartsWith('/') && !url.StartsWith("//")) ||
                   (url.StartsWith(NavigationManager.BaseUri) && !url.Contains("://"));
        }

        protected sealed class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = "";

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; } = "";

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }
    }
}