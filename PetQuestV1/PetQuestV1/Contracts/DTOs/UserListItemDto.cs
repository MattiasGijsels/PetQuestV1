namespace PetQuestV1.Contracts.DTOs
{
    public class UserListItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
        public int PetCount { get; set; } 
        public string RoleName { get; set; } = string.Empty; 
    }
}