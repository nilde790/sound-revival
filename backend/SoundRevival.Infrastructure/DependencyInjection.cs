using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SoundRevival.Core.Interfaces;
using SoundRevival.Infrastructure.Repository;
using SoundRevival.Infrastructure.Services;
using SoundRevival.Infrastructure.Settings;

namespace SoundRevival.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Fail fast at startup, like ValidateOnStart for JwtSettings, instead of at the first query.
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

            services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IListingRepository, ListingRepository>();
            services.AddScoped<ITokenService, JwtTokenService>();

            // Stateless: one instance for the whole app.
            services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();

            services.AddOptions<JwtSettings>()
                .Bind(configuration.GetSection("Jwt"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            return services;
        }
    }
}
