using System.ComponentModel.DataAnnotations;

namespace RecipeExperimentLab.DTO.Recepie
{
    public class RecipeIngredientRequestDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        [Required]
        [StringLength(20)]
        public string Unit { get; set; } = string.Empty;
    }
}
