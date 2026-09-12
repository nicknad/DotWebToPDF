namespace DotWebToPdf;

/// <summary>
/// The margins of a PDF page.
/// </summary>
public sealed record PdfMargin
{
    /// <summary>
    /// Gets the top margin in CSS units, such as <c>1in</c> or <c>2cm</c>.
    /// </summary>
    public string? Top { get; init; }

    /// <summary>
    /// Gets the right margin in CSS units, such as <c>1in</c> or <c>2cm</c>.
    /// </summary>
    public string? Right { get; init; }

    /// <summary>
    /// Gets the bottom margin in CSS units, such as <c>1in</c> or <c>2cm</c>.
    /// </summary>
    public string? Bottom { get; init; }

    /// <summary>
    /// Gets the left margin in CSS units, such as <c>1in</c> or <c>2cm</c>.
    /// </summary>
    public string? Left { get; init; }
}
