using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotWebToPDF;

public class DotWebToPDFGenerator
{
    internal readonly IServiceProvider services;
    internal readonly ILoggerFactory loggerFactory;

    public DotWebToPDFGenerator(IServiceProvider services, ILoggerFactory loggerFactory)
    {
        this.services = services;
        this.loggerFactory = loggerFactory;
    }

}
