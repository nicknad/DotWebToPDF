using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotWebToPDF;

public class PDFGenerator
{
    internal readonly IServiceProvider services;
    internal readonly ILoggerFactory loggerFactory;

    public PDFGenerator(IServiceProvider services, ILoggerFactory loggerFactory)
    {
        this.services = services;
        this.loggerFactory = loggerFactory;
    }

}
