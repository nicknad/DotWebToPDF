namespace DotWebToPdf;

/// <summary>
/// The state an element must reach before rendering the PDF.
/// </summary>
public enum PageElementState
{
    /// <summary>
    /// Wait for the element to be present in the DOM.
    /// </summary>
    Attached,

    /// <summary>
    /// Wait for the element to be absent from the DOM.
    /// </summary>
    Detached,

    /// <summary>
    /// Wait for the element to be present in the DOM and visible.
    /// </summary>
    Visible,

    /// <summary>
    /// Wait for the element to be hidden or absent from the DOM.
    /// </summary>
    Hidden,
}
