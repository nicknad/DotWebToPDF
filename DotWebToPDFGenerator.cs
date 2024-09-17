using CommunityToolkit.Diagnostics;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using System.Diagnostics.CodeAnalysis;

namespace DotWebToPDF;

public class DotWebToPDFGenerator
{
    private readonly IServiceProvider _services;
    private readonly ILoggerFactory _loggerFactory;

    public DotWebToPDFGenerator(IServiceProvider services, ILoggerFactory loggerFactory)
    {
        Guard.IsNotNull(loggerFactory, nameof(loggerFactory));
        Guard.IsNotNull(services, nameof(services));
        this._services = services;
        this._loggerFactory = loggerFactory;
    }

    public async Task<byte[]> ToPDFAsync(string html, PagePdfOptions? pdfOptions = null)
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync();
        await page.SetContentAsync(html);

        return await page.PdfAsync(pdfOptions);
    }

    public async Task<byte[]> ToPDFAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TComponent>(
        ParameterView parameters,
        PagePdfOptions? pdfOptions = null) where TComponent : IComponent
    {
        await using var htmlRenderer = new HtmlRenderer(_services, _loggerFactory);

        var html = await htmlRenderer.Dispatcher.InvokeAsync(async () =>
        {
            var output = await htmlRenderer.RenderComponentAsync<TComponent>(parameters);
            return output.ToHtmlString();
        });

        return await ToPDFAsync(html, pdfOptions);
    }
}
