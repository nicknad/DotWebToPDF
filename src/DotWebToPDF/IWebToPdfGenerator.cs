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
    /// <param name="cancellationToken">A token to cancel waiting for the shared browser to become available.</param>
    /// <returns>The rendered PDF document as a byte array.</returns>
    Task<byte[]> ToPdfAsync(string html, PdfOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Renders the specified Blazor component as a PDF.
    /// </summary>
    /// <typeparam name="TComponent">The type of the component to render.</typeparam>
    /// <param name="parameters">The parameters to pass to the component.</param>
    /// <param name="options">The options that control how the PDF is rendered.</param>
    /// <param name="cancellationToken">A token to cancel waiting for the shared browser to become available.</param>
    /// <returns>The rendered PDF document as a byte array.</returns>
    Task<byte[]> ToPdfAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TComponent>(
        ParameterView parameters,
        PdfOptions? options = null,
        CancellationToken cancellationToken = default)
        where TComponent : IComponent;
}
