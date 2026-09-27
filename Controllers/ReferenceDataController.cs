using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeExperimentLab.Data;
using RecipeExperimentLab.DTO.ReferenceData;
using RecipeExperimentLab.Models;

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

        [HttpPost("styles")]
        public async Task<IActionResult> CreateStyle(CreateReferenceDataDto request)
        {
            var name = request.Name.Trim();

            var style = await _context.Styles
                .FirstOrDefaultAsync(style => style.CookingStyle.ToLower() == name.ToLower());

            if (style is not null)
            {
                return Ok(new { style.Id, Name = style.CookingStyle });
            }

            style = new Style { CookingStyle = name };
            _context.Styles.Add(style);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetStyles), new { style.Id, Name = style.CookingStyle });
        }
    }
}
