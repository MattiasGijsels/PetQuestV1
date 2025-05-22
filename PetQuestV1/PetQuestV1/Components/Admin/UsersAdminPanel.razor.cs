using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PetQuestV1.Contracts.Defines;
using PetQuestV1.Contracts.DTOs;

namespace PetQuestV1.Components.Admin
{
    public partial class UsersAdminPanelBase : ComponentBase
    {
        // Define the enum *inside* the class
        public enum SortDirection
        {
            Ascending,
            Descending
        }

        [Inject]
        private IUserService UserService { get; set; } = default!;

        protected List<UserListItemDto> Users { get; set; } = new();

        // Pagination properties
        protected int UsersCurrentPage { get; set; } = 1;
        protected int UsersPageSize { get; set; } = 10;
        protected int UsersTotalPages => Users.Any() ? (int)System.Math.Ceiling((double)Users.Count / UsersPageSize) : 1;
        protected IEnumerable<UserListItemDto> PagedUsers => Users
            .Skip((UsersCurrentPage - 1) * UsersPageSize)
            .Take(UsersPageSize);

        // Sorting properties
        protected string UsersSortColumn { get; set; } = "UserName"; // Default sort column
        protected SortDirection UsersSortDirection { get; set; } = SortDirection.Ascending; // Default sort direction

        protected bool IsUsersSectionVisible { get; set; } = false;

        protected override async Task OnInitializedAsync()
        {
            await LoadUsers();
            ApplySorting(); // Apply initial sorting after loading users
        }

        private async Task LoadUsers()
        {
            Users = await UserService.GetAllUsersWithPetCountsAsync();
            ApplySorting(); // Apply sorting after loading to ensure the initial order is correct
            UsersCurrentPage = System.Math.Clamp(UsersCurrentPage, 1, UsersTotalPages == 0 ? 1 : UsersTotalPages);
            StateHasChanged();
        }

        // Method to handle sorting
        protected void SortUsers(string column)
        {
            if (UsersSortColumn == column)
            {
                // If clicking on the same column, toggle the sort direction
                UsersSortDirection = (UsersSortDirection == SortDirection.Ascending) ? SortDirection.Descending : SortDirection.Ascending;
            }
            else
            {
                // If clicking on a new column, set it as the sort column and default to ascending
                UsersSortColumn = column;
                UsersSortDirection = SortDirection.Ascending;
            }

            ApplySorting();
            ChangeUsersPage(1); // Reset to the first page after sorting
            StateHasChanged();
        }

        // Apply sorting logic to the Users list
        private void ApplySorting()
        {
            if (UsersSortColumn == "UserName")
            {
                Users = (UsersSortDirection == SortDirection.Ascending)
                    ? Users.OrderBy(u => u.UserName).ToList()
                    : Users.OrderByDescending(u => u.UserName).ToList();
            }
            else if (UsersSortColumn == "PetCount")
            {
                Users = (UsersSortDirection == SortDirection.Ascending)
                    ? Users.OrderBy(u => u.PetCount).ToList()
                    : Users.OrderByDescending(u => u.PetCount).ToList();
            }
        }

        // Pagination Handlers
        protected void ChangeUsersPage(int page)
        {
            UsersCurrentPage = page < 1 ? 1 : page > UsersTotalPages ? UsersTotalPages : page;
            StateHasChanged();
        }

        // Section Visibility Toggle Methods
        protected void ToggleUsersSection()
        {
            IsUsersSectionVisible = !IsUsersSectionVisible;
            StateHasChanged();
        }

        // Soft Delete and Restore methods
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