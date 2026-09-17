namespace RecipeExperimentLab.DTO.Recepie
{
    public class RecipeResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Style { get; set; }
        public int Score { get; set; }
        public string Review { get; set; }
        public List<string> Ingredients { get; set; } = [];
    }
}
