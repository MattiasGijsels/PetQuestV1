using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PetQuestV1.Contracts.Defines;
using PetQuestV1.Contracts.DTOs; // Add this using directive for the new DTO

namespace PetQuestV1.Components.Admin
{
    public partial class UsersAdminPanelBase : ComponentBase
    {
        [Inject]
        private IUserService UserService { get; set; } = default!;

        // Change the list type to the new DTO
        protected List<UserListItemDto> Users { get; set; } = new();

        // Pagination properties remain the same, but will operate on UserListItemDto
        protected int UsersCurrentPage { get; set; } = 1;
        protected int UsersPageSize { get; set; } = 10;
        protected int UsersTotalPages => Users.Any() ? (int)System.Math.Ceiling((double)Users.Count / UsersPageSize) : 1;
        protected IEnumerable<UserListItemDto> PagedUsers => Users.Skip((UsersCurrentPage - 1) * UsersPageSize).Take(UsersPageSize);

        protected bool IsUsersSectionVisible { get; set; } = false;

        protected override async Task OnInitializedAsync()
        {
            await LoadUsers();
        }

        private async Task LoadUsers()
        {
            // Call the new method to get users with pet counts
            Users = await UserService.GetAllUsersWithPetCountsAsync();
            UsersCurrentPage = System.Math.Clamp(UsersCurrentPage, 1, UsersTotalPages == 0 ? 1 : UsersTotalPages);
            StateHasChanged();
        }

        // Pagination Handlers - no changes needed, as they operate on the list
        protected void ChangeUsersPage(int page)
        {
            UsersCurrentPage = page < 1 ? 1 : page > UsersTotalPages ? UsersTotalPages : page;
            StateHasChanged();
        }

        // Section Visibility Toggle Methods - no changes needed
        protected void ToggleUsersSection()
        {
            IsUsersSectionVisible = !IsUsersSectionVisible;
            StateHasChanged();
        }

        // Soft Delete and Restore methods remain the same as they operate by userId
        protected async Task SoftDeleteUser(string userId)
        {
            await UserService.SoftDeleteUserAsync(userId);
            await LoadUsers();
            StateHasChanged();
        }

        protected async Task RestoreUser(string userId)
        {
            await UserService.RestoreUserAsync(userId);
            await LoadUsers();
            StateHasChanged();
        }
    }
}