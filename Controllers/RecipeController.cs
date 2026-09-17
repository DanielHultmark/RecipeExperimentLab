using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeExperimentLab.Data;
using RecipeExperimentLab.DTO.Recepie;
using RecipeExperimentLab.Models;

namespace RecipeExperimentLab.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecipeController : Controller
    {
        private readonly RecipeExperimentalLabDbContext _context;

        public RecipeController(RecipeExperimentalLabDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RecipeResponseDto>>> GetAll()
        {
            var recipes = await _context.Recipes
                .Select(r => new RecipeResponseDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Style = r.Style.Name,
                    Score = r.Score.Value,
                    Review = r.Review,
                    Ingredients = r.RecipeIngredients.Select(ri => ri.Ingredient.Name).ToList()
                })
                .ToListAsync();
            return Ok(recipes);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<RecipeResponseDto>> GetRecipe(int id)
        {
            var recipe = await GetRecipeDto
        }

        [HttpPost]
        public async Task<ActionResult<RecipeResponseDto>> CreateRecipe(RecipeRequestDto recipeDto)
        {
            if (string.IsNullOrWhiteSpace(recipeDto.Name))
            {
                return BadRequest("Recipe name is required.");
            }

            var styleExists = await _context.Styles.AnyAsync(s => s.Id == recipeDto.StyleId);
            if (!styleExists)
            {
                return BadRequest("Invalid style ID.");
            }

            var scoreExists = await _context.Scores.AnyAsync(s => s.Id == recipeDto.ScoreId);
            if (!scoreExists)
            {
                return BadRequest("Invalid score ID.");
            }

            var foundIngredientsIds = await _context.Ingredients
                .Where(i => recipeDto.IngredientIds.Contains(i.Id))
                .Select(i => i.Id)
                .ToListAsync();

            if (foundIngredientsIds.Count != recipeDto.IngredientIds.Count)
            {
                return BadRequest("One or more ingredient IDs are invalid.");
            }

            var recipe = new Recipe
            {
                Name = recipeDto.Name,
                StyleId = recipeDto.StyleId,
                ScoreId = recipeDto.ScoreId,
                Review = recipeDto.Review,
                RecipeIngredients = foundIngredientsIds.Select(id => new RecipeIngredient { IngredientId = id }).ToList()
            };
            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetRecipe), new { id = recipe.Id }, recipe);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateRecipe(int id, RecipeRequestDto recipeDto)
        {
            var recipe = await _context.Recipes
                .Include(r => r.RecipeIngredients)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (recipe == null)
            {
                return NotFound();
            }
            if (string.IsNullOrWhiteSpace(recipeDto.Name))
            {
                return BadRequest("Recipe name is required.");
            }
            var styleExists = await _context.Styles.AnyAsync(s => s.Id == recipeDto.StyleId);
            if (!styleExists)
            {
                return BadRequest("Invalid style ID.");
            }
            var scoreExists = await _context.Scores.AnyAsync(s => s.Id == recipeDto.ScoreId);
            if (!scoreExists)
            {
                return BadRequest("Invalid score ID.");
            }
            var foundIngredientsIds = await _context.Ingredients
                .Where(i => recipeDto.IngredientIds.Contains(i.Id))
                .Select(i => i.Id)
                .ToListAsync();
            if (foundIngredientsIds.Count != recipeDto.IngredientIds.Count)
            {
                return BadRequest("One or more ingredient IDs are invalid.");
            }
            recipe.Name = recipeDto.Name;
            recipe.StyleId = recipeDto.StyleId;
            recipe.ScoreId = recipeDto.ScoreId;
            recipe.Review = recipeDto.Review;
            // Update ingredients
            _context.RecipeIngredients.RemoveRange(recipe.RecipeIngredients);
            recipe.RecipeIngredients = foundIngredientsIds.Select(id => new RecipeIngredient { IngredientId = id }).ToList();
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteRecipe(int id)
        {
            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe == null)
            {
                return NotFound();
            }
            _context.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private async Task<RecipeResponseDto?> GetRecipeDto(int id)
        {
            return await _context.Recipes
               .Where(recipe => recipe.Id == id)
               .Select(recipe => new RecipeResponseDto
               {
                   Id = recipe.Id,
                   Name = recipe.Name,
                   Style = recipe.Style.CookingStyle,
                   Score = recipe.Score.NumberScore,
                   Review = recipe.Review,
                   Ingredients = recipe.RecipeIngredients
                       .Select(recipeIngredient => recipeIngredient.Ingredient.Name)
                       .ToList()
               }).FirstOrDefaultAsync();
        }
    }
}
