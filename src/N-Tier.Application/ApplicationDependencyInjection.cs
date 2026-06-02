using Mapster;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using N_Tier.Application.Common.Email;
using N_Tier.Application.Jobs;
using N_Tier.Application.MappingProfiles;
using N_Tier.Application.Models.Search;
using N_Tier.Application.Services;
using N_Tier.Application.Services.DevImpl;
using N_Tier.Application.Services.Impl;
using N_Tier.Shared.Services;
using N_Tier.Shared.Services.Impl;
using Nest;
using StackExchange.Redis;

namespace N_Tier.Application;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IWebHostEnvironment env, IConfiguration configuration)
    {
        services.AddServices(env);

        services.RegisterMapper();

        services.AddElasticsearch(configuration);
        
        services.AddRedisCache(configuration);

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
        
        // Search services
        services.AddScoped<IElasticsearchService, ElasticsearchService>();
        services.AddScoped<ICacheService, RedisCacheService>();
        services.AddScoped<ISearchService, SearchService>();
        
        // Background jobs
        services.AddScoped<IReindexJob, ReindexJob>();

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

    private static void AddElasticsearch(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection("Elasticsearch").Get<ElasticsearchSettings>();
        services.Configure<ElasticsearchSettings>(configuration.GetSection("Elasticsearch"));

        var connectionSettings = new ConnectionSettings(new Uri(settings.Uri))
            .DefaultIndex(settings.DefaultIndex)
            .DefaultMappingFor<PaperDocument>(m => m.IndexName(settings.DefaultIndex));

        var client = new ElasticClient(connectionSettings);
        services.AddSingleton<IElasticClient>(client);
    }

    private static void AddRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection("Redis").Get<RedisSettings>();
        services.Configure<RedisSettings>(configuration.GetSection("Redis"));

        var multiplexer = ConnectionMultiplexer.Connect(settings.Configuration);
        services.AddSingleton<IConnectionMultiplexer>(multiplexer);
    }
}
