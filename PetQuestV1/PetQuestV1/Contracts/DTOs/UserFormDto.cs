// PetQuestV1/Contracts/DTOs/UserFormDto.cs
using System.ComponentModel.DataAnnotations;

namespace PetQuestV1.Contracts.DTOs
{
    public class UserFormDto
    {
        public string? Id { get; set; } // Nullable for new users

        [Required(ErrorMessage = "User Name is required.")]
        [StringLength(256, ErrorMessage = "User Name cannot exceed 256 characters.")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        [StringLength(256, ErrorMessage = "Email cannot exceed 256 characters.")]
        public string Email { get; set; } = string.Empty;

        // PetCount is usually derived, not directly editable, but kept for consistency with existing form
        public int PetCount { get; set; }

        public bool IsDeleted { get; set; }

        // Add this property to hold the selected role ID
        [Required(ErrorMessage = "Role is required.")]
        public string SelectedRoleId { get; set; } = string.Empty;
    }
}