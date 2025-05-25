using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PetQuestV1.Contracts.Defines;
using PetQuestV1.Contracts.DTOs;
using System.ComponentModel.DataAnnotations;
using PetQuestV1.Contracts.Enums; // <--- Add this using directive

namespace PetQuestV1.Components.Admin
{
    public partial class UsersAdminPanelBase : ComponentBase
    {

        [Inject]
        private IUserService UserService { get; set; } = default!;

        [Inject]
        private NavigationManager NavigationManager { get; set; } = default!; // Inject NavigationManager

        // Rename Users to AllUsers to be consistent with SpeciesAdminPanel
        // This will hold the raw data loaded from the service
        protected List<UserListItemDto> AllUsers { get; set; } = new();


        // --- Search Property ---
        protected string SearchTerm { get; set; } = string.Empty; // Add SearchTerm property


        // Pagination properties
        protected int UsersCurrentPage { get; set; } = 1;
        protected int UsersPageSize { get; set; } = 10;

        // Computed property for filtered and sorted users
        protected IEnumerable<UserListItemDto> FilteredAndSortedUsers
        {
            get
            {
                // Start with all users that are not deleted
                var query = AllUsers.Where(u => !u.IsDeleted).AsQueryable();

                // Apply Search Filter
                if (!string.IsNullOrWhiteSpace(SearchTerm))
                {
                    query = query.Where(u =>
                        u.UserName.Contains(SearchTerm, System.StringComparison.OrdinalIgnoreCase) ||
                        u.Email.Contains(SearchTerm, System.StringComparison.OrdinalIgnoreCase)
                    );
                }

                // Apply Sorting
                query = UsersSortColumn switch
                {
                    "UserName" => UsersSortDirection == SortDirection.Ascending ? query.OrderBy(u => u.UserName) : query.OrderByDescending(u => u.UserName),
                    "PetCount" => UsersSortDirection == SortDirection.Ascending ? query.OrderBy(u => u.PetCount) : query.OrderByDescending(u => u.PetCount),
                    _ => query.OrderBy(u => u.UserName) // Default sort
                };

                return query.ToList();
            }
        }

        // Calculates total pages, ensures at least 1 page if no users
        protected int UsersTotalPages => FilteredAndSortedUsers.Any() ? (int)System.Math.Ceiling((double)FilteredAndSortedUsers.Count() / UsersPageSize) : 1;
        // Returns the subset of users for the current page
        protected IEnumerable<UserListItemDto> PagedUsers => FilteredAndSortedUsers
            .Skip((UsersCurrentPage - 1) * UsersPageSize)
            .Take(UsersPageSize);

        // Sorting properties
        protected string UsersSortColumn { get; set; } = "UserName"; // Default sort column
        protected SortDirection UsersSortDirection { get; set; } = SortDirection.Ascending; // Default sort direction

        // Visibility property for the entire Users section
        protected bool IsUsersSectionVisible { get; set; } = false;

        // --- NEW/MODIFIED PROPERTIES FOR EDITING FORM (mimicking PetsAdminPanel) ---
        protected bool IsUserFormVisible { get; set; } = false; // Controls visibility of the embedded edit/add form
        protected UserFormDto UserFormModel { get; set; } = new UserFormDto(); // Model for the form's input fields

        protected override async Task OnInitializedAsync()
        {
            await LoadUsers();
            // Ensure current page is valid after initial load
            UsersCurrentPage = System.Math.Clamp(UsersCurrentPage, 1, UsersTotalPages == 0 ? 1 : UsersTotalPages);
        }

        // Loads users from the service, applies current sorting, and updates pagination
        private async Task LoadUsers()
        {
            AllUsers = await UserService.GetAllUsersWithPetCountsAsync();
            // No need to call ApplySorting here, as FilteredAndSortedUsers computed property handles it.
            // Also, no need to call StateHasChanged here, as it's called at the end of OnInitializedAsync or calling methods.
        }

        // Method to handle sorting column click
        protected void SortUsers(string column)
        {
            if (UsersSortColumn == column)
            {
                // If clicking the same column, toggle sort direction
                UsersSortDirection = (UsersSortDirection == SortDirection.Ascending) ? SortDirection.Descending : SortDirection.Ascending;
            }
            else
            {
                // If clicking a new column, set it as sort column and default to ascending
                UsersSortColumn = column;
                UsersSortDirection = SortDirection.Ascending;
            }

            // The FilteredAndSortedUsers computed property will automatically re-evaluate
            ChangeUsersPage(1); // Reset to first page after sorting to see results immediately
            StateHasChanged();  // Notify Blazor component
        }

