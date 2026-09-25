using System.ComponentModel.DataAnnotations;

namespace RecipeExperimentLab.DTO.User
{
    public class UpdateUserRoleDto
    {
        [Required]
        [RegularExpression("^(Admin|User)$")]
        public string Role { get; set; } = string.Empty;
    }
}
