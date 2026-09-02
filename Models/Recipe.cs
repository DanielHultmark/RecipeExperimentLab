namespace RecipeExperimentLab.Models
{
    public class Recipe
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int StyleId { get; set; }
        public Style Style { get; set; }
        public List<Ingredient> Ingridients { get; set; }
        public Ingredient ingredient { get; set; }
        public int ScoreId { get; set; }
        public Score Score { get; set; }
        public string Review { get; set; }
    }
}
