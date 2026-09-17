namespace RecipeExperimentLab.DTO.Recepie
{
    public class RecipeCreateDto
    {
        public string Name { get; set; }
        public int StyleId { get; set; }
        public int ScoreId { get; set; }
        public string Review { get; set; }
        public List<int> Ingredients { get; set; } = [];
    }
}
