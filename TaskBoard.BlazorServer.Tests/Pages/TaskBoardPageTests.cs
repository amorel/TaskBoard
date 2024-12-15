using Bunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using TaskBoard.BlazorServer.Services;
using TaskBoard.BlazorServer.Pages;
using TaskBoard.BlazorServer.ViewModels;
using TaskBoard.Domain.Enums;
using Microsoft.AspNetCore.Components;
using AngleSharp.Dom;
using Microsoft.AspNetCore.Components.Web;
using TaskBoard.BlazorServer.Components;

namespace TaskBoard.BlazorServer.Tests.Pages
{

    public class TaskBoardPageTests : TestContext
    {
        private readonly Mock<ITaskService> _mockTaskService;
        private readonly List<TaskViewModel> _todoTasks;
        private readonly List<TaskViewModel> _inProgressTasks;
        private readonly List<TaskViewModel> _doneTasks;

        public TaskBoardPageTests()
        {
            _mockTaskService = new Mock<ITaskService>();
            Services.AddSingleton(_mockTaskService.Object);

            _todoTasks = new List<TaskViewModel>
        {
            new() { Id = Guid.NewGuid(), Title = "Todo Task 1", Status = TaskState.Todo },
            new() { Id = Guid.NewGuid(), Title = "Todo Task 2", Status = TaskState.Todo }
        };

            _inProgressTasks = new List<TaskViewModel>
        {
            new() { Id = Guid.NewGuid(), Title = "In Progress Task", Status = TaskState.InProgress }
        };

            _doneTasks = new List<TaskViewModel>
        {
            new() { Id = Guid.NewGuid(), Title = "Done Task", Status = TaskState.Done }
        };

            _mockTaskService
                .Setup(s => s.GetTasksByStateAsync(TaskState.Todo))
                .ReturnsAsync(_todoTasks);

            _mockTaskService
                .Setup(s => s.GetTasksByStateAsync(TaskState.InProgress))
                .ReturnsAsync(_inProgressTasks);

            _mockTaskService
                .Setup(s => s.GetTasksByStateAsync(TaskState.Done))
                .ReturnsAsync(_doneTasks);
        }

        [Fact]
        public void TaskBoardPage_ShouldRenderAllColumns()
        {
            // Act
            var cut = RenderComponent<TaskBoardPage>();

            // Assert
            var columns = cut.FindAll(".card > .card-header").Count;
            columns.Should().Be(3); // Three columns

            var taskCards = cut.FindComponents<TaskCard>();
            taskCards.Count.Should().Be(4); // Total number of tasks
        }

        [Fact]
        public async Task TaskBoardPage_ShouldRefreshTasksAfterCreation()
        {
            // Arrange
            var newTask = new TaskViewModel
            {
                Id = Guid.NewGuid(),
                Title = "New Task",
                Status = TaskState.Todo
            };

            _mockTaskService
                .Setup(s => s.CreateTaskAsync(It.IsAny<TaskViewModel>()))
                .ReturnsAsync(newTask);

            var cut = RenderComponent<TaskBoardPage>();

            // Act
            var addButton = cut.Find("button.btn-primary");
            await cut.InvokeAsync(() => addButton.Click());

            // Simuler la sauvegarde et déclencher l'événement
            await cut.InvokeAsync(async () =>
            {
                await _mockTaskService.Object.CreateTaskAsync(newTask);
                _mockTaskService.Raise(m => m.OnChange += null);
            });

            // Assert
            _mockTaskService.Verify(s => s.GetTasksByStateAsync(It.IsAny<TaskState>()), Times.AtLeast(3));
        }

        [Fact]
        public void TaskBoardPage_ShouldRenderCorrectTasksInEachColumn()
        {
            // Act
            var cut = RenderComponent<TaskBoardPage>();

            // Assert
            var todoColumn = cut.Find("div:has(> .card-header:contains('À faire'))");
            var inProgressColumn = cut.Find("div:has(> .card-header:contains('En cours'))");
            var doneColumn = cut.Find("div:has(> .card-header:contains('Terminé'))");

            todoColumn.QuerySelectorAll(".card-body > .card").Length.Should().Be(2);
            inProgressColumn.QuerySelectorAll(".card-body > .card").Length.Should().Be(1);
            doneColumn.QuerySelectorAll(".card-body > .card").Length.Should().Be(1);
        }

        [Fact]
        public async Task TaskBoardPage_ShouldHandleTaskServiceEvents()
        {
            // Arrange
            var taskLists = new Dictionary<TaskState, List<TaskViewModel>>
            {
                [TaskState.Todo] = new List<TaskViewModel> { new() { Title = "Initial Todo" } },
                [TaskState.InProgress] = new List<TaskViewModel> { new() { Title = "Initial InProgress" } },
                [TaskState.Done] = new List<TaskViewModel> { new() { Title = "Initial Done" } }
            };

            _mockTaskService
                .Setup(s => s.GetTasksByStateAsync(It.IsAny<TaskState>()))
                .ReturnsAsync((TaskState state) => taskLists[state]);

            var cut = RenderComponent<TaskBoardPage>();

            // Modifier les listes de tâches
            taskLists[TaskState.Todo].Add(new TaskViewModel { Title = "New Todo" });
            taskLists[TaskState.InProgress].Add(new TaskViewModel { Title = "New InProgress" });
            taskLists[TaskState.Done].Add(new TaskViewModel { Title = "New Done" });

            // Act
            await cut.InvokeAsync(() =>
            {
                _mockTaskService.Raise(m => m.OnChange += null);
                return Task.CompletedTask;
            });

            // Forcer le re-rendu
            cut.Render();

            // Assert
            var todoTasks = cut.FindAll("div:has(> .card-header:contains('À faire')) .card-body > .card");
            var inProgressTasks = cut.FindAll("div:has(> .card-header:contains('En cours')) .card-body > .card");
            var doneTasks = cut.FindAll("div:has(> .card-header:contains('Terminé')) .card-body > .card");

            todoTasks.Count.Should().Be(2);
            inProgressTasks.Count.Should().Be(2);
            doneTasks.Count.Should().Be(2);

            _mockTaskService.Verify(s => s.GetTasksByStateAsync(It.IsAny<TaskState>()), Times.AtLeast(6)); // 3 initial + 3 after change
        }



        [Fact]
        public async Task CreateTask_ShouldShowDialogAndCreateTask()
        {
            // Arrange
            var newTask = new TaskViewModel
            {
                Id = Guid.NewGuid(),
                Title = "New Task",
                Status = TaskState.Todo
            };

            _mockTaskService
                .Setup(s => s.CreateTaskAsync(It.IsAny<TaskViewModel>()))
                .ReturnsAsync(newTask);

            var cut = RenderComponent<TaskBoardPage>();

            // Act
            var addButtons = cut.FindAll("button.btn-primary");
            var todoColumnAddButton = addButtons
                .FirstOrDefault(b => b.TextContent.Contains("Ajouter une tâche"));

            todoColumnAddButton.Should().NotBeNull();
            await cut.InvokeAsync(() => todoColumnAddButton!.Click());

            // Assert
            var taskDialog = cut.FindComponent<TaskDialog>();
            taskDialog.Should().NotBeNull();
            taskDialog.Instance.TaskState.Should().Be(TaskState.Todo);
        }
    }
}
