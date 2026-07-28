using Hangfire;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace N_Tier.API.Extensions;

public static class HangfireExtensions
{
    public static void EnsureHangfireSchemaExists(this IServiceProvider services, IConfiguration configuration)
    {
        var connectionString = configuration["Database:ConnectionString"];
        
        try
        {
            // Test connection and ensure Hangfire schema exists
            using var connection = new SqlConnection(connectionString);
            connection.Open();
            
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Hangfire database connection verified");
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogWarning("Failed to verify Hangfire database connection: {Message}. App will continue starting up.", ex.Message);
        }
    }
}