        // Pagination Handlers: Changes the current page
        protected void ChangeUsersPage(int page)
        {
            UsersCurrentPage = page < 1 ? 1 : page > UsersTotalPages ? UsersTotalPages : page;
            StateHasChanged();
        }

        // Section Visibility Toggle Methods: Shows/hides the entire user panel content
        protected void ToggleUsersSection()
        {
            IsUsersSectionVisible = !IsUsersSectionVisible;
            // When collapsing the section, ensure the form is also hidden
            if (!IsUsersSectionVisible)
            {
                IsUserFormVisible = false;
            }
            StateHasChanged();
        }

        // Soft Delete method: Marks a user as deleted
        protected async Task SoftDeleteUser(string userId)
        {
            // If the user being deleted is the one currently in the form, close the form
            if (IsUserFormVisible && UserFormModel.Id == userId)
            {
                CancelUserForm();
            }
            await UserService.SoftDeleteUserAsync(userId);
            await LoadUsers(); // Reload users to show the updated status
            StateHasChanged();
        }

        // Restore method: Un-marks a user as deleted
        protected async Task RestoreUser(string userId)
        {
            await UserService.RestoreUserAsync(userId);
            await LoadUsers(); // Reload users to show the updated status
            StateHasChanged();
        }

        // --- EDITING METHODS (aligned with PetsAdminPanel's form logic) ---

        // Method to initiate editing a user: fetches data, populates form model, shows form
        protected async Task EditUser(string userId)
        {
            // Fetch the detailed user data for editing
            var userDetail = await UserService.GetUserByIdAsync(userId);
            if (userDetail != null)
            {
                // Map the fetched UserDetailDto to the UserFormDto for the form
                UserFormModel = new UserFormDto
                {
                    Id = userDetail.Id,
                    UserName = userDetail.UserName,
                    Email = userDetail.Email,
                    PetCount = userDetail.PetCount, // Be cautious: PetCount is usually derived, not directly editable
                    IsDeleted = userDetail.IsDeleted
                    // Map any other relevant properties from UserDetailDto to UserFormDto
                };
                IsUserFormVisible = true; // Show the embedded edit form
            }
            StateHasChanged();
        }

        // Handles the submission of the user edit/add form
        protected async Task HandleUserFormSubmit()
        {
            // Determine if it's an existing user (edit) or a new user (add)
            if (UserFormModel.Id == null)
            {
                // Logic for adding a new user (if you enable an "Add User" button later)
                // You would typically call a UserService.CreateUserAsync(UserFormModel) here
                // For now, this branch won't be hit unless you add a create button.
                // Example: await UserService.CreateUserAsync(UserFormModel);
            }
            else
            {
                // Logic for updating an existing user
                await UserService.UpdateUserAsync(UserFormModel); // Calls the updated service method
            }

            IsUserFormVisible = false;      // Hide the form after successful submission
            UserFormModel = new UserFormDto(); // Reset the form model to a clean state
            await LoadUsers();              // Reload the user list to reflect the changes
            StateHasChanged();
        }

        // Handles cancelling the form: hides it and resets the model
        protected void CancelUserForm()
        {
            IsUserFormVisible = false;      // Hide the form
            UserFormModel = new UserFormDto(); // Clear the form model
            StateHasChanged();
        }

        /// <summary>
        /// Redirects the user to the registration page.
        /// </summary>
        protected void NavigateToRegisterPage()
        {
            // CORRECTED PATH: Use the same path as in NavMenu.razor
            NavigationManager.NavigateTo("/Account/Register");
        }

        // --- Search Handler (New Method) ---
        protected void OnSearchInput(ChangeEventArgs e)
        {
            SearchTerm = e.Value?.ToString() ?? string.Empty;
            UsersCurrentPage = 1; // Reset to first page on search
            StateHasChanged(); // Trigger re-render to apply filter
        }

        // Add this helper method to get the sort icon based on current sort state
        protected string GetSortIcon(string columnName)
        {
            if (UsersSortColumn != columnName)
            {
                return "bi-arrows-alt"; // Neutral icon when not sorting by this column
            }
            return UsersSortDirection == SortDirection.Ascending ? "bi-caret-up-fill" : "bi-caret-down-fill";
        }
    }
}