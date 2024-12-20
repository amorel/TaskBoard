using TaskBoard.Infrastructure;
using TaskBoard.BlazorServer.Services;
using TaskBoard.Application.Tasks.Queries.GetAllTasks;
using TaskBoard.Application.Tasks.Queries.GetTaskById;
using TaskBoard.Application.Tasks.Queries.GetTasksByState;
using TaskBoard.Application.Tasks.Commands.CreateTask;
using TaskBoard.Application.Tasks.Commands.UpdateTask;
using TaskBoard.Application.Tasks.Commands.DeleteTask;
using TaskBoard.Application.Common.Interfaces;
using TaskBoard.Domain.Entities;
using TaskBoard.Application.Common.Models;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using TaskBoard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using TaskBoard.BlazorServer.Hubs;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine("Application starting...");

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Services.AddApplicationInsightsTelemetry();

builder.Logging.SetMinimumLevel(LogLevel.Trace);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
});
builder.Services.AddHttpClient();

builder.Services.AddServerSideBlazor(options =>
{
    options.DetailedErrors = true;
    options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(3);
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

// Configuration du logging
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
    logging.SetMinimumLevel(LogLevel.Debug);
});

// Configurer les options de circuit
builder.Services.Configure<CircuitOptions>(options =>
{
    options.DetailedErrors = true;
});

builder.Services.Configure<HubOptions>(options =>
{
    options.EnableDetailedErrors = true;
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.HandshakeTimeout = TimeSpan.FromSeconds(15);
});

builder.Services.Configure<HubConnectionContextOptions>(options =>
{
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

// Add Infrastructure services
builder.Services.AddInfrastructure(
    builder.Configuration.GetConnectionString("DefaultConnection")!);

// Add Application services
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ITaskHubClient, TaskHubClient>();

// Register Query Handlers
builder.Services.AddScoped<IQueryHandler<GetAllTasksQuery, IEnumerable<BoardTask>>, GetAllTasksQueryHandler>();
builder.Services.AddScoped<IQueryHandler<GetTaskByIdQuery, BoardTask?>, GetTaskByIdQueryHandler>();
builder.Services.AddScoped<IQueryHandler<GetTasksByStateQuery, IEnumerable<BoardTask>>, GetTasksByStateQueryHandler>();

// Register Command Handlers
builder.Services.AddScoped<ICommandHandler<CreateTaskCommand, BoardTask>, CreateTaskCommandHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateTaskCommand, BoardTask>, UpdateTaskCommandHandler>();
builder.Services.AddScoped<ICommandHandler<DeleteTaskCommand, Unit>, DeleteTaskCommandHandler>();

builder.Services.AddScoped<IReadmeService, ReadmeService>();

var app = builder.Build();
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Starting application...");
logger.LogInformation("ContentRoot Path: {Path}", app.Environment.ContentRootPath);
logger.LogInformation("WebRoot Path: {Path}", app.Environment.WebRootPath);
try
{
    var dbPath = builder.Configuration.GetConnectionString("DefaultConnection")
        ?.Replace("Data Source=", "")
        ?.Trim();

    if (!string.IsNullOrEmpty(dbPath))
    {
        logger.LogInformation("Database Path: {path}", dbPath);

        // Obtenir le répertoire de la base de données
        var directory = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            logger.LogInformation("Creating database directory: {dir}", directory);
            Directory.CreateDirectory(directory);
        }

        logger.LogInformation("Database File Exists: {exists}", File.Exists(dbPath));
    }
}
catch (Exception ex)
{
    logger.LogError(ex, "Error configuring database path");
    throw; // Important de relancer l'exception
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TaskBoardDbContext>();
    try
    {
        logger.LogInformation("Applying database migrations...");
        await db.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error applying database migrations");
        throw;
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "text/html";

            var exceptionHandlerPathFeature =
                context.Features.Get<IExceptionHandlerPathFeature>();

            if (exceptionHandlerPathFeature?.Error != null)
            {
                logger.LogError(exceptionHandlerPathFeature.Error,
                    "Une erreur non gérée est survenue : {ErrorMessage}",
                    exceptionHandlerPathFeature.Error.Message);

                // En production, ajouter plus d'informations dans le log
                if (!app.Environment.IsDevelopment())
                {
                    logger.LogError("Stack trace: {StackTrace}",
                        exceptionHandlerPathFeature.Error.StackTrace);
                }

                await context.Response.WriteAsync($@"
                    <html>
                        <head>
                            <title>Erreur - TaskBoard</title>
                            <link href=""css/bootstrap/bootstrap.min.css"" rel=""stylesheet"" />
                        </head>
                        <body class=""container mt-5"">
                            <div class=""alert alert-danger"">
                                <h4>Une erreur est survenue</h4>
                                <p>Nous avons été notifiés et travaillons à résoudre le problème.</p>
                                <hr>
                                <p class=""mb-0"">
                                    <a href=""/"" class=""btn btn-primary"">Retour à l'accueil</a>
                                </p>
                            </div>
                        </body>
                    </html>");
            }
        });
    });
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
    builder.Configuration["BaseUrl"] = "http://localhost:5201";
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapHub<TaskHub>("/taskhub");
app.MapFallbackToPage("/_Host");

app.Run();