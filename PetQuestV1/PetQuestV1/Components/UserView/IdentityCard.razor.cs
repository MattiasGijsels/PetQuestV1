using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using PetQuestV1.Contracts.Models;
using System.Threading.Tasks;
using System; 

namespace PetQuestV1.Components.UserView
{
    public partial class IdentityCard : ComponentBase
    {
        [Parameter]
        public Pet Pet { get; set; } = default!;

        [Parameter]
        public EventCallback<(string PetId, IBrowserFile ImageFile)> OnImageUploadRequested { get; set; }

        [Parameter]
        public EventCallback<string> OnImageDeleteRequested { get; set; }

        private IBrowserFile? _selectedFileForUpload; // Holds the file temporarily in this component

        protected async Task OnInputFileChange(InputFileChangeEventArgs e)
        {
            _selectedFileForUpload = e.File;
            if (_selectedFileForUpload != null && !string.IsNullOrEmpty(Pet.Id))
            {
                await OnImageUploadRequested.InvokeAsync((Pet.Id, _selectedFileForUpload));
                _selectedFileForUpload = null; // Clear after sending to parent
            }
            else
            {
                Console.WriteLine($"No file selected or pet ID is missing for pet: {Pet?.PetName}");
            }
        }

        protected async Task DeleteImage()
        {
            if (!string.IsNullOrEmpty(Pet.Id))
            {
                await OnImageDeleteRequested.InvokeAsync(Pet.Id);
            }
            else
            {
                Console.WriteLine($"Cannot delete image: Pet ID is missing for pet: {Pet?.PetName}");
            }
        }
    }
}