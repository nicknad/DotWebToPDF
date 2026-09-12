using Microsoft.Playwright;

namespace DotWebToPdf;

internal static class PagePdfOptionsFactory
{
    internal static PagePdfOptions? Create(PdfOptions? options)
    {
        if (options is null)
        {
            return null;
        }

        return new PagePdfOptions
        {
            DisplayHeaderFooter = options.DisplayHeaderFooter,
            FooterTemplate = options.FooterTemplate,
            Format = options.Format,
            HeaderTemplate = options.HeaderTemplate,
            Height = options.Height,
            Landscape = options.Landscape,
            Margin = options.Margin is null ? null : new Margin
            {
                Top = options.Margin.Top,
                Right = options.Margin.Right,
                Bottom = options.Margin.Bottom,
                Left = options.Margin.Left,
            },
            Outline = options.Outline,
            PageRanges = options.PageRanges,
            PreferCSSPageSize = options.PreferCssPageSize,
            PrintBackground = options.PrintBackground,
            Scale = options.Scale is { } scale ? (float)scale : null,
            Tagged = options.Tagged,
            Width = options.Width,
        };
    }
}
