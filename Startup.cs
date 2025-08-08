using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Void.Database;
using Void.Repositories;
using Void.Services;

namespace Void
{
    public class Startup
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            ConfigureServices(builder.Services);

            var app = builder.Build();

            // Middleware order matters!
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                // Disable HTTPS redirection in development for easier testing
                // app.UseHttpsRedirection();
            }
            else
            {
                app.UseHttpsRedirection();
            }

            app.UseRouting();

            // CORS must come before Authentication/Authorization
            app.UseCors("AllowReact");

            // Cookie policy for cross-origin requests
            app.UseCookiePolicy(new CookiePolicyOptions
            {
                MinimumSameSitePolicy = SameSiteMode.None,
                Secure = CookieSecurePolicy.Always
            });

            // Authentication before Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }

        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            services.AddScoped<UserService>();
            services.AddDbContext<DatabaseContext>(options =>
                options.UseSqlite("Data Source=Void.db"));

            // CORS configuration
            services.AddCors(options =>
            {
                options.AddPolicy("AllowReact",
                    builder =>
                    {
                        builder.WithOrigins("http://localhost:5173") // Your React app's URL
                               .AllowAnyHeader()
                               .AllowAnyMethod()
                               .AllowCredentials(); // Important for cookies/auth
                    });
            });

            // Cookie authentication
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie(options =>
                    {
                        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                        options.SlidingExpiration = true;
                        options.Cookie.SameSite = SameSiteMode.None;
                        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                        options.Cookie.HttpOnly = true;
                    });

            services.AddAuthorization();
            services.AddScoped<AuthenticationService>();
            services.AddScoped<UserRepository>();

            // Cookie policy
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
        }
    }
}