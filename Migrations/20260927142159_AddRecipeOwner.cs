using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecipeExperimentLab.Migrations
{
    /// <inheritdoc />
    public partial class AddRecipeOwner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Recipes_Styles_StyleId",
                table: "Recipes");

            migrationBuilder.AddForeignKey(
                name: "FK_Recipes_Styles_StyleId",
                table: "Recipes",
                column: "StyleId",
                principalTable: "Styles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Recipes_Styles_StyleId",
                table: "Recipes");

            migrationBuilder.AddForeignKey(
                name: "FK_Recipes_Styles_StyleId",
                table: "Recipes",
                column: "StyleId",
                principalTable: "Styles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
