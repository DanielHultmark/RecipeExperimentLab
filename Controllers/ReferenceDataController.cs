using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeExperimentLab.Data;

namespace RecipeExperimentLab.Controllers
{
    [ApiController]
    [Route("api/reference-data")]
    [Authorize]
    public class ReferenceDataController : ControllerBase
    {
        private readonly RecipeExperimentalLabDbContext _context;

        public ReferenceDataController(RecipeExperimentalLabDbContext context)
        {
            _context = context;
        }

        [HttpGet("ingredients")]
        public async Task<IActionResult> GetIngredients()
        {
            var ingredients = await _context.Ingredients
                .OrderBy(ingredient => ingredient.Name)
                .Select(ingredient => new
                {
                    ingredient.Id,
                    ingredient.Name
                })
                .ToListAsync();
            
            return Ok(ingredients);
        }

        [HttpGet("styles")]
        public async Task<IActionResult> GetStyles()
        {
            var styles = await _context.Styles
                .OrderBy(style => style.CookingStyle)
                .Select(style => new
                {
                    style.Id,
                    Name = style.CookingStyle
                })
                .ToListAsync();

            return Ok(styles);
        }

        [HttpGet("scores")]
        public async Task<IActionResult> GetScores()
        {
            var scores = await _context.Scores
                .OrderBy(score => score.NumberScore)
                .Select(score => new
                {
                    score.Id,
                    Name = score.NumberScore.ToString()
                })
                .ToListAsync();

            return Ok(scores);
        }
    }
}
