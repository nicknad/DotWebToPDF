using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;

namespace DotWebToPdf;

/// <summary>
/// Generates PDF documents from HTML strings or Blazor components.
/// </summary>
public interface IWebToPdfGenerator
{
    /// <summary>
    /// Renders the specified HTML document as a PDF.
    /// </summary>
    /// <param name="html">The HTML document to render.</param>
    /// <param name="options">The options that control how the PDF is rendered.</param>
    /// <param name="renderOptions">The options that control how the HTML is loaded before rendering.</param>
    /// <param name="cancellationToken">
    /// A token to cancel the operation. Cancellation closes the page that is being rendered.
    /// </param>
    /// <returns>The rendered PDF document as a byte array.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="html"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><see cref="RenderOptions.Timeout"/> is negative.</exception>
    /// <exception cref="ObjectDisposedException">The generator has been disposed.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
    /// <exception cref="Microsoft.Playwright.PlaywrightException">The browser failed to load the HTML or render the PDF.</exception>
    Task<byte[]> ToPdfAsync(
        string html,
        PdfOptions? options = null,
        RenderOptions? renderOptions = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Renders the specified Blazor component as a PDF.
    /// Components are rendered statically, so JavaScript interop and interactive features are not available.
    /// </summary>
    /// <typeparam name="TComponent">The type of the component to render.</typeparam>
    /// <param name="parameters">The parameters to pass to the component.</param>
    /// <param name="options">The options that control how the PDF is rendered.</param>
    /// <param name="renderOptions">The options that control how the HTML is loaded before rendering.</param>
    /// <param name="cancellationToken">
    /// A token to cancel the operation. Cancellation closes the page that is being rendered.
    /// </param>
    /// <returns>The rendered PDF document as a byte array.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><see cref="RenderOptions.Timeout"/> is negative.</exception>
    /// <exception cref="ObjectDisposedException">The generator has been disposed.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
    /// <exception cref="Microsoft.Playwright.PlaywrightException">The browser failed to load the HTML or render the PDF.</exception>
    Task<byte[]> ToPdfAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TComponent>(
        ParameterView parameters,
        PdfOptions? options = null,
        RenderOptions? renderOptions = null,
        CancellationToken cancellationToken = default)
        where TComponent : IComponent;
}
