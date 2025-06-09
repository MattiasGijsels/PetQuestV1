using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using PetQuestV1.Data;
using PetQuestV1.Components.Account;
using PetQuestV1.Components.Account.Shared;

namespace PetQuestV1.Components.Pages
{
    public class HomeBase : ComponentBase
    {
        [Inject]
        protected SignInManager<ApplicationUser> SignInManager { get; set; } = default!;
        [Inject]
        protected ILogger<HomeBase> Logger { get; set; } = default!;
        [Inject]
        protected NavigationManager NavigationManager { get; set; } = default!;
        [Inject]
        protected AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
        [Inject]
        private IdentityRedirectManager RedirectManager { get; set; } = default!;

        [CascadingParameter]
        protected HttpContext HttpContext { get; set; } = default!;

        [SupplyParameterFromForm]
        protected InputModel Input { get; set; } = new();

        [SupplyParameterFromQuery]
        protected string? ReturnUrl { get; set; }

        protected string? errorMessage;

        public bool IsAuthenticated { get; set; }
        public string? Username { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            IsAuthenticated = user.Identity?.IsAuthenticated ?? false;

            if (IsAuthenticated)
            {
                Username = user.Identity?.Name;
            }
            else if (HttpMethods.IsGet(HttpContext.Request.Method))
            {
                await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            }
        }

        public async Task LoginUser()
        {
            var result = await SignInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                Logger.LogInformation("User logged in.");

                if (!string.IsNullOrEmpty(ReturnUrl) && IsLocalUrl(ReturnUrl))
                {
                    RedirectManager.RedirectTo(ReturnUrl);
                }
                else
                {
                    RedirectManager.RedirectTo("/mypets");
                }
            }
            else if (result.RequiresTwoFactor)
            {
                RedirectManager.RedirectTo("Account/LoginWith2fa", new()
                {
                    ["returnUrl"] = ReturnUrl,
                    ["rememberMe"] = Input.RememberMe
                });
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

        private bool IsLocalUrl(string? url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }

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
