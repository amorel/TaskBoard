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
                new() {
                    Id = Guid.NewGuid(),
                    Title = "Todo Task 1",
                    Status = TaskState.Todo,
                    CreatedAt = DateTime.UtcNow,
                    LastModifiedAt = DateTime.UtcNow
                },
                new() {
                    Id = Guid.NewGuid(),
                    Title = "Todo Task 2",
                    Status = TaskState.Todo,
                    CreatedAt = DateTime.UtcNow,
                    LastModifiedAt = DateTime.UtcNow
                }
            };

            _inProgressTasks = new List<TaskViewModel>
            {
                new() {
                    Id = Guid.NewGuid(),
                    Title = "In Progress Task",
                    Status = TaskState.InProgress,
                    CreatedAt = DateTime.UtcNow,
                    LastModifiedAt = DateTime.UtcNow
                }
            };

            _doneTasks = new List<TaskViewModel>
            {
                new() {
                    Id = Guid.NewGuid(),
                    Title = "Done Task",
                    Status = TaskState.Done,
                    CreatedAt = DateTime.UtcNow,
                    LastModifiedAt = DateTime.UtcNow
                }
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
            var columns = cut.FindAll(".card-header").Count;
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
        public async Task TaskBoardPage_ShouldRenderCorrectTasksInEachColumn()
        {
            // Act
            var cut = RenderComponent<TaskBoardPage>();
            await cut.InvokeAsync(() => Task.CompletedTask);

            // Assert
            var todoColumn = TestHelpers.FindColumnByState(cut, TaskState.Todo);
            var inProgressColumn = TestHelpers.FindColumnByState(cut, TaskState.InProgress);
            var doneColumn = TestHelpers.FindColumnByState(cut, TaskState.Done);

            var todoTasks = TestHelpers.GetTasksInColumn(todoColumn);
            var inProgressTasks = TestHelpers.GetTasksInColumn(inProgressColumn);
            var doneTasks = TestHelpers.GetTasksInColumn(doneColumn);

            todoTasks.Length.Should().Be(2);
            inProgressTasks.Length.Should().Be(1);
            doneTasks.Length.Should().Be(1);
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
            await cut.InvokeAsync(() => Task.CompletedTask);

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
            var todoTasks = cut.Find("[data-column='Todo']").QuerySelectorAll(".task-card");
            var inProgressTasks = cut.Find("[data-column='InProgress']").QuerySelectorAll(".task-card");
            var doneTasks = cut.Find("[data-column='Done']").QuerySelectorAll(".task-card");

            todoTasks.Length.Should().Be(2);
            inProgressTasks.Length.Should().Be(2);
            doneTasks.Length.Should().Be(2);

            _mockTaskService.Verify(s => s.GetTasksByStateAsync(It.IsAny<TaskState>()), Times.AtLeast(6));
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

        private static class TestHelpers
        {
            public static IElement FindColumnByState(IRenderedFragment cut, TaskState state)
            {
                return cut.Find($"[data-column='{state}']");
            }

            public static IHtmlCollection<IElement> GetTasksInColumn(IElement column)
            {
                return column.QuerySelectorAll(".task-card");
            }
        }
    }
}
