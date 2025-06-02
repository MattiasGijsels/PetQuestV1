// Pages/Ranking.razor.cs
using Microsoft.AspNetCore.Components;
using PetQuestV1.Contracts.Defines; // Keep this for IPetService
using PetQuestV1.Contracts.Models; // Keep this for Pet
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PetQuestV1.Components.Pages // Ensure this namespace matches your project structure
{
    public partial class Ranking : ComponentBase
    {
        // REMOVE THE [Inject] AND THE PROPERTY DECLARATION FROM HERE
        // The @inject in Ranking.razor handles this.
        // public IPetService PetService { get; set; } = default!; // DELETE OR COMMENT OUT THIS LINE

        // You still need to access PetService, but it's now implicitly created by the @inject directive.
        // If you need to refer to it in this code-behind, you can directly use 'PetService' as it's defined by the Razor file.


        public List<Pet>? RankedPets { get; set; }

        protected override async Task OnInitializedAsync()
        {
            // 'PetService' is available here because it's injected in Ranking.razor
            var allPets = await PetService.GetAllAsync(); //
            RankedPets = allPets.OrderByDescending(p => p.Advantage).ToList();
        }
    }
}