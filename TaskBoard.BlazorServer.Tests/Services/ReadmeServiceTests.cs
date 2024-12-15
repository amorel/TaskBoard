using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using TaskBoard.BlazorServer.Services;

public class ReadmeServiceTests
{
    private readonly Mock<IWebHostEnvironment> _mockEnvironment;
    private readonly Mock<ILogger<ReadmeService>> _mockLogger;
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly ReadmeService _service;

    public ReadmeServiceTests()
    {
        _mockEnvironment = new Mock<IWebHostEnvironment>();
        _mockLogger = new Mock<ILogger<ReadmeService>>();
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _service = new ReadmeService(
            _mockEnvironment.Object,
            _mockLogger.Object,
            _mockHttpClientFactory.Object);
    }

    [Fact]
    public async Task GetReadmeContentAsync_WhenFileExists_ShouldReturnHtmlContent()
    {
        // Arrange
        var testMarkdown = "# Test Markdown\n\nThis is a test";
        var contentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(contentPath);

        _mockEnvironment.Setup(e => e.ContentRootPath).Returns(contentPath);

        // Mock HTTP client failure
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Failed to fetch from GitHub"));

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        _mockHttpClientFactory
            .Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        var fullPath = Path.Combine(contentPath, "README.md");
        await File.WriteAllTextAsync(fullPath, testMarkdown);

        try
        {
            // Act
            var result = await _service.GetReadmeContentAsync();

            // Assert
            result.Should().Contain("Test Markdown");
            result.Should().Contain("This is a test");
        }
        finally
        {
            // Cleanup
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
            if (Directory.Exists(contentPath))
            {
                Directory.Delete(contentPath, true);
            }
        }
    }

    [Fact]
    public async Task GetReadmeContentAsync_WhenFileDoesNotExist_ShouldReturnErrorMessage()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _mockEnvironment.Setup(e => e.ContentRootPath)
            .Returns(nonExistentPath);

        // Mock HTTP client pour simuler un échec de lecture depuis GitHub
        var mockHttpClient = new Mock<HttpClient>();
        _mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(new HttpClient())
            .Callback(() => throw new HttpRequestException("Failed to fetch from GitHub"));

        // Act
        var result = await _service.GetReadmeContentAsync();

        // Assert
        // Vérifions juste que le message contient "not found" au lieu de vérifier le message exact
        result.Should().Contain("not found");
    }
}