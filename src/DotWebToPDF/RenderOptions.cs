namespace DotWebToPdf;

/// <summary>
/// Options that control how the HTML is loaded before it is rendered as a PDF.
/// </summary>
public sealed record RenderOptions
{
    /// <summary>
    /// Gets the page lifecycle event to wait for after loading the HTML.
    /// Defaults to <see cref="PageLoadState.Load"/>.
    /// </summary>
    public PageLoadState? WaitUntil { get; init; }

    /// <summary>
    /// Gets the maximum time to wait for the HTML to load and for <see cref="WaitForSelector"/> to be satisfied.
    /// Overrides <see cref="GeneratorOptions.DefaultTimeout"/> for a single call.
    /// </summary>
    public TimeSpan? Timeout { get; init; }

    /// <summary>
    /// Gets a CSS selector to wait for before rendering the PDF.
    /// </summary>
    public string? WaitForSelector { get; init; }

    /// <summary>
    /// Gets the state <see cref="WaitForSelector"/> must reach.
    /// Defaults to <see cref="PageElementState.Visible"/>.
    /// </summary>
    public PageElementState? WaitForSelectorState { get; init; }

    /// <summary>
    /// Gets the base URL used to resolve relative URLs in the HTML.
    /// A <c>&lt;base&gt;</c> element is injected when the HTML does not already contain one.
    /// </summary>
    public Uri? BaseUrl { get; init; }
}
