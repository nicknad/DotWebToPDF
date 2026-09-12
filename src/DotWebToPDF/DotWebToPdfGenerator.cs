using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;

namespace DotWebToPdf;

/// <summary>
/// Generates PDF documents from HTML strings or Blazor components by rendering them in a shared Chromium browser.
/// </summary>
public sealed partial class DotWebToPdfGenerator : IWebToPdfGenerator, IAsyncDisposable
{
    private readonly IServiceProvider _services;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger _logger;
    private readonly GeneratorOptions _options;
    private readonly object _disposeSync = new();

    [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", Justification = "The semaphore is intentionally not disposed so waiting renders can release it safely during shutdown.")]
    private readonly SemaphoreSlim _browserLock = new(1, 1);

    [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", Justification = "The semaphore is intentionally not disposed so waiting renders can release it safely during shutdown.")]
    private readonly SemaphoreSlim? _pageLimiter;

    private Task? _disposeTask;
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private volatile bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="DotWebToPdfGenerator"/> class.
    /// </summary>
    /// <param name="services">The service provider used to resolve dependencies while rendering components.</param>
    /// <param name="loggerFactory">The logger factory used to create loggers while rendering components.</param>
    /// <param name="options">The options that control the shared browser and rendering concurrency.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="services"/>, <paramref name="loggerFactory"/> or <paramref name="options"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException"><see cref="GeneratorOptions.DefaultTimeout"/> is negative.</exception>
    public DotWebToPdfGenerator(IServiceProvider services, ILoggerFactory loggerFactory, IOptions<GeneratorOptions> options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(loggerFactory);
        ArgumentNullException.ThrowIfNull(options);

        if (options.Value.DefaultTimeout < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "DefaultTimeout must not be negative.");
        }

        _services = services;
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<DotWebToPdfGenerator>();
        _options = options.Value;
        _pageLimiter = _options.MaxConcurrentPages > 0
            ? new SemaphoreSlim(_options.MaxConcurrentPages, _options.MaxConcurrentPages)
            : null;
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentOutOfRangeException"><see cref="RenderOptions.Timeout"/> is negative.</exception>
    public async Task<byte[]> ToPdfAsync(
        string html,
        PdfOptions? options = null,
        RenderOptions? renderOptions = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(html);
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (renderOptions?.Timeout < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(renderOptions), "Timeout must not be negative.");
        }

        var document = HtmlBaseUrlInjector.Inject(html, renderOptions?.BaseUrl);
        var browser = await GetBrowserAsync(cancellationToken).ConfigureAwait(false);

        await AcquirePageSlotAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var page = await NewPageAsync(browser, cancellationToken).ConfigureAwait(false);
            await using (page.ConfigureAwait(false))
            {
                return await RenderPageAsync(page, document, options, renderOptions, cancellationToken).ConfigureAwait(false);
            }
        }
        finally
        {
            _pageLimiter?.Release();
        }
    }

    /// <inheritdoc />
    public async Task<byte[]> ToPdfAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TComponent>(
        ParameterView parameters,
        PdfOptions? options = null,
        RenderOptions? renderOptions = null,
        CancellationToken cancellationToken = default)
        where TComponent : IComponent
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        cancellationToken.ThrowIfCancellationRequested();

        var htmlRenderer = new HtmlRenderer(_services, _loggerFactory);

