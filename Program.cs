using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RecipeExperimentLab.Data;
using RecipeExperimentLab.Models;
using Scalar.AspNetCore;

namespace RecipeExperimentLab
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddDbContext<RecipeExperimentalLabDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddIdentityApiEndpoints<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
            })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<RecipeExperimentalLabDbContext>();

            if (builder.Environment.IsDevelopment())
            {
                builder.Services.ConfigureApplicationCookie(options =>
                {
                    options.Cookie.SameSite = SameSiteMode.None;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                });
            };

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("Frontend", policy =>
                {
                    policy.WithOrigins(builder.Configuration["Frontend_Domain"]) // lägg domänen i User Secrets
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
                });
            });

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                await IdentitySeeder.SeedAsync(
                    services,
                    app.Configuration);
            }

            app.UseCors("Frontend");

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
