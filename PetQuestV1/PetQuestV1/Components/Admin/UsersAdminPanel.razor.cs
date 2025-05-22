using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PetQuestV1.Contracts.Defines;
using PetQuestV1.Contracts.DTOs;
using System.ComponentModel.DataAnnotations; // Needed for DataAnnotationsValidator in the razor file, best to include it here too for context


namespace PetQuestV1.Components.Admin
{
    // If you are using a separate code-behind file, it should look like this:
    // public partial class UsersAdminPanel : ComponentBase // If UsersAdminPanel.razor.cs
    // If you are using a base class for inheritance:
    public partial class UsersAdminPanelBase : ComponentBase
    {
        // Enum for sorting direction
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
        // Calculates total pages, ensures at least 1 page if no users
        protected int UsersTotalPages => Users.Any() ? (int)System.Math.Ceiling((double)Users.Count / UsersPageSize) : 1;
        // Returns the subset of users for the current page
        protected IEnumerable<UserListItemDto> PagedUsers => Users
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
        }

        // Loads users from the service, applies current sorting, and updates pagination
        private async Task LoadUsers()
        {
            Users = await UserService.GetAllUsersWithPetCountsAsync();
            ApplySorting(); // Ensure sorting is applied after loading
            // Adjust current page if the total pages change (e.g., after filter/sort)
            UsersCurrentPage = System.Math.Clamp(UsersCurrentPage, 1, UsersTotalPages == 0 ? 1 : UsersTotalPages);
            StateHasChanged(); // Notify Blazor component that state has changed
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

            ApplySorting();     // Re-apply sorting to the list
            ChangeUsersPage(1); // Reset to first page after sorting to see results immediately
            StateHasChanged();  // Notify Blazor component
        }

        // Applies sorting logic to the Users list based on current sort column and direction
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
            // Add more sorting logic for other columns if you introduce them
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
    }
}