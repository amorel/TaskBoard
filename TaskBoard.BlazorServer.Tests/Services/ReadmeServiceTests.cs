using Microsoft.AspNetCore.Hosting;
using Moq;
using FluentAssertions;
using TaskBoard.BlazorServer.Services;

namespace TaskBoard.BlazorServer.Tests.Services
{
    public class ReadmeServiceTests
    {
        private readonly Mock<IWebHostEnvironment> _mockEnvironment;
        private readonly ReadmeService _service;
        private readonly string _testContent = "# Test Markdown\n\nThis is a test";

        public ReadmeServiceTests()
        {
            _mockEnvironment = new Mock<IWebHostEnvironment>();
            _service = new ReadmeService(_mockEnvironment.Object);
        }

        [Fact]
        public async Task GetReadmeContentAsync_WhenFileExists_ShouldReturnHtmlContent()
        {
            // Arrange
            var contentPath = Path.Combine("TestPath");
            _mockEnvironment.Setup(e => e.ContentRootPath).Returns(contentPath);

            var fullPath = Path.Combine(contentPath, "..", "README.md");
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            await File.WriteAllTextAsync(fullPath, _testContent);

            // Act
            var result = await _service.GetReadmeContentAsync();

            // Assert
            result.Should().Contain("Test Markdown");
            result.Should().Contain("This is a test");
            result.Should().StartWith("<h1");
            result.Should().Contain("</h1>");

            // Cleanup
            File.Delete(fullPath);
        }

        [Fact]
        public async Task GetReadmeContentAsync_WhenFileDoesNotExist_ShouldReturnErrorMessage()
        {
            // Arrange
            var nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            _mockEnvironment.Setup(e => e.ContentRootPath)
                .Returns(nonExistentPath);

            // Act
            var result = await _service.GetReadmeContentAsync();

            // Assert
            result.Should().Be("README.md not found");
        }
    }
}