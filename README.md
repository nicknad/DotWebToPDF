# DotWebToPDF

Generate PDF documents from HTML strings or Blazor components using [Microsoft Playwright](https://playwright.dev/dotnet/).

## Features

- Render raw HTML or Blazor components to PDF
- One shared headless Chromium browser, launched lazily and reused across calls
- Playwright-independent `PdfOptions` API with `CancellationToken` support
- Dependency injection friendly: singleton `IWebToPdfGenerator` and `DotWebToPdfGenerator`

## Installation

```shell
dotnet add package DotWebToPDF
```

Playwright needs browser binaries. Download Chromium once after restoring the package:

```shell
pwsh bin/Debug/net8.0/playwright.ps1 install chromium
```

## Usage

### Register the generator

```csharp
services.AddDotWebToPdfGenerator();

// or register a custom instance yourself
services.AddSingleton<IWebToPdfGenerator, MyGenerator>();
```

The generator is registered as a singleton. The shared Chromium browser is launched on the first call and
disposed together with the service provider (call `DisposeAsync`/`await using` on the provider).

### HTML to PDF

```csharp
var generator = services.GetRequiredService<IWebToPdfGenerator>();

byte[] pdf = await generator.ToPdfAsync("<html><body><h1>Hello</h1></body></html>");

await File.WriteAllBytesAsync("hello.pdf", pdf);
```

### Blazor component to PDF

```csharp
byte[] pdf = await generator.ToPdfAsync<Invoice>(
    ParameterView.FromDictionary(new Dictionary<string, object?>
    {
        [nameof(Invoice.Number)] = "INV-001",
    }));
```

### Options

```csharp
byte[] pdf = await generator.ToPdfAsync(html, new PdfOptions
{
    Format = "A4",
    PrintBackground = true,
    Scale = 0.9,
    Margin = new PdfMargin { Top = "1cm", Bottom = "1cm" },
    DisplayHeaderFooter = true,
    FooterTemplate = "<span style='font-size:9px'>Page <span class='pageNumber'></span></span>",
});
```

## Building

The library targets `net8.0` and is built with the .NET 10 SDK. Static analysis runs as part of the build:
.NET analyzers at `latest-all`, [Meziantou.Analyzer](https://github.com/meziantou/Meziantou.Analyzer) and
[Roslynator](https://github.com/dotnet/roslynator), with warnings treated as errors.

```shell
dotnet build
dotnet test
dotnet format --verify-no-changes
```

## License

MIT. See [LICENSE](LICENSE).
