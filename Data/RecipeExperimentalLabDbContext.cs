using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RecipeExperimentLab.Models;

namespace RecipeExperimentLab.Data
{
    public class RecipeExperimentalLabDbContext : IdentityDbContext<ApplicationUser>
    {
        public RecipeExperimentalLabDbContext(DbContextOptions<RecipeExperimentalLabDbContext> options)
            : base(options)
        {
        }
        public DbSet<Score> Scores { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<Style> Styles { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<RecipeIngredient> RecipeIngredients { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Score>()
                .HasData(
                    new Score { Id = 1, NumberScore = 1 },
                    new Score { Id = 2, NumberScore = 2 },
                    new Score { Id = 3, NumberScore = 3 },
                    new Score { Id = 4, NumberScore = 4 },
                    new Score { Id = 5, NumberScore = 5 }
                );
        }
    }
}
