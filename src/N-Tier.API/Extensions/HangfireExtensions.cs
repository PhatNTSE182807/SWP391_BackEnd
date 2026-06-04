using Hangfire;
using Microsoft.Data.SqlClient;

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
            
            // Hangfire will automatically create schema on first use
            // Just verify connection works
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Hangfire database connection verified");
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Failed to verify Hangfire database connection");
            throw;
        }
    }
}
