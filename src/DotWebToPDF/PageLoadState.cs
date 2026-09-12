namespace DotWebToPdf;

/// <summary>
/// The page lifecycle event to wait for before rendering the PDF.
/// </summary>
public enum PageLoadState
{
    /// <summary>
    /// Wait for the <c>load</c> event.
    /// </summary>
    Load,

    /// <summary>
    /// Wait for the <c>DOMContentLoaded</c> event.
    /// </summary>
    DOMContentLoaded,

    /// <summary>
    /// Wait until there have been no network connections for at least 500 milliseconds.
    /// </summary>
    NetworkIdle,

    /// <summary>
    /// Wait for the network response to be received and the document to start loading.
    /// </summary>
    Commit,
}
