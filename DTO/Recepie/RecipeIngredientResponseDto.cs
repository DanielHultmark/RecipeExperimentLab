namespace RecipeExperimentLab.DTO.Recepie
{
    public class RecipeIngredientResponseDto
    {
        public int IngredientId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Unit { get; set; } = string.Empty;
    }
}
