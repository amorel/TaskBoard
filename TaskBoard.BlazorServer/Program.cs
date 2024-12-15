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

// Add Infrastructure services
builder.Services.AddInfrastructure(
    builder.Configuration.GetConnectionString("DefaultConnection")!);

// Add Application services
builder.Services.AddScoped<ITaskService, TaskService>();

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

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TaskBoardDbContext>();
    try
    {
        Console.WriteLine("Attempting to apply migrations...");
        await db.Database.MigrateAsync();
        Console.WriteLine("Migrations applied successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while applying migrations: {ex.Message}");
        throw; // Relancer l'exception pour être sûr que l'app ne démarre pas sans BDD
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    var folder = Path.GetDirectoryName(connectionString);

    Console.WriteLine($"Connection string: {connectionString}");
    Console.WriteLine($"Folder path: {folder}");

    if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
    {
        try
        {
            Directory.CreateDirectory(folder);
            Console.WriteLine($"Created directory: {folder}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating directory: {ex.Message}");
        }
    }

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
                var logger = context.RequestServices
                    .GetRequiredService<ILogger<Program>>();

                logger.LogError(exceptionHandlerPathFeature.Error,
                    "Une erreur est survenue : {ErrorMessage}",
                    exceptionHandlerPathFeature.Error.Message);

                await context.Response.WriteAsync("<html><body>\n");
                await context.Response.WriteAsync(
                    "<h2>Une erreur est survenue dans l'application.</h2>\n");
                await context.Response.WriteAsync("<hr>\n");
                await context.Response.WriteAsync("</body></html>\n");
            }
        });
    });
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();