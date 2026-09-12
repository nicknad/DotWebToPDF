using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DotWebToPdf.Tests;

public sealed class DotWebToPdfGeneratorExtensionsTests
{
    [Fact]
    public void AddDotWebToPdfGenerator_Throws_WhenServicesAreNull()
    {
        Assert.Throws<ArgumentNullException>(() => DotWebToPdfGeneratorExtensions.AddDotWebToPdfGenerator(null!));
    }

    [Fact]
    public async Task AddDotWebToPdfGenerator_RegistersGeneratorAsSingleton()
    {
        var services = new ServiceCollection();
        services.AddDotWebToPdfGenerator();

        await using var provider = services.BuildServiceProvider();

        var first = provider.GetRequiredService<DotWebToPdfGenerator>();
        var second = provider.GetRequiredService<DotWebToPdfGenerator>();
        var abstraction = provider.GetRequiredService<IWebToPdfGenerator>();

        Assert.Same(first, second);
        Assert.Same(first, abstraction);
    }

    [Fact]
    public void AddDotWebToPdfGenerator_IsIdempotent()
    {
        var services = new ServiceCollection();
        services.AddDotWebToPdfGenerator();
        services.AddDotWebToPdfGenerator();

        Assert.Single(services, descriptor => descriptor.ServiceType == typeof(DotWebToPdfGenerator));
        Assert.Single(services, descriptor => descriptor.ServiceType == typeof(IWebToPdfGenerator));
    }
}
