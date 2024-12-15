using Bunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using TaskBoard.BlazorServer.Components;
using TaskBoard.BlazorServer.Services;
using TaskBoard.BlazorServer.ViewModels;
using TaskBoard.Domain.Enums;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace TaskBoard.BlazorServer.Tests.Components
{
    public class TaskCardTests : TestContext
    {
        private readonly Mock<ITaskService> _mockTaskService;
        private readonly Mock<IJSRuntime> _mockJsRuntime;
        private readonly TaskViewModel _testTask;

        public TaskCardTests()
        {
            _mockTaskService = new Mock<ITaskService>();
            _mockJsRuntime = new Mock<IJSRuntime>();

            Services.AddSingleton(_mockTaskService.Object);
            Services.AddSingleton(_mockJsRuntime.Object);

            _testTask = new TaskViewModel
            {
                Id = Guid.NewGuid(),
                Title = "Test Task",
                Description = "Test Description",
                Status = TaskState.Todo,
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            };
        }


        [Fact]
        public async Task DeleteButton_WhenConfirmed_ShouldCallDeleteService()
        {
            // Arrange
            _mockJsRuntime
                .Setup(x => x.InvokeAsync<bool>(It.Is<string>(s => s == "confirm"), It.IsAny<object[]>()))
                .ReturnsAsync(true);

            var onTaskDeletedCalled = false;
            var cut = RenderComponent<TaskCard>(parameters => parameters
                .Add(p => p.BoardTask, _testTask)
                .Add(p => p.OnTaskDeleted, EventCallback.Factory.Create(this, () => onTaskDeletedCalled = true)));

            // Act
            var deleteButton = cut.Find("button.btn-danger");
            await deleteButton.ClickAsync(new MouseEventArgs());

            // Assert
            _mockTaskService.Verify(s => s.DeleteTaskAsync(_testTask.Id), Times.Once);
            onTaskDeletedCalled.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteButton_WhenNotConfirmed_ShouldNotCallDeleteService()
        {
            // Arrange
            _mockJsRuntime
                .Setup(x => x.InvokeAsync<bool>(It.Is<string>(s => s == "confirm"), It.IsAny<object[]>()))
                .ReturnsAsync(false);  // Important : retourne false

            var cut = RenderComponent<TaskCard>(parameters => parameters
                .Add(p => p.BoardTask, _testTask));

            // Act
            var deleteButton = cut.Find("button.btn-danger");
            await deleteButton.ClickAsync(new MouseEventArgs());

            // Assert
            _mockTaskService.Verify(s =>
                s.DeleteTaskAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public void EditButton_ShouldShowTaskDialog()
        {
            // Arrange & Act
            var cut = RenderComponent<TaskCard>(parameters => parameters
                .Add(p => p.BoardTask, _testTask));

            // Act
            var editButton = cut.Find("button.btn-primary");
            editButton.Click(new MouseEventArgs());

            // Assert
            cut.FindComponent<TaskDialog>().Should().NotBeNull();
        }

        [Fact]
        public void TaskCard_ShouldRenderTaskDetails()
        {
            // Arrange & Act
            var cut = RenderComponent<TaskCard>(parameters => parameters
                .Add(p => p.BoardTask, _testTask));

            // Assert
            cut.Find(".card-title").TextContent.Should().Be(_testTask.Title);
            cut.Find(".card-text").TextContent.Should().Be(_testTask.Description);
        }

        [Fact]
        public void TaskCard_ShouldShowButtons()
        {
            // Arrange & Act
            var cut = RenderComponent<TaskCard>(parameters => parameters
                .Add(p => p.BoardTask, _testTask));

            // Assert
            cut.FindAll("button").Count.Should().Be(2); // Edit et Delete
            cut.Find("button.btn-primary").TextContent.Should().Contain("Éditer");
            cut.Find("button.btn-danger").TextContent.Should().Contain("Supprimer");
        }
    }
}