// Example: PetQuestV1.Contracts.DTOs/UserFormDto.cs
using System.ComponentModel.DataAnnotations;

namespace PetQuestV1.Contracts.DTOs
{
    public class UserFormDto
    {
        public string? Id { get; set; } // Will be null for new users, populated for existing ones

        [Required(ErrorMessage = "User Name is required.")]
        [StringLength(50, ErrorMessage = "User Name cannot exceed 50 characters.")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        public string Email { get; set; } = string.Empty;

        // Assuming you might want to edit this, otherwise remove
        public int PetCount { get; set; }

        public bool IsDeleted { get; set; }

        // Add any other properties you want to edit on the user
    }
}