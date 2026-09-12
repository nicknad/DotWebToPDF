using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DotWebToPdf.Tests;

[Trait("Category", "Integration")]
public sealed class DotWebToPdfGeneratorIntegrationTests
{
    private const string DelayedContentHtml = """
        <html><body>
        <script>
        setTimeout(() => {
            const element = document.createElement('div');
            element.id = 'ready';
            element.textContent = 'ready';
            document.body.appendChild(element);
        }, 400);
        </script>
        </body></html>
        """;

    [IntegrationFact]
    public async Task ToPdfAsync_GeneratesValidPdf_FromHtml()
    {
        await using var provider = CreateProvider();
        var generator = provider.GetRequiredService<IWebToPdfGenerator>();

        var pdf = await generator.ToPdfAsync("<html><body><h1>Hello</h1></body></html>");

        AssertValidPdf(pdf);
    }

    [IntegrationFact]
    public async Task ToPdfAsync_GeneratesValidPdf_FromComponent()
    {
        await using var provider = CreateProvider();
        var generator = provider.GetRequiredService<IWebToPdfGenerator>();

        var pdf = await generator.ToPdfAsync<TestComponent>(
            ParameterView.FromDictionary(new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                [nameof(TestComponent.Text)] = "Invoice",
            }));

        AssertValidPdf(pdf);
    }

    [IntegrationFact]
    public async Task ToPdfAsync_WaitsForSelectorAddedByScript()
    {
        await using var provider = CreateProvider();
        var generator = provider.GetRequiredService<IWebToPdfGenerator>();

        var pdf = await generator.ToPdfAsync(
            DelayedContentHtml,
            renderOptions: new RenderOptions
            {
                WaitForSelector = "#ready",
                WaitForSelectorState = PageElementState.Attached,
                Timeout = TimeSpan.FromSeconds(30),
            });

        AssertValidPdf(pdf);
    }

    [IntegrationFact]
    public async Task ToPdfAsync_UsesBaseUrl_ForRelativeUrls()
    {
        await using var provider = CreateProvider();
        var generator = provider.GetRequiredService<IWebToPdfGenerator>();

        const string baseUrl = "https://base.test/";
        var html = $$"""
            <html><head></head><body>
            <script>
            if (document.baseURI === '{{baseUrl}}') {
                const element = document.createElement('div');
                element.id = 'base-ready';
                element.textContent = 'ready';
                document.body.appendChild(element);
            }
            </script>
            </body></html>
            """;

        var pdf = await generator.ToPdfAsync(
            html,
            renderOptions: new RenderOptions
            {
                BaseUrl = new Uri(baseUrl),
                WaitForSelector = "#base-ready",
                Timeout = TimeSpan.FromSeconds(10),
            });

        AssertValidPdf(pdf);
    }

    [IntegrationFact]
    public async Task ToPdfAsync_SerializesRenders_WhenMaxConcurrentPagesIsOne()
    {
        await using var provider = CreateProvider(options => options.MaxConcurrentPages = 1);
        var generator = provider.GetRequiredService<IWebToPdfGenerator>();

        await generator.ToPdfAsync("<html><body>warmup</body></html>");

        var stopwatch = Stopwatch.StartNew();
        var renders = Enumerable.Range(0, 3)
            .Select(_ => generator.ToPdfAsync(
                DelayedContentHtml,
                renderOptions: new RenderOptions
                {
                    WaitForSelector = "#ready",
                    WaitForSelectorState = PageElementState.Attached,
                    Timeout = TimeSpan.FromSeconds(30),
                }))
            .ToArray();

        var pdfs = await Task.WhenAll(renders);
        stopwatch.Stop();

        Assert.All(pdfs, AssertValidPdf);
        Assert.True(
            stopwatch.Elapsed >= TimeSpan.FromMilliseconds(900),
            $"Renders should be serialized but completed in {stopwatch.Elapsed}.");
    }

    [IntegrationFact]
    public async Task ToPdfAsync_ThrowsTimeout_WhenTimeoutExpires()
    {
        await using var provider = CreateProvider();
        var generator = provider.GetRequiredService<IWebToPdfGenerator>();

        await generator.ToPdfAsync("<html><body>warmup</body></html>");

        var exception = await Assert.ThrowsAsync<TimeoutException>(() => generator.ToPdfAsync(
            "<html><body>timeout</body></html>",
            renderOptions: new RenderOptions
            {
                WaitForSelector = "#never",
                Timeout = TimeSpan.FromMilliseconds(500),
            }));

        Assert.Contains("500ms", exception.Message, StringComparison.Ordinal);
    }

    [IntegrationFact]
    public async Task ToPdfAsync_ThrowsOperationCanceled_WhenCancelledDuringWait()
    {
        await using var provider = CreateProvider();
        var generator = provider.GetRequiredService<IWebToPdfGenerator>();

        await generator.ToPdfAsync("<html><body>warmup</body></html>");

        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(300));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => generator.ToPdfAsync(
            "<html><body>cancel</body></html>",
            renderOptions: new RenderOptions
            {
                WaitForSelector = "#never",
                Timeout = TimeSpan.FromSeconds(30),
            },
            cancellationToken: cancellationTokenSource.Token));

        var pdf = await generator.ToPdfAsync("<html><body>after cancel</body></html>");

        AssertValidPdf(pdf);
    }

    private static ServiceProvider CreateProvider(Action<GeneratorOptions>? configure = null)
    {
        var services = new ServiceCollection();
        services.AddDotWebToPdfGenerator(configure);

        return services.BuildServiceProvider();
    }

    private static void AssertValidPdf(byte[] pdf)
    {
        Assert.True(pdf.Length > 100, "The generated PDF is unexpectedly small.");
        Assert.Equal("%PDF-", Encoding.ASCII.GetString(pdf, 0, 5));

        var tail = Encoding.ASCII.GetString(pdf, Math.Max(0, pdf.Length - 32), Math.Min(32, pdf.Length));
        Assert.Contains("%%EOF", tail, StringComparison.Ordinal);
    }

    [SuppressMessage("Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated by the Blazor renderer through reflection.")]
    private sealed class TestComponent : ComponentBase
    {
        [Parameter]
        public string? Text { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "h1");
            builder.AddContent(1, Text);
            builder.CloseElement();
        }
    }
}
