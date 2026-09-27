using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeExperimentLab.Data;
using RecipeExperimentLab.DTO.Recepie;
using RecipeExperimentLab.Models;

namespace RecipeExperimentLab.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RecipesController : ControllerBase
{
    private readonly RecipeExperimentalLabDbContext _context;
    public RecipesController(RecipeExperimentalLabDbContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RecipeResponseDto>>> GetAll() =>
        Ok(await GetAccessibleRecipes().Select(ToResponse()).ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RecipeResponseDto>> GetRecipe(int id)
    {
        var recipe = await GetAccessibleRecipes().Where(recipe => recipe.Id == id)
            .Select(ToResponse()).FirstOrDefaultAsync();
        return recipe is null ? NotFound() : Ok(recipe);
    }

    [HttpPost]
    public async Task<ActionResult<RecipeResponseDto>> CreateRecipe(RecipeRequestDto recipeDto)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();
        var error = await ValidateRecipeAsync(recipeDto);
        if (error is not null) return BadRequest(error);

        var recipe = new Recipe
        {
            Name = recipeDto.Name.Trim(), StyleId = recipeDto.StyleId,
            ScoreId = recipeDto.ScoreId, Review = recipeDto.Review?.Trim(), UserId = userId,
            RecipeIngredients = await BuildRecipeIngredientsAsync(recipeDto.Ingredients)
        };
        _context.Recipes.Add(recipe);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetRecipe), new { id = recipe.Id }, await GetRecipeDto(recipe.Id));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateRecipe(int id, RecipeRequestDto recipeDto)
    {
        var recipe = await _context.Recipes.Include(recipe => recipe.RecipeIngredients)
            .FirstOrDefaultAsync(recipe => recipe.Id == id);
        if (recipe is null) return NotFound();
        if (!CanManageRecipe(recipe)) return Forbid();
        var error = await ValidateRecipeAsync(recipeDto);
        if (error is not null) return BadRequest(error);

        recipe.Name = recipeDto.Name.Trim();
        recipe.StyleId = recipeDto.StyleId;
        recipe.ScoreId = recipeDto.ScoreId;
        recipe.Review = recipeDto.Review?.Trim();
        _context.RecipeIngredients.RemoveRange(recipe.RecipeIngredients);
        recipe.RecipeIngredients = await BuildRecipeIngredientsAsync(recipeDto.Ingredients);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteRecipe(int id)
    {
        var recipe = await _context.Recipes.FindAsync(id);
        if (recipe is null) return NotFound();
        if (!CanManageRecipe(recipe)) return Forbid();
        _context.Recipes.Remove(recipe);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private async Task<string?> ValidateRecipeAsync(RecipeRequestDto recipeDto)
    {
        if (string.IsNullOrWhiteSpace(recipeDto.Name)) return "Recipe name is required.";
        if (recipeDto.Ingredients.Count == 0) return "At least one ingredient is required.";
        if (recipeDto.Ingredients.Any(item => string.IsNullOrWhiteSpace(item.Name) || string.IsNullOrWhiteSpace(item.Unit) || item.Amount <= 0))
            return "Every ingredient requires a name, amount and unit.";
        if (recipeDto.Ingredients.Select(item => item.Name.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count() != recipeDto.Ingredients.Count)
            return "The same ingredient can only be added once.";
        if (!await _context.Styles.AnyAsync(style => style.Id == recipeDto.StyleId)) return "Invalid style ID.";
        if (!await _context.Scores.AnyAsync(score => score.Id == recipeDto.ScoreId)) return "Invalid score ID.";
        return null;
    }

    private async Task<List<RecipeIngredient>> BuildRecipeIngredientsAsync(IEnumerable<RecipeIngredientRequestDto> ingredients)
    {
        var result = new List<RecipeIngredient>();
        var sortOrder = 0;
        foreach (var request in ingredients)
        {
            var name = request.Name.Trim();
            var ingredient = await _context.Ingredients.FirstOrDefaultAsync(item => item.Name.ToLower() == name.ToLower());
            if (ingredient is null)
            {
                ingredient = new Ingredient { Name = name };
                _context.Ingredients.Add(ingredient);
            }
            result.Add(new RecipeIngredient { Ingredient = ingredient, Amount = request.Amount, Unit = request.Unit.Trim(), SortOrder = sortOrder++ });
        }
        return result;
    }

    private string? GetCurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);
    private bool CanManageRecipe(Recipe recipe) => User.IsInRole("Admin") || recipe.UserId == GetCurrentUserId();
    private IQueryable<Recipe> GetAccessibleRecipes() => User.IsInRole("Admin")
        ? _context.Recipes.AsNoTracking()
        : _context.Recipes.AsNoTracking().Where(recipe => recipe.UserId == GetCurrentUserId());

    private static System.Linq.Expressions.Expression<Func<Recipe, RecipeResponseDto>> ToResponse() => recipe => new RecipeResponseDto
    {
        Id = recipe.Id, Name = recipe.Name, StyleId = recipe.StyleId, Style = recipe.Style.CookingStyle,
        ScoreId = recipe.ScoreId, Score = recipe.Score.NumberScore, Review = recipe.Review,
        Ingredients = recipe.RecipeIngredients.OrderBy(item => item.SortOrder).Select(item => item.Ingredient.Name).ToList(),
        RecipeIngredients = recipe.RecipeIngredients.OrderBy(item => item.SortOrder).Select(item => new RecipeIngredientResponseDto
        {
            IngredientId = item.IngredientId, Name = item.Ingredient.Name, Amount = item.Amount, Unit = item.Unit
        }).ToList()
    };

    private async Task<RecipeResponseDto?> GetRecipeDto(int id) => await GetAccessibleRecipes()
        .Where(recipe => recipe.Id == id).Select(ToResponse()).FirstOrDefaultAsync();
}
