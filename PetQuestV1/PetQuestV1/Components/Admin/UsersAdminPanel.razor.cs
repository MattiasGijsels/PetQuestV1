// UsersAdminPanel.razor.cs
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity; // Needed for UserManager
using PetQuestV1.Data; // For ApplicationUser
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection; // REQUIRED for IServiceScopeFactory
using Microsoft.EntityFrameworkCore; // <--- ADD THIS LINE

namespace PetQuestV1.Components.Admin
{
    public partial class UsersAdminPanelBase : ComponentBase
    {
        [Inject]
        private IServiceScopeFactory ScopeFactory { get; set; } = default!;

        protected List<ApplicationUser> Users { get; set; } = new();

        // Pagination users
        protected int UsersCurrentPage { get; set; } = 1;
        protected int UsersPageSize { get; set; } = 10;
        protected int UsersTotalPages => Users.Any() ? (int)System.Math.Ceiling((double)Users.Count / UsersPageSize) : 1;
        protected IEnumerable<ApplicationUser> PagedUsers => Users.Skip((UsersCurrentPage - 1) * UsersPageSize).Take(UsersPageSize);

        // Toggling section visibility
        protected bool IsUsersSectionVisible { get; set; } = true;

        protected override async Task OnInitializedAsync()
        {
            await LoadUsers();
        }

        private async Task LoadUsers()
        {
            using (var scope = ScopeFactory.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                Users = await userManager.Users.ToListAsync(); // This should now work
            }

            UsersCurrentPage = System.Math.Clamp(UsersCurrentPage, 1, UsersTotalPages == 0 ? 1 : UsersTotalPages);
            StateHasChanged();
        }

        // ---------- Pagination Handlers ----------
        protected void ChangeUsersPage(int page)
        {
            UsersCurrentPage = page < 1 ? 1 : page > UsersTotalPages ? UsersTotalPages : page;
            StateHasChanged();
        }

        // ---------- Section Visibility Toggle Methods ----------
        protected void ToggleUsersSection()
        {
            IsUsersSectionVisible = !IsUsersSectionVisible;
            StateHasChanged();
        }

        protected async Task DeleteUser(string userId)
        {
            using (var scope = ScopeFactory.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var userToDelete = await userManager.FindByIdAsync(userId);
                if (userToDelete != null)
                {
                    var result = await userManager.DeleteAsync(userToDelete);
                    if (result.Succeeded)
                    {
                        await LoadUsers();
                    }
                    else
                    {
                        // Handle errors (e.g., display a message)
                    }
                }
            }
        }
    }
}