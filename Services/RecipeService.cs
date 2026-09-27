using Microsoft.EntityFrameworkCore;
using RecipeExperimentLab.Data;
using RecipeExperimentLab.DTO.Recepie;
using RecipeExperimentLab.Models;

namespace RecipeExperimentLab.Services;

public class RecipeService
{
    private readonly RecipeExperimentalLabDbContext _context;

    public RecipeService(RecipeExperimentalLabDbContext context) => _context = context;

    public async Task<string?> ValidateRecipeAsync(RecipeRequestDto recipeDto)
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

    public async Task<List<RecipeIngredient>> BuildRecipeIngredientsAsync(IEnumerable<RecipeIngredientRequestDto> ingredients)
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
}
