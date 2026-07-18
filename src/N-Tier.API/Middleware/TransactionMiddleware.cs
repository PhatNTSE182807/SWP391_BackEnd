using Microsoft.EntityFrameworkCore;
using N_Tier.DataAccess.Persistence;

namespace N_Tier.API.Middleware;

public class TransactionMiddleware(RequestDelegate next, ILogger<TransactionMiddleware> logger)
{
    private readonly ILogger<TransactionMiddleware> _logger = logger;

    public async Task Invoke(HttpContext context, DatabaseContext databaseContext)
    {
        if (context.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        var strategy = databaseContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await databaseContext.Database.BeginTransactionAsync();

            try
            {
                await next(context);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }
}
