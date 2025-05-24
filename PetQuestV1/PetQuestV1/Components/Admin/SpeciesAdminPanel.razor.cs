using Microsoft.AspNetCore.Components;
using PetQuestV1.Contracts.Models;
using PetQuestV1.Contracts.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PetQuestV1.Components.Admin
{
    public class SpeciesAdminPanelBase : ComponentBase
    {
        [Inject]
        public ISpeciesService SpeciesService { get; set; } = default!;

        public bool IsSpeciesSectionVisible { get; set; } = false;
        public bool IsSpeciesFormVisible { get; set; }
        public Species SpeciesFormModel { get; set; } = new Species();

        public List<Species> AllSpecies { get; set; } = new List<Species>();
        public List<Species> FilteredAndSortedSpecies { get; set; } = new List<Species>();
        public List<Species> PagedSpecies { get; set; } = new List<Species>();

        // Pagination
        public int SpeciesCurrentPage { get; set; } = 1;
        public int SpeciesPageSize { get; set; } = 10;
        public int SpeciesTotalPages => (int)Math.Ceiling((double)FilteredAndSortedSpecies.Count / SpeciesPageSize);

        // Sorting
        public string CurrentSortColumn { get; set; } = "SpeciesName";
        public bool IsSortAscending { get; set; } = true;

        // Search
        public string SearchTerm { get; set; } = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            await LoadSpecies();
        }

        private async Task LoadSpecies()
        {
            AllSpecies = (await SpeciesService.GetAllSpeciesAsync()).Where(s => !s.IsDeleted).ToList();
            ApplyFilterAndSort();
        }

        private void ApplyFilterAndSort()
        {
            // Filter
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                FilteredAndSortedSpecies = AllSpecies
                    .Where(s => s.SpeciesName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            else
            {
                FilteredAndSortedSpecies = AllSpecies;
            }

            // Sort
            if (CurrentSortColumn == "SpeciesName")
            {
                FilteredAndSortedSpecies = IsSortAscending ?
                    FilteredAndSortedSpecies.OrderBy(s => s.SpeciesName).ToList() :
                    FilteredAndSortedSpecies.OrderByDescending(s => s.SpeciesName).ToList();
            }
            // Add other sorting options here if needed

            ApplyPagination();
        }

        private void ApplyPagination()
        {
            PagedSpecies = FilteredAndSortedSpecies
                .Skip((SpeciesCurrentPage - 1) * SpeciesPageSize)
                .Take(SpeciesPageSize)
                .ToList();
        }

        protected void ToggleSpeciesSection()
        {
            IsSpeciesSectionVisible = !IsSpeciesSectionVisible;
            if (IsSpeciesSectionVisible)
            {
                // Optionally re-load species if needed when section is shown
                _ = LoadSpecies();
            }
            IsSpeciesFormVisible = false; // Hide form when collapsing section
        }

        protected void ShowAddSpeciesForm()
        {
            SpeciesFormModel = new Species(); // Reset form for new species
            IsSpeciesFormVisible = true;
        }

        protected void EditSpecies(Species species)
        {
            SpeciesFormModel = species; // Set form model to the selected species for editing
            IsSpeciesFormVisible = true;
        }

        protected async Task HandleSpeciesFormSubmit()
        {
            if (string.IsNullOrEmpty(SpeciesFormModel.Id) || SpeciesFormModel.Id == Guid.Empty.ToString("N"))
            {
                // Add new species
                await SpeciesService.AddSpeciesAsync(SpeciesFormModel);
            }
            else
            {
                // Update existing species
                await SpeciesService.UpdateSpeciesAsync(SpeciesFormModel);
            }
            IsSpeciesFormVisible = false;
            await LoadSpecies(); // Refresh list
            StateHasChanged();
        }

        protected void CancelSpeciesForm()
        {
            IsSpeciesFormVisible = false;
            SpeciesFormModel = new Species(); // Clear the form model
        }

        protected async Task SoftDeleteSpecies(string speciesId)
        {
            await SpeciesService.SoftDeleteSpeciesAsync(speciesId);
            await LoadSpecies(); // Refresh list
            StateHasChanged();
        }

        protected void OnSearchInput(ChangeEventArgs e)
        {
            SearchTerm = e.Value?.ToString() ?? string.Empty;
            SpeciesCurrentPage = 1; // Reset to first page on search
            ApplyFilterAndSort();
        }

        protected void SortBy(string columnName)
        {
            if (CurrentSortColumn == columnName)
            {
                IsSortAscending = !IsSortAscending;
            }
            else
            {
                CurrentSortColumn = columnName;
                IsSortAscending = true;
            }
            ApplyFilterAndSort();
        }

        protected string GetSortIcon(string columnName)
        {
            if (CurrentSortColumn != columnName)
            {
                return "bi-sort"; // Default sort icon
            }
            return IsSortAscending ? "bi-sort-alpha-down" : "bi-sort-alpha-up";
        }

        protected void ChangeSpeciesPage(int page)
        {
            if (page >= 1 && page <= SpeciesTotalPages)
            {
                SpeciesCurrentPage = page;
                ApplyPagination();
            }
        }
    }
}