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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

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

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();