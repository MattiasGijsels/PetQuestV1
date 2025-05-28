using Microsoft.AspNetCore.Components;
using PetQuestV1.Contracts.Defines;
using PetQuestV1.Contracts.Models;
using PetQuestV1.Contracts.Enums;

namespace PetQuestV1.Components.Admin
{
    public partial class BreedAdminPanelBase : ComponentBase
    {
        [Inject]
        private IServiceScopeFactory ScopeFactory { get; set; } = default!;

        protected List<BreedWithSpeciesDto> AllBreeds { get; set; } = new();
        protected List<Species> AvailableSpecies { get; set; } = new(); // For species dropdown

        // --- Sorting Properties ---
        protected string CurrentSortColumn { get; set; } = "BreedName"; // Default sort column
        protected SortDirection SortDirection { get; set; } = SortDirection.Ascending;

        // --- Search Property ---
        protected string SearchTerm { get; set; } = string.Empty;

        // Pagination properties
        protected int BreedCurrentPage { get; set; } = 1;
        protected int BreedPageSize { get; set; } = 10;

        // Forms & UI state for breed management
        protected Breed BreedFormModel { get; set; } = new();
        protected bool IsBreedFormVisible { get; set; } = false;
        private bool IsEditing { get; set; } = false;
        protected bool IsBreedSectionVisible { get; set; } = false;
        protected int BreedTotalPages => FilteredAndSortedBreeds.Any() ? (int)System.Math.Ceiling((double)FilteredAndSortedBreeds.Count() / BreedPageSize) : 1;
        protected IEnumerable<BreedWithSpeciesDto> PagedBreeds => FilteredAndSortedBreeds.Skip((BreedCurrentPage - 1) * BreedPageSize).Take(BreedPageSize);

        protected IEnumerable<BreedWithSpeciesDto> FilteredAndSortedBreeds
        {
            get
            {
                var query = AllBreeds.Where(b => !b.IsDeleted).AsQueryable();

                // Apply Search Filter
                if (!string.IsNullOrWhiteSpace(SearchTerm))
                {
                    query = query.Where(b =>
                        b.BreedName.Contains(SearchTerm, System.StringComparison.OrdinalIgnoreCase) ||
                        b.SpeciesName.Contains(SearchTerm, System.StringComparison.OrdinalIgnoreCase)
                    );
                }

                // Apply Sorting
                query = CurrentSortColumn switch
                {
                    "BreedName" => SortDirection == SortDirection.Ascending ? query.OrderBy(b => b.BreedName) : query.OrderByDescending(b => b.BreedName),
                    "SpeciesName" => SortDirection == SortDirection.Ascending ? query.OrderBy(b => b.SpeciesName) : query.OrderByDescending(b => b.SpeciesName),
                    _ => query.OrderBy(b => b.BreedName) // Default sort
                };

                return query.ToList();
            }
        }

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
            BreedCurrentPage = System.Math.Clamp(BreedCurrentPage, 1, BreedTotalPages == 0 ? 1 : BreedTotalPages);
        }

        private async Task LoadData()
        {
            using (var scope = ScopeFactory.CreateScope())
            {
                var breedService = scope.ServiceProvider.GetRequiredService<IBreedService>();
                AllBreeds = await breedService.GetAllBreedsForAdminAsync();
                AvailableSpecies = await breedService.GetAllSpeciesAsync(); // Load species for dropdown
            }
            StateHasChanged();
        }

        protected void ToggleBreedSection()
        {
            IsBreedSectionVisible = !IsBreedSectionVisible;
            StateHasChanged();
        }

        protected void ShowAddBreedForm()
        {
            BreedFormModel = new Breed();
            IsEditing = false;
            IsBreedFormVisible = true;
            StateHasChanged();
        }

        protected async Task EditBreed(BreedWithSpeciesDto breedDto)
        {
            using (var scope = ScopeFactory.CreateScope())
            {
                var breedService = scope.ServiceProvider.GetRequiredService<IBreedService>();
                BreedFormModel = await breedService.GetByIdAsync(breedDto.Id) ?? new Breed();
            }

            IsEditing = true;
            IsBreedFormVisible = true;
            StateHasChanged();
        }

        protected async Task HandleBreedFormSubmit()
        {
            if (string.IsNullOrWhiteSpace(BreedFormModel.BreedName) || string.IsNullOrWhiteSpace(BreedFormModel.SpeciesId))
            {
                System.Diagnostics.Debug.WriteLine("Validation Error: Breed Name or SpeciesId is missing.");
                return;
            }

            try
            {
                using (var scope = ScopeFactory.CreateScope())
                {
                    var breedService = scope.ServiceProvider.GetRequiredService<IBreedService>();

                    if (IsEditing)
                    {
                        await breedService.UpdateAsync(BreedFormModel);
                    }
                    else
                    {
                        await breedService.AddAsync(BreedFormModel);
                    }
                }

                IsBreedFormVisible = false;
                BreedFormModel = new Breed(); // Reset form model for next use
                await LoadData(); // Reload all data after submit to reflect changes
                StateHasChanged();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"An error occurred while saving the breed: {ex.Message}");
            }
        }

        protected void CancelBreedForm()
        {
            IsBreedFormVisible = false;
            BreedFormModel = new Breed(); // Clears the form
            StateHasChanged();
        }

        protected async Task SoftDeleteBreed(string id)
        {
            using (var scope = ScopeFactory.CreateScope())
            {
                var breedService = scope.ServiceProvider.GetRequiredService<IBreedService>();
                await breedService.SoftDeleteAsync(id);
            }
            await LoadData(); // Reload all data to reflect the soft deletion
            StateHasChanged();
        }

        // ---------- Pagination Handlers ----------
        protected void ChangeBreedPage(int page)
        {
            BreedCurrentPage = System.Math.Clamp(page, 1, BreedTotalPages);
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
            BreedCurrentPage = 1; // Reset to first page on sort change
            StateHasChanged();
        }

        protected string GetSortIcon(string columnName)
        {
            if (CurrentSortColumn != columnName)
            {
                return "bi-arrows-alt"; // Neutral icon
            }
            return SortDirection == SortDirection.Ascending ? "bi-caret-up-fill" : "bi-caret-down-fill";
            //selects an icon based on the sorting direction
        }

        protected void OnSearchInput(ChangeEventArgs e)
        {
            SearchTerm = e.Value?.ToString() ?? string.Empty;
            BreedCurrentPage = 1; // Reset to first page on search
            StateHasChanged();
        }
    }
}