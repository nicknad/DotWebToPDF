# DotWebToPDF

Generate PDF documents from HTML strings or Blazor components using [Microsoft Playwright](https://playwright.dev/dotnet/).

## Features

- Render raw HTML or Blazor components to PDF
- One shared Chromium browser, launched lazily and reused across calls
- Playwright-independent options: `PdfOptions`, `RenderOptions` and `GeneratorOptions`
- Configurable browser launch (channel, executable, args, proxy, headless)
- Wait for load state or a CSS selector before rendering; resolve relative URLs with a base URL
- Cooperative cancellation, per-call timeouts and a configurable concurrency limit
- Dependency injection friendly: singleton `IWebToPdfGenerator` and `DotWebToPdfGenerator`

## Installation

```shell
dotnet add package DotWebToPDF
```

Playwright needs browser binaries. Download Chromium once after restoring the package:

```shell
pwsh bin/Debug/net8.0/playwright.ps1 install chromium
```

On Linux use `--with-deps` to install the required system libraries:

```shell
pwsh bin/Debug/net8.0/playwright.ps1 install --with-deps chromium
```

## Usage

### Register the generator

```csharp
// Register with the default options
services.AddDotWebToPdfGenerator();

// Or register and configure the shared browser and concurrency
services.AddDotWebToPdfGenerator(options =>
{
    options.Channel = "chrome";
    options.DefaultTimeout = TimeSpan.FromSeconds(30);
    options.MaxConcurrentPages = 4;
});
```

The generator is registered as a singleton. The shared Chromium browser is launched on the first call and
disposed together with the service provider (call `DisposeAsync`/`await using` on the provider). Options are
read once when the singleton is created.

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

Components are rendered statically: JavaScript interop and interactive features are not available.

### PDF options

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

### Render options

```csharp
byte[] pdf = await generator.ToPdfAsync(html, renderOptions: new RenderOptions
{
    WaitUntil = PageLoadState.NetworkIdle,
    WaitForSelector = "#chart",
    WaitForSelectorState = PageElementState.Visible,
    BaseUrl = new Uri("https://app.example.com/"),
    Timeout = TimeSpan.FromSeconds(20),
});
```

`BaseUrl` injects a `<base href>` element (unless the HTML already contains one) so relative images,
stylesheets and fonts resolve correctly. `Timeout` overrides `GeneratorOptions.DefaultTimeout` for a
single call and covers loading the HTML and waiting for the selector.

### Cancellation and concurrency

- Passing a cancelled `CancellationToken` closes the page that is being rendered and throws
  `OperationCanceledException`. Playwright itself is not cancellable, so the page is closed cooperatively.
- Timeouts bound HTML loading and selector waits only. PDF generation itself has no timeout and can only
  be stopped through cancellation.
- `GeneratorOptions.MaxConcurrentPages` (default: `Environment.ProcessorCount`) limits how many renders
  can run at the same time. Set it to `0` or less to disable the limit.
- Every call uses its own page and browser context, so cookies and storage never leak between renders.

## Building

The library targets `net8.0` and is built with the .NET 10 SDK. Static analysis runs as part of the build:
.NET analyzers at `latest-all`, [Meziantou.Analyzer](https://github.com/meziantou/Meziantou.Analyzer) and
[Roslynator](https://github.com/dotnet/roslynator), with warnings treated as errors.

```shell
dotnet build
dotnet test
dotnet format --verify-no-changes
```

Unit tests run everywhere. Integration tests exercise a real Chromium browser and are skipped unless set up:

```shell
pwsh tests/DotWebToPDF.Tests/bin/Debug/net8.0/playwright.ps1 install chromium
$env:DOTWEBTOPDF_RUN_INTEGRATION = "1"
dotnet test --filter "Category=Integration"
```

## License

MIT. See [LICENSE](LICENSE).
