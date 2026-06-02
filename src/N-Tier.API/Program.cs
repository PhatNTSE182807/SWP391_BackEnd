using Elastic.Clients.Elasticsearch;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.Dashboard;
using N_Tier.API;
using N_Tier.API.Extensions;
using N_Tier.API.Filters;
using N_Tier.API.Middleware;
using N_Tier.Application;
using N_Tier.Application.Models.Validators;
using N_Tier.DataAccess;
using N_Tier.DataAccess.Persistence;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

//Elasticsearch
var esUri = builder.Configuration["Elasticsearch:Uri"]
    ?? throw new InvalidOperationException("Elasticsearch:Uri is not configured");

var esUsername = builder.Configuration["Elasticsearch:Username"];
var esPassword = builder.Configuration["Elasticsearch:Password"];

ElasticsearchClientSettings esSettings;
if (!string.IsNullOrEmpty(esUsername) && !string.IsNullOrEmpty(esPassword))
{
    esSettings = new ElasticsearchClientSettings(new Uri(esUri))
        .Authentication(new Elastic.Transport.BasicAuthentication(esUsername, esPassword));
}
else
{
    esSettings = new ElasticsearchClientSettings(new Uri(esUri));
}

builder.Services.AddSingleton(new ElasticsearchClient(esSettings));

//Redis
var redisConnection = builder.Configuration["Redis:ConnectionString"]
    ?? throw new InvalidOperationException("Redis:ConnectionString is not configured");

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(redisConnection)
);

// Register IDistributedCache backed by Redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnection;
    options.InstanceName = "N-Tier:";
});

//Hangfire
builder.Services.AddHangfire(config =>
    config
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(builder.Configuration["Database:ConnectionString"])
);

builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = Environment.ProcessorCount * 2;
    options.SchedulePollingInterval = TimeSpan.FromSeconds(15);
});

builder.Services.AddControllers(
    config => config.Filters.Add(typeof(ValidateModelAttribute))
);

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining(typeof(IValidationsMarker));

builder.Services.AddSwagger();
builder.Services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds(type =>
    {
        string name = type.Name;
        if (name.EndsWith("LoginRequestModel"))
        {
            return name.Replace("LoginRequestModel", "UserLogin"); 
        }
        if (name.EndsWith("RegisterRequestModel"))
        {
            return name.Replace("RegisterRequestModel", "UserRegistration"); 
        }
        if (name.EndsWith("Enum"))
        {
            return name.Replace("Enum", ""); 
        }

        return name; 
    });
});
builder.Services.AddHttpContextAccessor();

builder.Services.AddDataAccess(builder.Configuration)
    .AddApplication(builder.Environment);

builder.Services.AddJwt(builder.Configuration);

builder.Services.AddEmailConfiguration(builder.Configuration);

var app = builder.Build();

using var scope = app.Services.CreateScope();

await AutomatedMigration.MigrateAsync(scope.ServiceProvider);

// Ensure Hangfire schema exists
app.Services.EnsureHangfireSchemaExists(builder.Configuration);

app.UseSwagger();
app.UseSwaggerUI(c => 
{ 
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Scientific Journal Publication Trend Tracking System"); 
    c.DocumentTitle = "Scientific Journal Publication Trend Tracking System";
});

app.UseHttpsRedirection();

app.UseCors(corsPolicyBuilder =>
    corsPolicyBuilder.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader()
);

// Hangfire dashboard (monitor background jobs)
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseMiddleware<PerformanceMiddleware>();

app.UseMiddleware<TransactionMiddleware>();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

// Schedule Hangfire recurring jobs after app is fully configured
try
{
    using (var jobScope = app.Services.CreateScope())
    {
        var hangfireJobService = jobScope.ServiceProvider.GetRequiredService<N_Tier.Application.Services.IHangfireJobService>();
        hangfireJobService.ScheduleRecurringJobs();
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogWarning(ex, "Failed to schedule Hangfire recurring jobs on startup. Jobs can be scheduled manually via API.");
}

app.Run();

namespace N_Tier.API
{
    public partial class Program { }
}
