using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace DotWebToPdf.Tests;

public sealed class DotWebToPdfGeneratorTests
{
    [Fact]
    public void Constructor_Throws_WhenServicesAreNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => new DotWebToPdfGenerator(null!, NullLoggerFactory.Instance, Options.Create(new GeneratorOptions())));
    }

    [Fact]
    public void Constructor_Throws_WhenLoggerFactoryIsNull()
    {
        using var provider = new ServiceCollection().BuildServiceProvider();

        Assert.Throws<ArgumentNullException>(
            () => new DotWebToPdfGenerator(provider, null!, Options.Create(new GeneratorOptions())));
    }

    [Fact]
    public void Constructor_Throws_WhenOptionsAreNull()
    {
        using var provider = new ServiceCollection().BuildServiceProvider();

        Assert.Throws<ArgumentNullException>(
            () => new DotWebToPdfGenerator(provider, NullLoggerFactory.Instance, null!));
    }

    [Fact]
    public void Constructor_Throws_WhenDefaultTimeoutIsNegative()
    {
        using var provider = new ServiceCollection().BuildServiceProvider();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateGenerator(provider, new GeneratorOptions { DefaultTimeout = TimeSpan.FromSeconds(-1) }));
    }

    [Fact]
    public async Task ToPdfAsync_Throws_WhenHtmlIsNull()
    {
        using var provider = new ServiceCollection().BuildServiceProvider();
        await using var generator = CreateGenerator(provider);

        await Assert.ThrowsAsync<ArgumentNullException>(() => generator.ToPdfAsync(null!));
    }

    [Fact]
    public async Task ToPdfAsync_Throws_WhenDisposed()
    {
        using var provider = new ServiceCollection().BuildServiceProvider();
        var generator = CreateGenerator(provider);

        await generator.DisposeAsync();

        await Assert.ThrowsAsync<ObjectDisposedException>(() => generator.ToPdfAsync("<html></html>"));
    }

    [Fact]
    public async Task ToPdfAsync_Throws_WhenCancelled()
    {
        using var provider = new ServiceCollection().BuildServiceProvider();
        await using var generator = CreateGenerator(provider);
        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => generator.ToPdfAsync("<html></html>", cancellationToken: cancellationTokenSource.Token));
    }

    [Fact]
    public async Task ToPdfAsync_Throws_WhenRenderTimeoutIsNegative()
    {
        using var provider = new ServiceCollection().BuildServiceProvider();
        await using var generator = CreateGenerator(provider);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => generator.ToPdfAsync(
            "<html></html>",
            renderOptions: new RenderOptions { Timeout = TimeSpan.FromSeconds(-1) }));
    }

    [Fact]
    public async Task DisposeAsync_IsIdempotent()
    {
        using var provider = new ServiceCollection().BuildServiceProvider();
        var generator = CreateGenerator(provider);

        await generator.DisposeAsync();
        await generator.DisposeAsync();
    }

    private static DotWebToPdfGenerator CreateGenerator(ServiceProvider provider, GeneratorOptions? options = null)
    {
        return new DotWebToPdfGenerator(
            provider,
            NullLoggerFactory.Instance,
            Options.Create(options ?? new GeneratorOptions()));
    }
}
