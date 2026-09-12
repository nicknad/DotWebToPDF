using Xunit;

namespace DotWebToPdf.Tests;

public sealed class PagePdfOptionsFactoryTests
{
    [Fact]
    public void Create_ReturnsNull_WhenOptionsAreNull()
    {
        Assert.Null(PagePdfOptionsFactory.Create(null));
    }

    [Fact]
    public void Create_MapsAllOptions()
    {
        var options = new PdfOptions
        {
            DisplayHeaderFooter = true,
            FooterTemplate = "<footer/>",
            Format = "A4",
            HeaderTemplate = "<header/>",
            Height = "11in",
            Landscape = true,
            Margin = new PdfMargin
            {
                Top = "1cm",
                Right = "2cm",
                Bottom = "3cm",
                Left = "4cm",
            },
            Outline = true,
            PageRanges = "1-3",
            PreferCssPageSize = true,
            PrintBackground = true,
            Scale = 1.5,
            Tagged = true,
            Width = "8.5in",
        };

        var result = PagePdfOptionsFactory.Create(options);

        Assert.NotNull(result);
        Assert.True(result.DisplayHeaderFooter);
        Assert.Equal("<footer/>", result.FooterTemplate);
        Assert.Equal("A4", result.Format);
        Assert.Equal("<header/>", result.HeaderTemplate);
        Assert.Equal("11in", result.Height);
        Assert.True(result.Landscape);
        Assert.NotNull(result.Margin);
        Assert.Equal("1cm", result.Margin.Top);
        Assert.Equal("2cm", result.Margin.Right);
        Assert.Equal("3cm", result.Margin.Bottom);
        Assert.Equal("4cm", result.Margin.Left);
        Assert.True(result.Outline);
        Assert.Equal("1-3", result.PageRanges);
        Assert.True(result.PreferCSSPageSize);
        Assert.True(result.PrintBackground);
        Assert.Equal(1.5f, result.Scale);
        Assert.True(result.Tagged);
        Assert.Equal("8.5in", result.Width);
    }

    [Fact]
    public void Create_MapsDefaults()
    {
        var result = PagePdfOptionsFactory.Create(new PdfOptions());

        Assert.NotNull(result);
        Assert.Null(result.DisplayHeaderFooter);
        Assert.Null(result.FooterTemplate);
        Assert.Null(result.Format);
        Assert.Null(result.HeaderTemplate);
        Assert.Null(result.Height);
        Assert.Null(result.Landscape);
        Assert.Null(result.Margin);
        Assert.Null(result.Outline);
        Assert.Null(result.PageRanges);
        Assert.Null(result.PreferCSSPageSize);
        Assert.Null(result.PrintBackground);
        Assert.Null(result.Scale);
        Assert.Null(result.Tagged);
        Assert.Null(result.Width);
    }
}
