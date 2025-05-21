using Microsoft.AspNetCore.Identity;

namespace PetQuestV1.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public bool IsDeleted { get; set; } = false;
        //used for soft-deletion of a user
    }

}
