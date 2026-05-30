using FluentValidation;
using FluentValidation.AspNetCore;
using N_Tier.API;
using N_Tier.API.Filters;
using N_Tier.API.Middleware;
using N_Tier.Application;
using N_Tier.Application.Models.Validators;
using N_Tier.DataAccess;
using N_Tier.DataAccess.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

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

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseMiddleware<PerformanceMiddleware>();

app.UseMiddleware<TransactionMiddleware>();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

app.Run();

namespace N_Tier.API
{
    public partial class Program { }
}
