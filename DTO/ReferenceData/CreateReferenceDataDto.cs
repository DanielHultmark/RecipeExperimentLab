using System.ComponentModel.DataAnnotations;

namespace RecipeExperimentLab.DTO.ReferenceData
{
    public class CreateReferenceDataDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}
