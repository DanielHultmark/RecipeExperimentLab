namespace RecipeExperimentLab.Models
{
    public class Recipe
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int StyleId { get; set; }
        public Style Style { get; set; }
        public int ScoreId { get; set; }
        public Score Score { get; set; }
        public string Review { get; set; }

        public List<RecipeIngredient> RecipeIngredients { get; set; } = [];

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
    }
}
