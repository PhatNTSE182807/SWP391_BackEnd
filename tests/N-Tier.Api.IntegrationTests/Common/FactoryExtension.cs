using Microsoft.Extensions.DependencyInjection;
using N_Tier.API;

namespace N_Tier.Api.IntegrationTests.Common;

public static class FactoryExtension
{
    public static T GetRequiredService<T>(this ApiApplicationFactory<Program> factory) where T : notnull
    {
        var scope = factory.Services.CreateScope();
        
        return (T)scope.ServiceProvider.GetRequiredService(typeof(T));
    }
}
