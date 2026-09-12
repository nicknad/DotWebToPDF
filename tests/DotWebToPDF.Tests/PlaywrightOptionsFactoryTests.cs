using Microsoft.Playwright;
using Xunit;

namespace DotWebToPdf.Tests;

public sealed class PlaywrightOptionsFactoryTests
{
    [Fact]
    public void CreatePdfOptions_ReturnsNull_WhenOptionsAreNull()
    {
        Assert.Null(PlaywrightOptionsFactory.CreatePdfOptions(null));
    }

    [Fact]
    public void CreatePdfOptions_MapsAllOptions()
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

        var result = PlaywrightOptionsFactory.CreatePdfOptions(options);

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
    public void CreatePdfOptions_MapsDefaults()
    {
        var result = PlaywrightOptionsFactory.CreatePdfOptions(new PdfOptions());

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

    [Fact]
    public void CreateLaunchOptions_ReturnsNull_WhenNoLaunchSettingsAreSet()
    {
        Assert.Null(PlaywrightOptionsFactory.CreateLaunchOptions(new GeneratorOptions()));
    }

    [Fact]
    public void CreateLaunchOptions_MapsAllOptions()
    {
        var options = new GeneratorOptions
        {
            Headless = false,
            Channel = "chrome",
            ExecutablePath = "C:/browser/chrome.exe",
            Args = ["--disable-gpu"],
            Proxy = new ProxyOptions
            {
                Server = "http://proxy.test:3128",
                Bypass = ".internal",
                Username = "user",
                Password = "secret",
            },
        };

        var result = PlaywrightOptionsFactory.CreateLaunchOptions(options);

        Assert.NotNull(result);
        Assert.False(result.Headless);
        Assert.Equal("chrome", result.Channel);
        Assert.Equal("C:/browser/chrome.exe", result.ExecutablePath);
        Assert.Equal(["--disable-gpu"], result.Args, StringComparer.Ordinal);
        Assert.NotNull(result.Proxy);
        Assert.Equal("http://proxy.test:3128", result.Proxy.Server);
        Assert.Equal(".internal", result.Proxy.Bypass);
        Assert.Equal("user", result.Proxy.Username);
        Assert.Equal("secret", result.Proxy.Password);
    }

    [Fact]
    public void CreateLaunchOptions_ReturnsOptions_WhenOnlyOneSettingIsSet()
    {
        var result = PlaywrightOptionsFactory.CreateLaunchOptions(new GeneratorOptions { Headless = true });

        Assert.NotNull(result);
        Assert.True(result.Headless);
        Assert.Null(result.Channel);
        Assert.Null(result.Proxy);
    }

    [Fact]
    public void ToMilliseconds_ReturnsNull_WhenTimeoutIsNull()
    {
        Assert.Null(PlaywrightOptionsFactory.ToMilliseconds(null));
    }

    [Fact]
    public void ToMilliseconds_ConvertsTimeSpan()
    {
        Assert.Equal(1500f, PlaywrightOptionsFactory.ToMilliseconds(TimeSpan.FromMilliseconds(1500)));
    }

    [Theory]
    [InlineData(PageLoadState.Load, WaitUntilState.Load)]
    [InlineData(PageLoadState.DOMContentLoaded, WaitUntilState.DOMContentLoaded)]
    [InlineData(PageLoadState.NetworkIdle, WaitUntilState.NetworkIdle)]
    [InlineData(PageLoadState.Commit, WaitUntilState.Commit)]
    public void ToWaitUntilState_MapsState(PageLoadState state, WaitUntilState expected)
    {
        Assert.Equal(expected, PlaywrightOptionsFactory.ToWaitUntilState(state));
    }

    [Fact]
    public void ToWaitUntilState_ReturnsNull_WhenStateIsNull()
    {
        Assert.Null(PlaywrightOptionsFactory.ToWaitUntilState(null));
    }

    [Theory]
    [InlineData(PageElementState.Attached, WaitForSelectorState.Attached)]
    [InlineData(PageElementState.Detached, WaitForSelectorState.Detached)]
    [InlineData(PageElementState.Visible, WaitForSelectorState.Visible)]
    [InlineData(PageElementState.Hidden, WaitForSelectorState.Hidden)]
    public void ToWaitForSelectorState_MapsState(PageElementState state, WaitForSelectorState expected)
    {
        Assert.Equal(expected, PlaywrightOptionsFactory.ToWaitForSelectorState(state));
    }

    [Fact]
    public void ToWaitForSelectorState_ReturnsNull_WhenStateIsNull()
    {
        Assert.Null(PlaywrightOptionsFactory.ToWaitForSelectorState(null));
    }
}
