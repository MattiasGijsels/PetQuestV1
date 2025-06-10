using Microsoft.AspNetCore.Identity;
using PetQuestV1.Contracts.Models;

namespace PetQuestV1.Data
{
    public class ApplicationUser : IdentityUser
    {
        public bool IsDeleted { get; set; } = false;

        public ICollection<Pet> Pets { get; set; } = new List<Pet>();
    }

}
