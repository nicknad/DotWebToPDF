using CommunityToolkit.Diagnostics;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using System.Diagnostics.CodeAnalysis;

namespace DotWebToPDF;

public static class PDFGeneratorExtension
{
    public static IServiceCollection AddDotWebToPDFGenerator(this IServiceCollection services)
    {
        services.AddScoped<PDFGenerator>();

        return services;
    }

    public static async Task<byte[]> ToPDFAsync(this PDFGenerator self, string html, PagePdfOptions? pdfOptions = null)
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync();
        await page.SetContentAsync(html);

        return await page.PdfAsync(pdfOptions);
    }

    public static async Task<byte[]> ToPDFAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TComponent>
        (this PDFGenerator self, ParameterView parameters, PagePdfOptions? pdfOptions = null) where TComponent : IComponent
    {
        Guard.IsNotNull(parameters);

        await using var htmlRenderer = new HtmlRenderer(self.services, self.loggerFactory);

        var html = await htmlRenderer.Dispatcher.InvokeAsync(async () =>
        {
            var output = await htmlRenderer.RenderComponentAsync<TComponent>(parameters);
            return output.ToHtmlString();
        });

        return await self.ToPDFAsync(html, pdfOptions);
    }
}
