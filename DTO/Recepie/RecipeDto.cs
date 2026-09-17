using RecipeExperimentLab.Models;

namespace RecipeExperimentLab.DTO.Recepie
{
    public class RecipeDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Description { get; set; }
        public string HowTo { get; set; }
        public Score Score { get; set; }
    }
}
