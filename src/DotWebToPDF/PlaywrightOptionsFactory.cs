using Microsoft.Playwright;

namespace DotWebToPdf;

internal static class PlaywrightOptionsFactory
{
    internal static PagePdfOptions? CreatePdfOptions(PdfOptions? options)
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

    internal static BrowserTypeLaunchOptions? CreateLaunchOptions(GeneratorOptions options)
    {
        if (options.Headless is null
            && options.Channel is null
            && options.ExecutablePath is null
            && options.Args is null
            && options.Proxy is null)
        {
            return null;
        }

        return new BrowserTypeLaunchOptions
        {
            Headless = options.Headless,
            Channel = options.Channel,
            ExecutablePath = options.ExecutablePath,
            Args = options.Args,
            Proxy = options.Proxy is null ? null : new Proxy
            {
                Server = options.Proxy.Server,
                Bypass = options.Proxy.Bypass,
                Username = options.Proxy.Username,
                Password = options.Proxy.Password,
            },
        };
    }

    internal static float? ToMilliseconds(TimeSpan? timeout)
    {
        return timeout is { } value ? (float)value.TotalMilliseconds : null;
    }

    internal static WaitUntilState? ToWaitUntilState(PageLoadState? state)
    {
        return state switch
        {
            PageLoadState.Load => WaitUntilState.Load,
            PageLoadState.DOMContentLoaded => WaitUntilState.DOMContentLoaded,
            PageLoadState.NetworkIdle => WaitUntilState.NetworkIdle,
            PageLoadState.Commit => WaitUntilState.Commit,
            _ => null,
        };
    }

    internal static WaitForSelectorState? ToWaitForSelectorState(PageElementState? state)
    {
        return state switch
        {
            PageElementState.Attached => WaitForSelectorState.Attached,
            PageElementState.Detached => WaitForSelectorState.Detached,
            PageElementState.Visible => WaitForSelectorState.Visible,
            PageElementState.Hidden => WaitForSelectorState.Hidden,
            _ => null,
        };
    }
}
