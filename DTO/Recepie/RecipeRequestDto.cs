using System.ComponentModel.DataAnnotations;

namespace RecipeExperimentLab.DTO.Recepie
{
    public class RecipeRequestDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int StyleId { get; set; }

        [Range(1, 5)]
        public int ScoreId { get; set; }

        [StringLength(200)]
        public string? Review { get; set; }

        [Required]
        [MinLength(1)]
        public List<RecipeIngredientRequestDto> Ingredients { get; set; } = [];
    }
}
