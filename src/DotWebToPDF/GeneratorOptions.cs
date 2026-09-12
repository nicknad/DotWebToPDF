namespace DotWebToPdf;

/// <summary>
/// Options that control the shared browser and how rendering requests are scheduled.
/// </summary>
public sealed class GeneratorOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to run the browser in headless mode. Defaults to <see langword="true"/>.
    /// </summary>
    public bool? Headless { get; set; }

    /// <summary>
    /// Gets or sets the browser channel to use, such as <c>chrome</c>, <c>msedge</c> or <c>chromium</c>.
    /// </summary>
    public string? Channel { get; set; }

    /// <summary>
    /// Gets or sets the path to a browser executable to run instead of the bundled Chromium.
    /// </summary>
    public string? ExecutablePath { get; set; }

    /// <summary>
    /// Gets or sets additional command-line arguments passed to the browser.
    /// </summary>
    public IReadOnlyList<string>? Args { get; set; }

    /// <summary>
    /// Gets or sets the proxy settings used when launching the browser.
    /// </summary>
    public ProxyOptions? Proxy { get; set; }

    /// <summary>
    /// Gets or sets the default timeout applied to HTML loading and selector waits.
    /// Defaults to the browser default when not set. PDF generation itself is not bounded by this timeout.
    /// </summary>
    public TimeSpan? DefaultTimeout { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of PDF renders that can run concurrently. Defaults to
    /// <see cref="Environment.ProcessorCount"/>. A value of zero or less disables the limit.
    /// </summary>
    public int MaxConcurrentPages { get; set; } = Environment.ProcessorCount;
}
