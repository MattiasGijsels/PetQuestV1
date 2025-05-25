namespace PetQuestV1.Contracts.DTOs
{
    public class UserDetailDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int PetCount { get; set; } 
        public bool IsDeleted { get; set; }
        public string SelectedRoleId { get; set; } = string.Empty;
    }
}