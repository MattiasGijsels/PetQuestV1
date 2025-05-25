// PetQuestV1/Components/Admin/SpeciesAdminPanel.razor.cs
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using PetQuestV1.Contracts.Defines; // For ISpeciesService
using PetQuestV1.Contracts.Models; // For Species model
using PetQuestV1.Contracts.Enums; // <--- NEW: For SortDirection enum
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace PetQuestV1.Components.Admin
{
    public partial class SpeciesAdminPanelBase : ComponentBase
    {
        [Inject]
        private IServiceScopeFactory ScopeFactory { get; set; } = default!;

        protected List<Species> AllSpecies { get; set; } = new();

        // --- Sorting Properties ---
        protected string CurrentSortColumn { get; set; } = "SpeciesName"; // Default sort column
        protected SortDirection SortDirection { get; set; } = SortDirection.Ascending; // Now uses the global enum

        // --- Search Property ---
        protected string SearchTerm { get; set; } = string.Empty;

        // Pagination properties
        protected int SpeciesCurrentPage { get; set; } = 1;
        protected int SpeciesPageSize { get; set; } = 10;

        // Computed property for filtered and sorted species
        protected IEnumerable<Species> FilteredAndSortedSpecies
        {
            get
            {
                // Filter out IsDeleted species first in the UI display.
                // The DbContext global filter will also handle this server-side, but this ensures
                // consistency in UI filtering for existing `AllSpecies` list.
                var query = AllSpecies.Where(s => !s.IsDeleted).AsQueryable();

                // Apply Search Filter
                if (!string.IsNullOrWhiteSpace(SearchTerm))
                {
                    query = query.Where(s =>
                        s.SpeciesName.Contains(SearchTerm, System.StringComparison.OrdinalIgnoreCase)
                    );
                }

                // Apply Sorting
                query = CurrentSortColumn switch
                {
                    "SpeciesName" => SortDirection == SortDirection.Ascending ? query.OrderBy(s => s.SpeciesName) : query.OrderByDescending(s => s.SpeciesName),
                    _ => query.OrderBy(s => s.SpeciesName) // Default sort
                };

                return query.ToList(); // Materialize the filtered and sorted list
            }
        }

        protected int SpeciesTotalPages => FilteredAndSortedSpecies.Any() ? (int)System.Math.Ceiling((double)FilteredAndSortedSpecies.Count() / SpeciesPageSize) : 1;
        protected IEnumerable<Species> PagedSpecies => FilteredAndSortedSpecies.Skip((SpeciesCurrentPage - 1) * SpeciesPageSize).Take(SpeciesPageSize);

        // Forms & UI state for species management
        protected Species SpeciesFormModel { get; set; } = new(); // Use Species model directly for the form
        protected bool IsSpeciesFormVisible { get; set; } = false;
        private bool IsEditing { get; set; } = false;

        // Toggling section visibility for "Species" accordion
        protected bool IsSpeciesSectionVisible { get; set; } = false; // Default to false (collapsed)

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
            // Ensure pagination is correct after initial load
            SpeciesCurrentPage = System.Math.Clamp(SpeciesCurrentPage, 1, SpeciesTotalPages == 0 ? 1 : SpeciesTotalPages);
        }

        private async Task LoadData()
        {
            using (var scope = ScopeFactory.CreateScope())
            {
                var speciesService = scope.ServiceProvider.GetRequiredService<ISpeciesService>();
                AllSpecies = await speciesService.GetAllAsync();
            }
            StateHasChanged(); // Refresh UI after loading data
        }

        protected void ToggleSpeciesSection()
        {
            IsSpeciesSectionVisible = !IsSpeciesSectionVisible;
            StateHasChanged();
        }

        protected void ShowAddSpeciesForm()
        {
            SpeciesFormModel = new Species(); // Initialize with a new Species object
            IsEditing = false;
            IsSpeciesFormVisible = true;
            StateHasChanged();
        }

        protected void EditSpecies(Species species)
        {
            // Create a copy to avoid modifying the list directly until saved
            // This is good practice to prevent accidental UI updates before a successful save
            SpeciesFormModel = new Species
            {
                Id = species.Id,
                SpeciesName = species.SpeciesName,
                IsDeleted = species.IsDeleted // Retain its current IsDeleted state
            };
            IsEditing = true;
            IsSpeciesFormVisible = true;
            StateHasChanged();
        }

        protected async Task HandleSpeciesFormSubmit()
        {
            // DataAnnotationsValidator handles basic validation on the form.
            // If more complex validation is needed, add it here or in the service layer.

            using (var scope = ScopeFactory.CreateScope())
            {
                var speciesService = scope.ServiceProvider.GetRequiredService<ISpeciesService>();

                if (IsEditing)
                {
                    await speciesService.UpdateAsync(SpeciesFormModel);
                }
                else
                {
                    await speciesService.AddAsync(SpeciesFormModel);
                }
            }

            IsSpeciesFormVisible = false;
            SpeciesFormModel = new Species(); // Reset form model for next use
            await LoadData(); // Reload all data after submit to reflect changes
            StateHasChanged();
        }

        protected void CancelSpeciesForm()
        {
            IsSpeciesFormVisible = false;
            SpeciesFormModel = new Species(); // Clear form
            StateHasChanged();
        }

        protected async Task SoftDeleteSpecies(string id)
        {
            using (var scope = ScopeFactory.CreateScope())
            {
                var speciesService = scope.ServiceProvider.GetRequiredService<ISpeciesService>();
                await speciesService.SoftDeleteAsync(id);
            }
            await LoadData(); // Reload all data to reflect the soft deletion
            StateHasChanged();
        }

        // ---------- Pagination Handlers ----------
        protected void ChangeSpeciesPage(int page)
        {
            SpeciesCurrentPage = System.Math.Clamp(page, 1, SpeciesTotalPages);
            StateHasChanged();
        }

        // ---------- Sorting Handlers ----------
        protected void SortBy(string columnName)
        {
            if (CurrentSortColumn == columnName)
            {
                SortDirection = (SortDirection == SortDirection.Ascending) ? SortDirection.Descending : SortDirection.Ascending;
            }
            else
            {
                CurrentSortColumn = columnName;
                SortDirection = SortDirection.Ascending; // Default to ascending when changing column
            }
            SpeciesCurrentPage = 1; // Reset to first page on sort change
            StateHasChanged();
        }

        protected string GetSortIcon(string columnName)
        {
            if (CurrentSortColumn != columnName)
            {
                return "bi-arrows-alt"; // Neutral icon
            }
            return SortDirection == SortDirection.Ascending ? "bi-caret-up-fill" : "bi-caret-down-fill";
        }

        protected void OnSearchInput(ChangeEventArgs e)
        {
            SearchTerm = e.Value?.ToString() ?? string.Empty;
            SpeciesCurrentPage = 1; // Reset to first page on search
            StateHasChanged();
        }

        // The enum SortDirection definition has been moved to PetQuestV1/Contracts/Enums/SortDirection.cs
    }
}