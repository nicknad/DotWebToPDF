using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace DotWebToPdf.Tests;

public sealed class DotWebToPdfGeneratorTests
{
    [Fact]
    public void Constructor_Throws_WhenServicesAreNull()
    {
        Assert.Throws<ArgumentNullException>(() => new DotWebToPdfGenerator(null!, NullLoggerFactory.Instance));
    }

    [Fact]
    public void Constructor_Throws_WhenLoggerFactoryIsNull()
    {
        using var provider = new ServiceCollection().BuildServiceProvider();

        Assert.Throws<ArgumentNullException>(() => new DotWebToPdfGenerator(provider, null!));
    }

    [Fact]
    public async Task ToPdfAsync_Throws_WhenHtmlIsNull()
    {
        using var provider = new ServiceCollection().BuildServiceProvider();
        await using var generator = new DotWebToPdfGenerator(provider, NullLoggerFactory.Instance);

        await Assert.ThrowsAsync<ArgumentNullException>(() => generator.ToPdfAsync(null!));
    }

    [Fact]
    public async Task ToPdfAsync_Throws_WhenDisposed()
    {
        using var provider = new ServiceCollection().BuildServiceProvider();
        var generator = new DotWebToPdfGenerator(provider, NullLoggerFactory.Instance);

        await generator.DisposeAsync();

        await Assert.ThrowsAsync<ObjectDisposedException>(() => generator.ToPdfAsync("<html></html>"));
    }

    [Fact]
    public async Task DisposeAsync_IsIdempotent()
    {
        using var provider = new ServiceCollection().BuildServiceProvider();
        var generator = new DotWebToPdfGenerator(provider, NullLoggerFactory.Instance);

        await generator.DisposeAsync();
        await generator.DisposeAsync();
    }
}
