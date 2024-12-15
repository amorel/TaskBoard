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
        private readonly ILogger<ReadmeService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public ReadmeService(IWebHostEnvironment environment, ILogger<ReadmeService> logger, IHttpClientFactory httpClientFactory)
        {
            _environment = environment;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> GetReadmeContentAsync()
        {
            try
            {
                try
                {
                    // Utiliser l'URL raw de GitHub
                    var githubUrl = "https://raw.githubusercontent.com/amorel/TaskBoard/master/README.md";
                    using var httpClient = _httpClientFactory.CreateClient();
                    var markdown = await httpClient.GetStringAsync(githubUrl);

                    if (!string.IsNullOrEmpty(markdown))
                    {
                        _logger.LogInformation("README.md lu depuis GitHub avec succès");
                        return ConvertMarkdownToHtml(markdown);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Impossible de lire depuis GitHub: {ex.Message}");
                }

                // Essayer plusieurs chemins possibles
                var possiblePaths = new[]
                {
                // Chemin direct depuis la racine du projet
                Path.Combine(_environment.ContentRootPath, "README.md"),
                // Chemin en remontant d'un niveau
                Path.GetFullPath(Path.Combine(_environment.ContentRootPath, "..", "README.md")),
                // Chemin en remontant de deux niveaux
                Path.GetFullPath(Path.Combine(_environment.ContentRootPath, "..", "..", "README.md"))
            };

                _logger.LogInformation("Recherche du README.md dans les chemins suivants :");
                foreach (var path in possiblePaths)
                {
                    _logger.LogInformation($"Tentative de lecture depuis : {path}");
                    if (File.Exists(path))
                    {
                        _logger.LogInformation($"README.md trouvé à : {path}");
                        var markdown = await File.ReadAllTextAsync(path);

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
                }

                _logger.LogWarning("README.md non trouvé dans tous les chemins possibles");
                return "README.md not found";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la lecture du README.md");
                return $"Error reading README.md: {ex.Message}";
            }
        }

        private string ConvertMarkdownToHtml(string markdown)
        {
            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();

            var document = Markdown.Parse(markdown, pipeline);
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