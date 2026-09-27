using System.ComponentModel.DataAnnotations;

namespace RecipeExperimentLab.DTO.Recepie
{
    public class RecipeIngredientRequestDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.01", "999999")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(20)]
        public string Unit { get; set; } = string.Empty;
    }
}
