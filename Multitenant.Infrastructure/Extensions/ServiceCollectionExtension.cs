using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Multitenant.Core.Settings;
using Multitenant.Infrastructure.Persistence;

namespace Multitenant.Infrastructure.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddAndMigrateTenantDatabases(this IServiceCollection services, IConfiguration config)
        {
            var options = services.GetOptions<TenantSettings>(nameof(TenantSettings));
            var defaultConnectionString = options.Defaults?.ConnectionString;
            string defaultDbProvider = options.Defaults?.DBProvider ?? string.Empty;
            
            if (defaultDbProvider.Equals("mssql", StringComparison.OrdinalIgnoreCase))
            {
                services.AddDbContext<ApplicationDbContext>(m => 
                    m.UseSqlServer(e => e.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));
            }

            if (defaultDbProvider.Equals("postgres", StringComparison.OrdinalIgnoreCase))
            {
                services.AddDbContext<ApplicationDbContext>(m =>
                    m.UseNpgsql(e => e.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));
            }


            var tenants = options.Tenants;
            foreach (var tenant in tenants)
            {
                string connectionString;
                if (string.IsNullOrEmpty(tenant.ConnectionString))
                {
                    connectionString = defaultConnectionString;
                }
                else
                {
                    connectionString = tenant.ConnectionString;
                }
                using var scope = services.BuildServiceProvider().CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Database.SetConnectionString(connectionString);
                
                if (dbContext.Database.GetMigrations().Count() > 0)
                {
                    dbContext.Database.Migrate();
                }
            }
            return services;
        }

        public static T GetOptions<T>(this IServiceCollection services, string sectionName) where T : new()
        {
            using var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
           
            var section = configuration.GetSection(sectionName);
            var options = new T();
            section.Bind(options);
            
            return options;
        }
    }
}
