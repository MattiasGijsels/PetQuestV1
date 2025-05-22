using Microsoft.AspNetCore.Identity;
using PetQuestV1.Contracts.Models;

namespace PetQuestV1.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public bool IsDeleted { get; set; } = false;
        //used for soft-deletion of a user

        // Add this navigation property to represent the pets owned by this user
        // This is crucial for Entity Framework Core to understand the relationship
        public ICollection<Pet> Pets { get; set; } = new List<Pet>();
    }

}
