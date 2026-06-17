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

// HttpClient for external API calls
builder.Services.AddHttpClient();

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
    c.EnablePersistAuthorization();
    c.InjectJavascript("/swagger-custom.js");
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

app.MapGet("/swagger-custom.js", async context =>
{
    context.Response.ContentType = "application/javascript";
    await context.Response.WriteAsync(@"
        (function () {
            const checkReady = setInterval(() => {
                const topbar = document.querySelector('.topbar-wrapper');
                if (topbar) {
                    clearInterval(checkReady);
                    
                    const btnContainer = document.createElement('div');
                    btnContainer.style.display = 'flex';
                    btnContainer.style.gap = '8px';
                    btnContainer.style.marginLeft = '20px';
                    btnContainer.style.alignItems = 'center';
                    
                    const currentRole = localStorage.getItem('swagger-ui:current-role');
                    const isAuthorized = localStorage.getItem('authorized');
                    
                    if (currentRole && isAuthorized) {
                        const statusText = document.createElement('span');
                        statusText.innerHTML = '👤 Role: <b>' + currentRole + '</b>';
                        statusText.style.color = '#fff';
                        statusText.style.fontSize = '13px';
                        statusText.style.marginRight = '10px';
                        
                        const logoutBtn = document.createElement('button');
                        logoutBtn.innerHTML = '❌ Logout';
                        logoutBtn.style.padding = '5px 12px';
                        logoutBtn.style.backgroundColor = '#f85a5a';
                        logoutBtn.style.color = 'white';
                        logoutBtn.style.border = 'none';
                        logoutBtn.style.borderRadius = '4px';
                        logoutBtn.style.cursor = 'pointer';
                        logoutBtn.style.fontWeight = 'bold';
                        logoutBtn.style.fontSize = '12px';
                        
                        logoutBtn.onclick = function () {
                            localStorage.removeItem('authorized');
                            localStorage.removeItem('swagger-ui:current-role');
                            window.location.reload();
                        };
                        
                        btnContainer.appendChild(statusText);
                        btnContainer.appendChild(logoutBtn);
                    } else {
                        if (!isAuthorized) {
                            localStorage.removeItem('swagger-ui:current-role');
                        }
                        
                        const roles = [
                            { name: 'Admin', email: 'admin@cloud.com', password: 'Admin123@', color: '#49cc90' },
                            { name: 'Researcher', email: 'researcher@cloud.com', password: 'Password123@', color: '#e0a800' },
                            { name: 'Lecturer', email: 'lecturer@cloud.com', password: 'Password123@', color: '#007bff' },
                            { name: 'Student', email: 'student@cloud.com', password: 'Password123@', color: '#17a2b8' }
                        ];
                        
                        roles.forEach(role => {
                            const btn = document.createElement('button');
                            btn.innerHTML = '🔑 ' + role.name;
                            btn.style.padding = '5px 12px';
                            btn.style.backgroundColor = role.color;
                            btn.style.color = 'white';
                            btn.style.border = 'none';
                            btn.style.borderRadius = '4px';
                            btn.style.cursor = 'pointer';
                            btn.style.fontWeight = 'bold';
                            btn.style.fontSize = '12px';
                            
                            btn.onclick = async function () {
                                btn.innerHTML = 'Logging in...';
                                try {
                                    const response = await fetch('/api/auth/login', {
                                        method: 'POST',
                                        headers: { 'Content-Type': 'application/json' },
                                        body: JSON.stringify({ email: role.email, password: role.password })
                                    });
                                    const result = await response.json();
                                    const succeeded = result.succeeded !== undefined ? result.succeeded : result.Succeeded;
                                    const resData = result.result !== undefined ? result.result : result.Result;
                                    const errors = result.errors !== undefined ? result.errors : result.Errors;
                                    
                                    if (succeeded && resData && resData.token) {
                                        const token = resData.token;
                                         const authObj = {
                                             'Bearer': {
                                                 'schema': {
                                                     'type': 'apiKey',
                                                     'name': 'Authorization',
                                                     'in': 'header'
                                                 },
                                                 'value': 'Bearer ' + token
                                             }
                                         };
                                        localStorage.setItem('authorized', JSON.stringify(authObj));
                                        localStorage.setItem('swagger-ui:current-role', role.name);
                                        btn.innerHTML = '✅ OK!';
                                        btn.style.backgroundColor = '#28a745';
                                        setTimeout(() => {
                                            window.location.reload();
                                        }, 500);
                                    } else {
                                        alert('Login failed for ' + role.name + ': ' + JSON.stringify(errors || []));
                                        btn.innerHTML = '🔑 ' + role.name;
                                    }
                                } catch (err) {
                                    alert('Login error: ' + err.message);
                                    btn.innerHTML = '🔑 ' + role.name;
                                }
                            };
                            
                            btnContainer.appendChild(btn);
                        });
                    }
                    
                    topbar.appendChild(btnContainer);
                }
            }, 100);
        })();
    ");
});

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
