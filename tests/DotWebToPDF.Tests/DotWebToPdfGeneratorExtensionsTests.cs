using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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

    [Fact]
    public void AddDotWebToPdfGenerator_AppliesConfiguration()
    {
        var services = new ServiceCollection();
        services.AddDotWebToPdfGenerator(options =>
        {
            options.Channel = "chrome";
            options.MaxConcurrentPages = 1;
        });

        using var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<GeneratorOptions>>().Value;

        Assert.Equal("chrome", options.Channel);
        Assert.Equal(1, options.MaxConcurrentPages);
    }

    [Fact]
    public void AddDotWebToPdfGenerator_PreservesCustomGeneratorRegistration()
    {
        var services = new ServiceCollection();
        var custom = new StubGenerator();
        services.AddSingleton<IWebToPdfGenerator>(custom);
        services.AddDotWebToPdfGenerator();

        using var provider = services.BuildServiceProvider();

        Assert.Same(custom, provider.GetRequiredService<IWebToPdfGenerator>());
    }

    [Fact]
    public void AddDotWebToPdfGenerator_UsesDefaultOptions_WhenNotConfigured()
    {
        var services = new ServiceCollection();
        services.AddDotWebToPdfGenerator();

        using var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<GeneratorOptions>>().Value;

        Assert.Null(options.Channel);
        Assert.Equal(Environment.ProcessorCount, options.MaxConcurrentPages);
    }

    private sealed class StubGenerator : IWebToPdfGenerator
    {
        public Task<byte[]> ToPdfAsync(string html, PdfOptions? options = null, RenderOptions? renderOptions = null, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<byte[]> ToPdfAsync<TComponent>(ParameterView parameters, PdfOptions? options = null, RenderOptions? renderOptions = null, CancellationToken cancellationToken = default)
            where TComponent : IComponent
        {
            throw new NotSupportedException();
        }
    }
}