        await using (htmlRenderer.ConfigureAwait(false))
        {
            var html = await htmlRenderer.Dispatcher.InvokeAsync(async () =>
            {
                var output = await htmlRenderer.RenderComponentAsync<TComponent>(parameters).ConfigureAwait(false);
                return output.ToHtmlString();
            }).ConfigureAwait(false);

            return await ToPdfAsync(html, options, renderOptions, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        lock (_disposeSync)
        {
            _disposeTask ??= DisposeCoreAsync();

            return new ValueTask(_disposeTask);
        }
    }

    private static async Task<IPage> NewPageAsync(IBrowser browser, CancellationToken cancellationToken)
    {
        try
        {
            return await browser.NewPageAsync().ConfigureAwait(false);
        }
        catch (PlaywrightException exception) when (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException("PDF generation was canceled.", exception, cancellationToken);
        }
    }

    private static async Task ClosePageOnCancellationAsync(IPage page)
    {
        try
        {
            await page.CloseAsync().ConfigureAwait(false);
        }
        catch (PlaywrightException)
        {
        }
    }

    private async Task<byte[]> RenderPageAsync(
        IPage page,
        string document,
        PdfOptions? options,
        RenderOptions? renderOptions,
        CancellationToken cancellationToken)
    {
        var cancellationState = new PageCancellationState(page);
        using var registration = cancellationToken.Register(
            static state => ((PageCancellationState)state!).RequestClose(),
            cancellationState);

        var timeout = PlaywrightOptionsFactory.ToMilliseconds(renderOptions?.Timeout)
            ?? PlaywrightOptionsFactory.ToMilliseconds(_options.DefaultTimeout);
        if (timeout is { } milliseconds)
        {
            page.SetDefaultTimeout(milliseconds);
        }

        try
        {
            await page.SetContentAsync(document, new PageSetContentOptions
            {
                WaitUntil = PlaywrightOptionsFactory.ToWaitUntilState(renderOptions?.WaitUntil),
            }).ConfigureAwait(false);

            if (renderOptions?.WaitForSelector is { Length: > 0 } selector)
            {
                await page.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions
                {
                    State = PlaywrightOptionsFactory.ToWaitForSelectorState(renderOptions.WaitForSelectorState),
                }).ConfigureAwait(false);
            }

            return await page.PdfAsync(PlaywrightOptionsFactory.CreatePdfOptions(options)).ConfigureAwait(false);
        }
        catch (PlaywrightException exception) when (cancellationState.CloseRequested)
        {
            LogRenderCanceled(_logger);
            throw new OperationCanceledException("PDF generation was canceled.", exception, cancellationToken);
        }
        catch (TimeoutException exception) when (cancellationState.CloseRequested)
        {
            LogRenderCanceled(_logger);
            throw new OperationCanceledException("PDF generation was canceled.", exception, cancellationToken);
        }
    }

    private async Task DisposeCoreAsync()
    {
        _disposed = true;
        LogGeneratorDisposed(_logger);

        await _browserLock.WaitAsync().ConfigureAwait(false);
        try
        {
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

            if (_browser is not null)
            {
                await _browser.DisposeAsync().ConfigureAwait(false);
                _browser = null;
                LogDisconnectedBrowserDisposed(_logger);
            }

            _playwright ??= await Playwright.CreateAsync().ConfigureAwait(false);
            _browser = await _playwright.Chromium.LaunchAsync(PlaywrightOptionsFactory.CreateLaunchOptions(_options)).ConfigureAwait(false);
            LogBrowserLaunched(_logger);

            return _browser;
        }
        finally
        {
            _browserLock.Release();
        }
    }

    private async Task AcquirePageSlotAsync(CancellationToken cancellationToken)
    {
        if (_pageLimiter is not null)
        {
            await _pageLimiter.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Launched the shared Chromium browser.")]
    private static partial void LogBrowserLaunched(ILogger logger);

    [LoggerMessage(EventId = 2, Level = LogLevel.Warning, Message = "Disconnected Chromium browser disposed before relaunching.")]
    private static partial void LogDisconnectedBrowserDisposed(ILogger logger);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "PDF generation was canceled; the page was closed.")]
    private static partial void LogRenderCanceled(ILogger logger);

    [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "PDF generator disposed and the browser was shut down.")]
    private static partial void LogGeneratorDisposed(ILogger logger);

    private sealed class PageCancellationState(IPage page)
    {
        private volatile bool _closeRequested;

        internal bool CloseRequested => _closeRequested;

        internal void RequestClose()
        {
            _closeRequested = true;
            _ = ClosePageOnCancellationAsync(page);
        }
    }
}
