using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

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
            var readmePath = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, "..", "README.md"));

            if (!File.Exists(readmePath))
            {
                return "README.md not found";
            }

            Console.WriteLine($"Trying to read README.md from: {readmePath}");

            var markdown = await File.ReadAllTextAsync(readmePath);

            // Configuration du pipeline Markdown
            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();

            // Création d'un document Markdown
            var document = Markdown.Parse(markdown, pipeline);

            // Ajustement des chemins d'images
            AdjustImagePaths(document);

            // Conversion en HTML
            using var writer = new StringWriter();
            var renderer = new HtmlRenderer(writer);
            pipeline.Setup(renderer);
            renderer.Render(document);

            return writer.ToString();
        }

        private void AdjustImagePaths(MarkdownDocument document)
        {
            // Parcours de tous les blocs du document
            foreach (var descendant in document.Descendants())
            {
                if (descendant is LinkInline { IsImage: true } linkInline)
                {
                    var url = linkInline.Url;
                    if (url != null)
                    {
                        // Conversion du chemin relatif en chemin absolu pour wwwroot
                        if (url.StartsWith("./"))
                        {
                            url = url.Substring(2);
                        }
                        linkInline.Url = url.Replace('\\', '/');
                    }
                }
            }
        }
    }
}