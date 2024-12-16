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
        private readonly Mock<ITaskHubClient> _mockTaskHubClient;
        private readonly List<TaskViewModel> _todoTasks;
        private readonly List<TaskViewModel> _inProgressTasks;
        private readonly List<TaskViewModel> _doneTasks;

        public TaskBoardPageTests()
        {
            _mockTaskService = new Mock<ITaskService>();
            _mockTaskHubClient = new Mock<ITaskHubClient>();
            Services.AddSingleton(_mockTaskService.Object);
            Services.AddSingleton(_mockTaskHubClient.Object);

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
            _mockTaskHubClient
           .Setup(h => h.Connect())
           .Returns(Task.CompletedTask);

            _mockTaskHubClient
                .Setup(h => h.Disconnect())
                .Returns(Task.CompletedTask);
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
            var taskLists = new Dictionary<TaskState, List<TaskViewModel>>();
            var initialTodoTask = new TaskViewModel
            {
                Id = Guid.NewGuid(),
                Title = "Initial Todo",
                Status = TaskState.Todo
            };

            taskLists[TaskState.Todo] = new List<TaskViewModel> { initialTodoTask };
            taskLists[TaskState.InProgress] = new List<TaskViewModel>();
            taskLists[TaskState.Done] = new List<TaskViewModel>();

            // Configure initial state
            _mockTaskService
                .Setup(s => s.GetTasksByStateAsync(TaskState.Todo))
                .ReturnsAsync(() => new List<TaskViewModel>(taskLists[TaskState.Todo]));

            var cut = RenderComponent<TaskBoardPage>();

            // Attendre le premier rendu
            await cut.InvokeAsync(() => Task.Delay(100));

            // Simuler l'ajout d'une nouvelle tâche
            var newTodoTask = new TaskViewModel
            {
                Id = Guid.NewGuid(),
                Title = "New Todo",
                Status = TaskState.Todo
            };

            // Ajouter la nouvelle tâche à la liste
            taskLists[TaskState.Todo].Add(newTodoTask);

            // Mettre à jour la configuration du mock pour retourner la liste mise à jour
            _mockTaskService
                .Setup(s => s.GetTasksByStateAsync(TaskState.Todo))
                .ReturnsAsync(() => new List<TaskViewModel>(taskLists[TaskState.Todo]));

            // Déclencher OnChange et attendre le rafraîchissement
            await cut.InvokeAsync(async () =>
            {
                _mockTaskService.Raise(m => m.OnChange += null);
                // Attendre que le rafraîchissement soit terminé
                await Task.Delay(100);
            });

            // Assert
            var todoColumn = cut.Find("[data-column='Todo']");
            var todoTasks = todoColumn.QuerySelectorAll(".task-card");
            todoTasks.Length.Should().Be(2, "because two tasks should be displayed");

            // Vérifier que les tâches ont les bons titres
            foreach (var task in todoTasks)
            {
                var titleElement = task.QuerySelector(".card-title");
                titleElement.Should().NotBeNull("each task card should have a title element");
                var title = titleElement!.TextContent.Trim();
                title.Should().NotBeNullOrEmpty("task title should not be empty");
                title.Should().BeOneOf("Initial Todo", "New Todo", "because these are the expected task titles");
            }

            _mockTaskService.Verify(s => s.GetTasksByStateAsync(TaskState.Todo), Times.AtLeast(2));
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
