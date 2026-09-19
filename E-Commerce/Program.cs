using E_Commerce.Data;
using E_Commerce.Mapping;
using E_Commerce.Models;
using E_Commerce.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // =====================================================
            // Email Settings
            // =====================================================

            builder.Services.Configure<EmailSettings>(
                builder.Configuration.GetSection("EmailSettings"));

            builder.Services.AddScoped<IEmailSender, EmailSender>();


            // =====================================================
            // AI Chatbot
            // =====================================================

            builder.Services.AddScoped<AIChatService>();


            // =====================================================
            // Add MVC
            // =====================================================

            builder.Services.AddControllersWithViews();


            // =====================================================
            // AutoMapper
            // =====================================================

            builder.Services.AddAutoMapper(
                cfg => { },
                typeof(MappingProfile).Assembly
            );


            // =====================================================
            // Entity Framework Core
            // =====================================================

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString(
                        "DefaultConnection"
                    )
                )
            );


            // =====================================================
            // ASP.NET Core Identity
            // =====================================================

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(
                options =>
                {
                    // Password settings
                    options.Password.RequiredLength = 6;
                    options.Password.RequireDigit = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireNonAlphanumeric = false;

                    // User settings
                    options.User.RequireUniqueEmail = true;
                }
            )
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();


            // =====================================================
            // Authentication Cookie
            // =====================================================

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/User/Login";
                options.AccessDeniedPath = "/User/AccessDenied";
            });


            // =====================================================
            // Stripe
            // =====================================================

            Stripe.StripeConfiguration.ApiKey =
                builder.Configuration["Stripe:SecretKey"];


            // =====================================================
            // Build Application
            // =====================================================

            var app = builder.Build();


            // =====================================================
            // Seed Roles
            // =====================================================

            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider
                    .GetRequiredService<RoleManager<IdentityRole>>();

                await DbInitializer.SeedRolesAsync(roleManager);
            }


            // =====================================================
            // Configure HTTP Request Pipeline
            // =====================================================

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            // Enable wwwroot files and images
            app.UseStaticFiles();

            app.UseRouting();

            // Authentication before Authorization
            app.UseAuthentication();
            app.UseAuthorization();


            // =====================================================
            // Default Route
            // =====================================================

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}"
            );


            app.Run();
        }
    }
}