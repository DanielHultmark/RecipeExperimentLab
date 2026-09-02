using Microsoft.AspNetCore.Identity;

namespace RecipeExperimentLab.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public string Email { get; set; }
    }
}
