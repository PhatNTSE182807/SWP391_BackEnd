using Hangfire.Dashboard;

namespace N_Tier.API.Filters
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            // TODO: Implement proper authorization
            // For now, allow access in development only
            // In production, you should check authentication and role
            return true; // Allow all for development
        }
    }
}
