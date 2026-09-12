using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace DotWebToPdf;

/// <summary>
/// Generates PDF documents from HTML strings or Blazor components by rendering them in a shared headless Chromium browser.
/// </summary>
public sealed class DotWebToPdfGenerator : IWebToPdfGenerator, IAsyncDisposable
{
    private readonly IServiceProvider _services;
    private readonly ILoggerFactory _loggerFactory;
    private readonly SemaphoreSlim _browserLock = new(1, 1);

    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="DotWebToPdfGenerator"/> class.
    /// </summary>
    /// <param name="services">The service provider used to resolve dependencies while rendering components.</param>
    /// <param name="loggerFactory">The logger factory used to create loggers while rendering components.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="services"/> or <paramref name="loggerFactory"/> is <see langword="null"/>.
    /// </exception>
    public DotWebToPdfGenerator(IServiceProvider services, ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        _services = services;
        _loggerFactory = loggerFactory;
    }

    /// <inheritdoc />
    public async Task<byte[]> ToPdfAsync(string html, PdfOptions? options = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(html);
        ObjectDisposedException.ThrowIf(_disposed, this);

        var browser = await GetBrowserAsync(cancellationToken).ConfigureAwait(false);
        var page = await browser.NewPageAsync().ConfigureAwait(false);

        await using (page.ConfigureAwait(false))
        {
            await page.SetContentAsync(html).ConfigureAwait(false);

            return await page.PdfAsync(PagePdfOptionsFactory.Create(options)).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async Task<byte[]> ToPdfAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TComponent>(
        ParameterView parameters,
        PdfOptions? options = null,
        CancellationToken cancellationToken = default)
        where TComponent : IComponent
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var htmlRenderer = new HtmlRenderer(_services, _loggerFactory);

        await using (htmlRenderer.ConfigureAwait(false))
        {
            var html = await htmlRenderer.Dispatcher.InvokeAsync(async () =>
            {
                var output = await htmlRenderer.RenderComponentAsync<TComponent>(parameters).ConfigureAwait(false);
                return output.ToHtmlString();
            }).ConfigureAwait(false);

            return await ToPdfAsync(html, options, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        await _browserLock.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            if (_browser is not null)
            {
                await _browser.DisposeAsync().ConfigureAwait(false);
                _browser = null;
            }

            _playwright?.Dispose();
            _playwright = null;
        }
        finally
        {
            _browserLock.Release();
        }

        _browserLock.Dispose();
    }

    private async Task<IBrowser> GetBrowserAsync(CancellationToken cancellationToken)
    {
        if (_browser is { IsConnected: true } browser)
        {
            return browser;
        }

        await _browserLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (_browser is { IsConnected: true })
            {
                return _browser;
            }

            _playwright ??= await Playwright.CreateAsync().ConfigureAwait(false);
            _browser = await _playwright.Chromium.LaunchAsync().ConfigureAwait(false);

            return _browser;
        }
        finally
        {
            _browserLock.Release();
        }
    }
}
