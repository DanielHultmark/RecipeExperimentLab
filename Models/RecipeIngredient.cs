namespace RecipeExperimentLab.Models
{
    public class RecipeIngredient
    {
        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; }
        public int IngredientId { get; set; }
        public Ingredient Ingredient { get; set; }
        public decimal Amount { get; set; }
        public string Unit { get; set; } = string.Empty;
        public int SortOrder { get; set; }
    }
}
