// PetQuestV1/Components/Admin/SpeciesAdminPanel.razor.cs
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using PetQuestV1.Contracts.Defines;
using PetQuestV1.Contracts.Models; // Ensure Species and SpeciesWithBreedCountDto are available
using PetQuestV1.Contracts.Enums;
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

        // Change the type of AllSpecies to the new DTO
        protected List<SpeciesWithBreedCountDto> AllSpecies { get; set; } = new(); // <--- CHANGED TYPE

        // --- Sorting Properties ---
        protected string CurrentSortColumn { get; set; } = "SpeciesName"; // Default sort column
        protected SortDirection SortDirection { get; set; } = SortDirection.Ascending;

        // --- Search Property ---
        protected string SearchTerm { get; set; } = string.Empty;

        // Pagination properties
        protected int SpeciesCurrentPage { get; set; } = 1;
        protected int SpeciesPageSize { get; set; } = 10;

        // Computed property for filtered and sorted species
        // This will now operate on SpeciesWithBreedCountDto
        protected IEnumerable<SpeciesWithBreedCountDto> FilteredAndSortedSpecies
        {
            get
            {
                // Filter out IsDeleted species first in the UI display.
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
                    "BreedCount" => SortDirection == SortDirection.Ascending ? query.OrderBy(s => s.BreedCount) : query.OrderByDescending(s => s.BreedCount), // <--- NEW SORT OPTION
                    _ => query.OrderBy(s => s.SpeciesName) // Default sort
                };

                return query.ToList();
            }
        }

        protected int SpeciesTotalPages => FilteredAndSortedSpecies.Any() ? (int)System.Math.Ceiling((double)FilteredAndSortedSpecies.Count() / SpeciesPageSize) : 1;
        protected IEnumerable<SpeciesWithBreedCountDto> PagedSpecies => FilteredAndSortedSpecies.Skip((SpeciesCurrentPage - 1) * SpeciesPageSize).Take(SpeciesPageSize);

        // Forms & UI state for species management
        // Keep SpeciesFormModel as Species, as it's used for actual CRUD operations
        protected Species SpeciesFormModel { get; set; } = new();
        protected bool IsSpeciesFormVisible { get; set; } = false;
        private bool IsEditing { get; set; } = false;

        protected bool IsSpeciesSectionVisible { get; set; } = false;

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
            SpeciesCurrentPage = System.Math.Clamp(SpeciesCurrentPage, 1, SpeciesTotalPages == 0 ? 1 : SpeciesTotalPages);
        }

        private async Task LoadData()
        {
            using (var scope = ScopeFactory.CreateScope())
            {
                var speciesService = scope.ServiceProvider.GetRequiredService<ISpeciesService>();
                // Call the new service method
                AllSpecies = await speciesService.GetAllSpeciesForAdminAsync(); // <--- CALL NEW SERVICE METHOD
            }
            StateHasChanged();
        }

        protected void ToggleSpeciesSection()
        {
            IsSpeciesSectionVisible = !IsSpeciesSectionVisible;
            StateHasChanged();
        }

        protected void ShowAddSpeciesForm()
        {
            SpeciesFormModel = new Species();
            IsEditing = false;
            IsSpeciesFormVisible = true;
            StateHasChanged();
        }

        // Change parameter type to SpeciesWithBreedCountDto for display, then fetch full Species for editing.
        protected async Task EditSpecies(SpeciesWithBreedCountDto speciesDto) // <--- CHANGED PARAMETER TYPE
        {
            using (var scope = ScopeFactory.CreateScope())
            {
                var speciesService = scope.ServiceProvider.GetRequiredService<ISpeciesService>();
                // Fetch the full Species object by ID for editing
                SpeciesFormModel = await speciesService.GetByIdAsync(speciesDto.Id) ?? new Species();
            }

            IsEditing = true;
            IsSpeciesFormVisible = true;
            StateHasChanged();
        }

        protected async Task HandleSpeciesFormSubmit()
        {
            // Basic client-side validation for SpeciesName if needed, otherwise rely on DataAnnotationsValidator
            if (string.IsNullOrWhiteSpace(SpeciesFormModel.SpeciesName))
            {
                // You might want to add a visible error message here
                return;
            }

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

        // SoftDelete will still use the original ID
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
    }
}