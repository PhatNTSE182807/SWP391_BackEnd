using Hangfire.Dashboard;

namespace N_Tier.API.Filters;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        
        // Allow access only to authenticated users
        // In production, you should add more specific role checks
        return httpContext.User.Identity?.IsAuthenticated ?? false;
    }
}
