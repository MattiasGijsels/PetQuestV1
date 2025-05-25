using Microsoft.AspNetCore.Components;

namespace PetQuestV1.Components.Pages
{
    public partial class AdminControlPanelBase : ComponentBase
    {
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            // Any shared initialization can go here, maybe something for future use?
        }
    }
}