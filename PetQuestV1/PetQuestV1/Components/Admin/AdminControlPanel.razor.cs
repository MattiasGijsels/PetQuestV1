using Microsoft.AspNetCore.Components;

namespace PetQuestV1.Components.Pages
{
    public partial class AdminControlPanelBase : ComponentBase
    {
        // This class now serves as a minimal container for the admin panel
        // No need for most of the previous code as it's moved to specific components

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            // Any shared initialization can go here
        }
    }
}