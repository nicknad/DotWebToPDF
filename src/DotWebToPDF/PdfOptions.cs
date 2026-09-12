namespace DotWebToPdf;

/// <summary>
/// Options that control how a PDF document is rendered.
/// </summary>
public sealed record PdfOptions
{
    /// <summary>
    /// Gets the paper format, such as <c>A4</c>, <c>Letter</c> or <c>Tabloid</c>.
    /// Defaults to <c>Letter</c> when neither <see cref="Format"/> nor <see cref="Width"/> and <see cref="Height"/> are set.
    /// Takes precedence over <see cref="Width"/> and <see cref="Height"/>.
    /// </summary>
    public string? Format { get; init; }

    /// <summary>
    /// Gets a value indicating whether to print the background graphics of the page. Defaults to <see langword="false"/>.
    /// </summary>
    public bool? PrintBackground { get; init; }

    /// <summary>
    /// Gets a value indicating whether the paper is in landscape orientation. Defaults to <see langword="false"/>.
    /// </summary>
    public bool? Landscape { get; init; }

    /// <summary>
    /// Gets the scale of the rendered page. Must be between <c>0.1</c> and <c>2</c>. Defaults to <c>1</c>.
    /// </summary>
    public double? Scale { get; init; }

    /// <summary>
    /// Gets the page width in CSS units, such as <c>8.5in</c>. Takes precedence over <see cref="Format"/>.
    /// </summary>
    public string? Width { get; init; }

    /// <summary>
    /// Gets the page height in CSS units, such as <c>11in</c>. Takes precedence over <see cref="Format"/>.
    /// </summary>
    public string? Height { get; init; }

    /// <summary>
    /// Gets the page margins.
    /// </summary>
    public PdfMargin? Margin { get; init; }

    /// <summary>
    /// Gets the page ranges to print, such as <c>1-5, 8, 11-13</c>. Prints all pages when not set.
    /// </summary>
    public string? PageRanges { get; init; }

    /// <summary>
    /// Gets a value indicating whether to display the header and footer. Defaults to <see langword="false"/>.
    /// </summary>
    public bool? DisplayHeaderFooter { get; init; }

    /// <summary>
    /// Gets the HTML template for the print header. Only used when <see cref="DisplayHeaderFooter"/> is <see langword="true"/>.
    /// </summary>
    public string? HeaderTemplate { get; init; }

    /// <summary>
    /// Gets the HTML template for the print footer. Only used when <see cref="DisplayHeaderFooter"/> is <see langword="true"/>.
    /// </summary>
    public string? FooterTemplate { get; init; }

    /// <summary>
    /// Gets a value indicating whether the page size defined by the page CSS takes precedence. Defaults to <see langword="false"/>.
    /// </summary>
    public bool? PreferCssPageSize { get; init; }

    /// <summary>
    /// Gets a value indicating whether to generate an outline based on the page headings. Defaults to <see langword="false"/>.
    /// </summary>
    public bool? Outline { get; init; }

    /// <summary>
    /// Gets a value indicating whether to generate a tagged (accessible) PDF. Defaults to <see langword="false"/>.
    /// </summary>
    public bool? Tagged { get; init; }
}
