using Microsoft.Extensions.DependencyInjection;

namespace DotWebToPDF;

public static class DotWebToPDFGeneratorExtension
{
    public static IServiceCollection AddDotWebToPDFGenerator(this IServiceCollection services)
    {
        services.AddScoped<DotWebToPDFGenerator>();

        return services;
    }
}
