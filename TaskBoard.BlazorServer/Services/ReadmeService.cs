using Markdig;

namespace TaskBoard.BlazorServer.Services
{
    public class ReadmeService : IReadmeService
    {
        private readonly IWebHostEnvironment _environment;

        public ReadmeService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> GetReadmeContentAsync()
        {
            var readmePath = Path.Combine(_environment.ContentRootPath, "..", "README.md");
            if (!File.Exists(readmePath))
            {
                return "README.md not found";
            }

            var markdown = await File.ReadAllTextAsync(readmePath);
            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();
            
            return Markdown.ToHtml(markdown, pipeline);
        }
    }
}