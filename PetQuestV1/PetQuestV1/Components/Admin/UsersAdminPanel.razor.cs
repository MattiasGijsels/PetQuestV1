using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PetQuestV1.Contracts.Defines;
using PetQuestV1.Contracts.DTOs;
using System.ComponentModel.DataAnnotations;
using PetQuestV1.Contracts.Enums;
using Microsoft.AspNetCore.Identity; // For IdentityRole

namespace PetQuestV1.Components.Admin
{
    public partial class UsersAdminPanelBase : ComponentBase
    {
        [Inject]
        private IUserService UserService { get; set; } = default!;

        [Inject]
        private NavigationManager NavigationManager { get; set; } = default!;

        protected List<UserListItemDto> AllUsers { get; set; } = new();
        protected List<IdentityRole> AvailableRoles { get; set; } = new(); // New property for roles dropdown

        protected string SearchTerm { get; set; } = string.Empty;

        protected int UsersCurrentPage { get; set; } = 1;
        protected int UsersPageSize { get; set; } = 10;

        protected IEnumerable<UserListItemDto> FilteredAndSortedUsers
        {
            get
            {
                var query = AllUsers.Where(u => !u.IsDeleted).AsQueryable();

                if (!string.IsNullOrWhiteSpace(SearchTerm))
                {
                    query = query.Where(u =>
                        u.UserName.Contains(SearchTerm, System.StringComparison.OrdinalIgnoreCase) ||
                        u.Email.Contains(SearchTerm, System.StringComparison.OrdinalIgnoreCase) ||
                        u.RoleName.Contains(SearchTerm, System.StringComparison.OrdinalIgnoreCase) // Search by role name too
                    );
                }

                query = UsersSortColumn switch
                {
                    "UserName" => UsersSortDirection == SortDirection.Ascending ? query.OrderBy(u => u.UserName) : query.OrderByDescending(u => u.UserName),
                    "PetCount" => UsersSortDirection == SortDirection.Ascending ? query.OrderBy(u => u.PetCount) : query.OrderByDescending(u => u.PetCount),
                    "RoleName" => UsersSortDirection == SortDirection.Ascending ? query.OrderBy(u => u.RoleName) : query.OrderByDescending(u => u.RoleName), // Sort by role name
                    _ => query.OrderBy(u => u.UserName)
                };

                return query.ToList();
            }
        }

        protected int UsersTotalPages => FilteredAndSortedUsers.Any() ? (int)System.Math.Ceiling((double)FilteredAndSortedUsers.Count() / UsersPageSize) : 1;
        protected IEnumerable<UserListItemDto> PagedUsers => FilteredAndSortedUsers
            .Skip((UsersCurrentPage - 1) * UsersPageSize)
            .Take(UsersPageSize);

        protected string UsersSortColumn { get; set; } = "UserName";
        protected SortDirection UsersSortDirection { get; set; } = SortDirection.Ascending;

        protected bool IsUsersSectionVisible { get; set; } = false;
        protected bool IsUserFormVisible { get; set; } = false;
        protected UserFormDto UserFormModel { get; set; } = new UserFormDto();

        protected override async Task OnInitializedAsync()
        {
            await LoadUsers();
            AvailableRoles = await UserService.GetAllRolesAsync(); // Load roles for dropdown
            UsersCurrentPage = System.Math.Clamp(UsersCurrentPage, 1, UsersTotalPages == 0 ? 1 : UsersTotalPages);
        }

        private async Task LoadUsers()
        {
            AllUsers = await UserService.GetAllUsersWithPetCountsAsync();
        }

        protected void SortUsers(string column)
        {
            if (UsersSortColumn == column)
            {
                UsersSortDirection = (UsersSortDirection == SortDirection.Ascending) ? SortDirection.Descending : SortDirection.Ascending;
            }
            else
            {
                UsersSortColumn = column;
                UsersSortDirection = SortDirection.Ascending;
            }
            ChangeUsersPage(1);
            StateHasChanged();
        }

        protected void ChangeUsersPage(int page)
        {
            UsersCurrentPage = page < 1 ? 1 : page > UsersTotalPages ? UsersTotalPages : page;
            StateHasChanged();
        }

        protected void ToggleUsersSection()
        {
            IsUsersSectionVisible = !IsUsersSectionVisible;
            if (!IsUsersSectionVisible)
            {
                IsUserFormVisible = false;
            }
            StateHasChanged();
        }

        protected async Task SoftDeleteUser(string userId)
        {
            if (IsUserFormVisible && UserFormModel.Id == userId)
            {
                CancelUserForm();
            }
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

        protected async Task EditUser(string userId)
        {
            var userDetail = await UserService.GetUserByIdAsync(userId);
            if (userDetail != null)
            {
                UserFormModel = new UserFormDto
                {
                    Id = userDetail.Id,
                    UserName = userDetail.UserName,
                    Email = userDetail.Email,
                    PetCount = userDetail.PetCount,
                    IsDeleted = userDetail.IsDeleted,
                    SelectedRoleId = userDetail.SelectedRoleId // Set the selected role ID
                };
                IsUserFormVisible = true;
            }
            StateHasChanged();
        }

        protected async Task HandleUserFormSubmit()
        {
            if (UserFormModel.Id == null)
            {
                // For adding new users, you'd typically use UserManager.CreateAsync
                // For now, this panel focuses on editing existing users.
            }
            else
            {
                await UserService.UpdateUserAsync(UserFormModel);
            }

            IsUserFormVisible = false;
            UserFormModel = new UserFormDto();
            await LoadUsers();
            StateHasChanged();
        }

        protected void CancelUserForm()
        {
            IsUserFormVisible = false;
            UserFormModel = new UserFormDto();
            StateHasChanged();
        }

        protected void NavigateToRegisterPage()
        {
            NavigationManager.NavigateTo("/Account/Register");
        }

        protected void OnSearchInput(ChangeEventArgs e)
        {
            SearchTerm = e.Value?.ToString() ?? string.Empty;
            UsersCurrentPage = 1;
            StateHasChanged();
        }

        protected string GetSortIcon(string columnName)
        {
            if (UsersSortColumn != columnName)
            {
                return "bi-arrows-alt";
            }
            return UsersSortDirection == SortDirection.Ascending ? "bi-caret-up-fill" : "bi-caret-down-fill";
        }
    }
}