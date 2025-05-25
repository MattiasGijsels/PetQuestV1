// PetQuestV1.Contracts.DTOs/UserDetailDto.cs
namespace PetQuestV1.Contracts.DTOs
{
    public class UserDetailDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int PetCount { get; set; } // The number of pets associated with the user
        public bool IsDeleted { get; set; }
        public string SelectedRoleId { get; set; } = string.Empty;
        // Add any other properties from your ApplicationUser that
        // represent the *full* details of a user that you might
        // want to fetch or display, even if not directly editable
        // in the UserFormDto.
    }
}