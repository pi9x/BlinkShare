using BlinkShare.Api.Common.Auth;
using BlinkShare.Api.Common.Endpoints;
using BlinkShare.Api.Features.AnonymousSessions;
using BlinkShare.Api.Features.Quotas;
using BlinkShare.Api.Features.Shares.CreateFileUpload;
using BlinkShare.Api.Features.Shares.CreateText;
using BlinkShare.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBlinkShareInfrastructure(builder.Configuration);
builder.Services.AddBlinkShareAuth(builder.Configuration);
builder.Services.AddAnonymousSessionFeatures(builder.Configuration);
builder.Services.AddQuotaFeature(builder.Configuration);
builder.Services.AddCreateTextFeature(builder.Configuration);
builder.Services.AddCreateFileUploadFeature(builder.Configuration);
builder.Services.AddBlinkShareEndpoints();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapBlinkShareEndpoints();
app.MapHub<AnonymousSessionHub>("/hubs/anonymous-session");

app.Run();

public partial class Program;
