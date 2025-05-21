// UsersAdminPanel.razor.cs
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity; // Still useful for general Identity context, though direct UserManager isn't injected here
using PetQuestV1.Data; // For ApplicationUser
using PetQuestV1.Contracts; // For IUserService (NEW!)
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
// Removed Microsoft.Extensions.DependencyInjection as IServiceScopeFactory is no longer directly injected
// Removed Microsoft.EntityFrameworkCore as ToListAsync() will be handled by UserService.GetAllUsersAsync()


namespace PetQuestV1.Components.Admin
{
    public partial class UsersAdminPanelBase : ComponentBase
    {
        // Inject your new IUserService directly
        [Inject]
        private IUserService UserService { get; set; } = default!;

        protected List<ApplicationUser> Users { get; set; } = new();

        // Pagination properties
        protected int UsersCurrentPage { get; set; } = 1;
        protected int UsersPageSize { get; set; } = 10;
        protected int UsersTotalPages => Users.Any() ? (int)System.Math.Ceiling((double)Users.Count / UsersPageSize) : 1;
        protected IEnumerable<ApplicationUser> PagedUsers => Users.Skip((UsersCurrentPage - 1) * UsersPageSize).Take(UsersPageSize);

        // Toggling section visibility
        protected bool IsUsersSectionVisible { get; set; } = false;

        protected override async Task OnInitializedAsync()
        {
            await LoadUsers();
        }

        private async Task LoadUsers()
        {
            // Use the injected UserService to get all users
            // The global query filter in DbContext will automatically exclude soft-deleted users.
            Users = await UserService.GetAllUsersAsync();
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

        // --- NEW: Soft Delete User method ---
        protected async Task SoftDeleteUser(string userId)
        {
            // Here we call the SoftDeleteUserAsync method from our IUserService
            await UserService.SoftDeleteUserAsync(userId);
            await LoadUsers(); // Reload users to update the UI
            StateHasChanged();
        }

        // --- Optional: Restore User method ---
        // You would need to add a way to see soft-deleted users in your UI
        // to make this useful (e.g., a "Show Deleted Users" toggle that calls
        // a different UserService method which ignores query filters).
        protected async Task RestoreUser(string userId)
        {
            await UserService.RestoreUserAsync(userId);
            await LoadUsers(); // Reload users to update the UI
            StateHasChanged();
        }


        // IMPORTANT: The original DeleteUser method (which performed a hard delete)
        // is being removed or renamed to avoid accidental hard deletion.
        // If you absolutely need a hard delete option, ensure it's heavily protected
        // and clearly distinguished from soft delete.
        // protected async Task DeleteUser(string userId) { /* Removed or renamed */ }
    }
}