using System.ComponentModel.DataAnnotations;

namespace PetQuestV1.Contracts.DTOs
{
    public class UserFormDto
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "User Name is required.")]
        [StringLength(256, ErrorMessage = "User Name cannot exceed 256 characters.")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        [StringLength(256, ErrorMessage = "Email cannot exceed 256 characters.")]
        public string Email { get; set; } = string.Empty;
        public int PetCount { get; set; }
        public bool IsDeleted { get; set; }

        [Required(ErrorMessage = "Role is required.")]
        public string SelectedRoleId { get; set; } = string.Empty;
    }
}