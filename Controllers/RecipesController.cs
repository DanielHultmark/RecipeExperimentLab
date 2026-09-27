using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeExperimentLab.Data;
using RecipeExperimentLab.DTO.Recepie;
using RecipeExperimentLab.Models;
using RecipeExperimentLab.Services;

namespace RecipeExperimentLab.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RecipesController : ControllerBase
{
    private readonly RecipeExperimentalLabDbContext _context;
    private readonly RecipeService _recipeService;
    public RecipesController(RecipeExperimentalLabDbContext context, RecipeService recipeService)
    {
        _context = context;
        _recipeService = recipeService;
    }

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
        var error = await _recipeService.ValidateRecipeAsync(recipeDto);
        if (error is not null) return BadRequest(error);

        var recipe = new Recipe
        {
            Name = recipeDto.Name.Trim(), StyleId = recipeDto.StyleId,
            ScoreId = recipeDto.ScoreId, Review = recipeDto.Review?.Trim(), UserId = userId,
            RecipeIngredients = await _recipeService.BuildRecipeIngredientsAsync(recipeDto.Ingredients)
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
        var error = await _recipeService.ValidateRecipeAsync(recipeDto);
        if (error is not null) return BadRequest(error);

        recipe.Name = recipeDto.Name.Trim();
        recipe.StyleId = recipeDto.StyleId;
        recipe.ScoreId = recipeDto.ScoreId;
        recipe.Review = recipeDto.Review?.Trim();
        _context.RecipeIngredients.RemoveRange(recipe.RecipeIngredients);
        recipe.RecipeIngredients = await _recipeService.BuildRecipeIngredientsAsync(recipeDto.Ingredients);
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
