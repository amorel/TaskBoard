using Microsoft.Playwright;
using FluentAssertions;

namespace TaskBoard.PlaywrightTests;

public class TaskBoardTests
{
    private const string BaseUrl = "http://localhost:5201";

    [Fact]
    public async Task ShouldCreateNewTask()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false,
            SlowMo = 1000,
            Timeout = 60000
        });
        var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();

        try
        {
            Console.WriteLine("Navigating to taskboard page...");
            await page.GotoAsync($"{BaseUrl}/taskboard", new()
            {
                Timeout = 60000,
                WaitUntil = WaitUntilState.Load
            });

            Console.WriteLine("Waiting for page load...");
            await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

            // Debug - Liste tous les boutons de la page
            var buttons = await page.QuerySelectorAllAsync("button");
            foreach (var button in buttons)
            {
                var text = await button.TextContentAsync();
                Console.WriteLine($"Found button: {text}");
            }

            Console.WriteLine("Looking for Add button...");
            var addButton = await page.WaitForSelectorAsync("button:has-text('+ Ajouter une tâche')",
                new() { State = WaitForSelectorState.Visible, Timeout = 10000 });

            if (addButton == null)
            {
                throw new Exception("Add button not found");
            }

            Console.WriteLine("Clicking Add button...");
            await addButton.ClickAsync();
            await page.WaitForTimeoutAsync(2000);

            Console.WriteLine("Waiting for modal...");
            var modal = await page.WaitForSelectorAsync("div.modal.fade.show", new()
            {
                State = WaitForSelectorState.Visible,
                Timeout = 10000
            });

            if (modal == null)
            {
                await page.ScreenshotAsync(new()
                {
                    Path = "modal-not-found.png",
                    FullPage = true
                });
                throw new Exception("Modal not found");
            }

            Console.WriteLine("Filling form fields...");
            await page.FillAsync("#title", "Test Task");
            await page.FillAsync("#description", "Test Description");

            Console.WriteLine("Clicking Save button...");
            var saveButton = await page.WaitForSelectorAsync("button:has-text('Enregistrer')",
                new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
            await saveButton!.ClickAsync();

            Console.WriteLine("Waiting for task card...");
            var taskCard = await page.WaitForSelectorAsync(".card-title:has-text('Test Task')",
                new() { State = WaitForSelectorState.Visible, Timeout = 10000 });

            if (taskCard == null)
            {
                throw new Exception("Task card not found after saving");
            }

            var taskTitle = await taskCard.TextContentAsync();
            taskTitle.Should().Contain("Test Task");

            Console.WriteLine("Test completed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Test failed with error: {ex.Message}");
            await page.ScreenshotAsync(new()
            {
                Path = $"error-{DateTime.Now:yyyyMMddHHmmss}.png",
                FullPage = true
            });
            throw;
        }
    }

    [Fact]
    public async Task ShouldSynchronizeAcrossBrowsers()
    {
        using var playwright = await Playwright.CreateAsync();
        var launchOptions = new BrowserTypeLaunchOptions
        {
            Headless = false,
            SlowMo = 1000,
            Timeout = 60000
        };

        await using var browser1 = await playwright.Chromium.LaunchAsync(launchOptions);
        await using var browser2 = await playwright.Chromium.LaunchAsync(launchOptions);

        var context1 = await browser1.NewContextAsync();
        var context2 = await browser2.NewContextAsync();

        var page1 = await context1.NewPageAsync();
        var page2 = await context2.NewPageAsync();

        try
        {
            // Ouvrir l'application dans les deux navigateurs
            await page1.GotoAsync($"{BaseUrl}/taskboard", new()
            {
                Timeout = 60000,
                WaitUntil = WaitUntilState.Load
            });
            await page2.GotoAsync($"{BaseUrl}/taskboard", new()
            {
                Timeout = 60000,
                WaitUntil = WaitUntilState.Load
            });

            // Attendre que les pages soient chargées
            await page1.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
            await page2.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

            await Task.Delay(3000);

            // Créer une tâche dans le premier navigateur
            var addButton = await page1.WaitForSelectorAsync("button:has-text('Ajouter une tâche')",
                new() { State = WaitForSelectorState.Visible, Timeout = 10000 });
            await addButton!.ClickAsync();

            // Attendre le modal et remplir le formulaire
            await page1.WaitForSelectorAsync(".modal-dialog",
                new() { State = WaitForSelectorState.Visible, Timeout = 10000 });

            await page1.FillAsync("#title", "Sync Test Task");
            await page1.FillAsync("#description", "Testing real-time sync");
            await page1.ClickAsync("button:has-text('Enregistrer')");

            Console.WriteLine("Waiting to observe synchronization (5 seconds)...");
            await Task.Delay(5000);

            // Vérifier dans le second navigateur
            var taskCard = await page2.WaitForSelectorAsync(".card-title:has-text('Sync Test Task')",
                new() { State = WaitForSelectorState.Visible, Timeout = 10000 });
            var taskTitle = await taskCard!.TextContentAsync();
            taskTitle.Should().Contain("Sync Test Task");

            Console.WriteLine("Test successful! Waiting 3 seconds before closing...");
            await Task.Delay(3000);
        }
        catch (Exception ex)
        {
            // Capture des screenshots en cas d'erreur
            await page1.ScreenshotAsync(new()
            {
                Path = "error-browser1.png",
                FullPage = true
            });
            await page2.ScreenshotAsync(new()
            {
                Path = "error-browser2.png",
                FullPage = true
            });
            throw;
        }
    }
}