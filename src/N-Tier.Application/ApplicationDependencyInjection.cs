using Mapster;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using N_Tier.Application.Common.Email;
using N_Tier.Application.MappingProfiles;
using N_Tier.Application.Services;
using N_Tier.Application.Services.DevImpl;
using N_Tier.Application.Services.Impl;
using N_Tier.Shared.Services;
using N_Tier.Shared.Services.Impl;

namespace N_Tier.Application;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IWebHostEnvironment env)
    {
        services.AddServices(env);

        services.RegisterMapper();

        return services;
    }

    private static void AddServices(this IServiceCollection services, IWebHostEnvironment env)
    {
        services.AddScoped<IClaimService, ClaimService>();
        services.AddScoped<ITemplateService, TemplateService>();
        services.AddScoped<IApiSourceService, ApiSourceService>();
        services.AddScoped<IJournalService, JournalService>();
        services.AddScoped<IPaperService, PaperService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IOpenAlexService, OpenAlexService>();
        services.AddScoped<ICrossrefService, CrossrefService>();
        services.AddScoped<ISemanticScholarService, SemanticScholarService>();

        if (env.IsDevelopment())
            services.AddScoped<IEmailService, DevEmailService>();
        else
            services.AddScoped<IEmailService, EmailService>();
    }

    private static void RegisterMapper(this IServiceCollection services)
    {
        TypeAdapterConfig.GlobalSettings.Scan(typeof(IMappingProfilesMarker).Assembly);
        services.AddMapster();
    }

    public static void AddEmailConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(configuration.GetSection("SmtpSettings").Get<SmtpSettings>());
    }
}
