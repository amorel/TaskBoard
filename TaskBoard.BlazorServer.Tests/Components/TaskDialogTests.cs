using Bunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using TaskBoard.BlazorServer.Components;
using TaskBoard.BlazorServer.ViewModels;
using TaskBoard.Domain.Enums;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;

namespace TaskBoard.BlazorServer.Tests.Components
{
    public class TaskDialogTests : TestContext
    {
        private readonly TaskViewModel _testTask;

        public TaskDialogTests()
        {
            DefaultWaitTimeout = TimeSpan.FromSeconds(15);
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
        public void TaskDialog_WhenCreatingNewTask_ShouldShowEmptyForm()
        {
            // Arrange & Act
            var cut = RenderComponent<TaskDialog>(parameters => parameters
                .Add(p => p.TaskState, TaskState.Todo));

            // Assert
            var titleInput = cut.Find("#title");
            var descriptionInput = cut.Find("#description");
            var statusSelect = cut.Find("#status");

            titleInput.Attributes["value"]?.Value.Should().BeEmpty();
            descriptionInput.TextContent.Should().BeEmpty();
            statusSelect.Attributes["value"]?.Value.Should().Be(TaskState.Todo.ToString());
        }

        [Fact]
        public void TaskDialog_WhenEditingTask_ShouldShowTaskData()
        {
            // Arrange & Act
            var cut = RenderComponent<TaskDialog>(parameters => parameters
                .Add(p => p.Task, _testTask));

            // Assert
            var titleInput = cut.Find("#title");
            var descriptionInput = cut.Find("#description");
            var statusSelect = cut.Find("#status");

            titleInput.GetAttribute("value").Should().Be(_testTask.Title);
            descriptionInput.GetAttribute("value").Should().Be(_testTask.Description);
            statusSelect.GetAttribute("value").Should().Be(_testTask.Status.ToString());
        }

        [Fact]
        public async Task SaveButton_WithValidData_ShouldTriggerOnSave()
        {
            // Arrange
            TaskViewModel? savedTask = null;
            var cut = RenderComponent<TaskDialog>(parameters => parameters
                .Add(p => p.Task, _testTask)
                .Add(p => p.OnSave, EventCallback.Factory.Create<TaskViewModel>(this,
                    task => savedTask = task)));

            var titleInput = cut.Find("#title");
            var descriptionInput = cut.Find("#description");

            // Act
            await titleInput.ChangeAsync(new ChangeEventArgs { Value = "Updated Title" });
            await descriptionInput.ChangeAsync(new ChangeEventArgs { Value = "Updated Description" });
            var form = cut.Find("form");
            await form.SubmitAsync();

            // Assert
            savedTask.Should().NotBeNull();
            savedTask!.Title.Should().Be("Updated Title");
            savedTask.Description.Should().Be("Updated Description");
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public async Task SaveButton_WithInvalidTitle_ShouldShowValidationMessage(string invalidTitle)
        {
            // Arrange
            var cut = RenderComponent<TaskDialog>(parameters => parameters
                .Add(p => p.Task, _testTask));

            var titleInput = cut.Find("#title");
            var form = cut.Find("form");

            // Act
            await titleInput.ChangeAsync(new ChangeEventArgs { Value = invalidTitle });
            await form.SubmitAsync();

            // Wait with extended timeout
            cut.WaitForState(() => cut.Markup.Contains("Le titre est requis"),
                TimeSpan.FromSeconds(15));

            // Assert
            cut.Markup.Should().Contain("Le titre est requis");
        }

        [Fact]
        public void CloseButton_ShouldTriggerOnClose()
        {
            // Arrange
            var closed = false;
            var cut = RenderComponent<TaskDialog>(parameters => parameters
                .Add(p => p.Task, _testTask)
                .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => closed = true)));

            // Act
            var closeButton = cut.Find("button.btn-secondary");
            closeButton.Click(new MouseEventArgs());

            // Assert
            closed.Should().BeTrue();
        }
    }
}