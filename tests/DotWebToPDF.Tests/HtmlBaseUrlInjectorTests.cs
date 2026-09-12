using Xunit;

namespace DotWebToPdf.Tests;

public sealed class HtmlBaseUrlInjectorTests
{
    [Fact]
    public void Inject_ReturnsOriginalHtml_WhenBaseUrlIsMissing()
    {
        const string html = "<html><head></head><body></body></html>";

        Assert.Equal(html, HtmlBaseUrlInjector.Inject(html, null));
    }

    [Fact]
    public void Inject_ReturnsOriginalHtml_WhenBaseWithHrefAlreadyExists()
    {
        const string html = "<html><head><BASE href=\"https://existing.test/\"></head><body></body></html>";

        Assert.Equal(html, HtmlBaseUrlInjector.Inject(html, new Uri("https://new.test/")));
    }

    [Fact]
    public void Inject_InjectsBase_WhenExistingBaseHasNoHref()
    {
        var result = HtmlBaseUrlInjector.Inject(
            "<html><head><base target=\"_blank\"></head><body></body></html>",
            new Uri("https://base.test/"));

        Assert.Equal(
            "<html><head><base href=\"https://base.test/\" /><base target=\"_blank\"></head><body></body></html>",
            result);
    }

    [Fact]
    public void Inject_InsertsAfterHeadOpeningTag()
    {
        var result = HtmlBaseUrlInjector.Inject(
            "<html><head lang=\"en\"><title>x</title></head><body></body></html>",
            new Uri("https://base.test/"));

        Assert.Equal(
            "<html><head lang=\"en\"><base href=\"https://base.test/\" /><title>x</title></head><body></body></html>",
            result);
    }

    [Fact]
    public void Inject_InsertsAfterHeadTagContainingQuotedClosingBracket()
    {
        var result = HtmlBaseUrlInjector.Inject(
            "<html><head data-x=\"a>b\"><title>x</title></head><body></body></html>",
            new Uri("https://base.test/"));

        Assert.Equal(
            "<html><head data-x=\"a>b\"><base href=\"https://base.test/\" /><title>x</title></head><body></body></html>",
            result);
    }

    [Fact]
    public void Inject_IgnoresHeadTagInsideScript()
    {
        const string html = "<html><body><script>const template = \"<head>\";</script></body></html>";

        var result = HtmlBaseUrlInjector.Inject(html, new Uri("https://base.test/"));

        Assert.Equal(
            "<html><head><base href=\"https://base.test/\" /></head><body><script>const template = \"<head>\";</script></body></html>",
            result);
    }

    [Fact]
    public void Inject_IgnoresHeadTagInsideComment()
    {
        const string html = "<html><!-- <head> --><body></body></html>";

        var result = HtmlBaseUrlInjector.Inject(html, new Uri("https://base.test/"));

        Assert.Equal(
            "<html><head><base href=\"https://base.test/\" /></head><!-- <head> --><body></body></html>",
            result);
    }

    [Fact]
    public void Inject_IgnoresHeaderElement()
    {
        const string html = "<html><header>Title</header><body></body></html>";

        var result = HtmlBaseUrlInjector.Inject(html, new Uri("https://base.test/"));

        Assert.Equal(
            "<html><head><base href=\"https://base.test/\" /></head><header>Title</header><body></body></html>",
            result);
    }

    [Fact]
    public void Inject_IgnoresBaseSubstringInText()
    {
        const string html = "<html><head></head><body><basefont size=\"4\">text</body></html>";

        var result = HtmlBaseUrlInjector.Inject(html, new Uri("https://base.test/"));

        Assert.Equal(
            "<html><head><base href=\"https://base.test/\" /></head><body><basefont size=\"4\">text</body></html>",
            result);
    }

    [Fact]
    public void Inject_AddsHead_WhenOnlyHtmlElementExists()
    {
        var result = HtmlBaseUrlInjector.Inject("<html><body></body></html>", new Uri("https://base.test/"));

        Assert.Equal(
            "<html><head><base href=\"https://base.test/\" /></head><body></body></html>",
            result);
    }

    [Fact]
    public void Inject_InsertsAfterDoctype_WhenNoHtmlOrHeadElementExists()
    {
        var result = HtmlBaseUrlInjector.Inject(
            "<!DOCTYPE html><body>Hello</body>",
            new Uri("https://base.test/"));

        Assert.Equal(
            "<!DOCTYPE html><base href=\"https://base.test/\" /><body>Hello</body>",
            result);
    }

    [Fact]
    public void Inject_PrependsBase_WhenNoDoctypeHtmlOrHeadElementExists()
    {
        var result = HtmlBaseUrlInjector.Inject("<p>fragment</p>", new Uri("https://base.test/"));

        Assert.Equal("<base href=\"https://base.test/\" /><p>fragment</p>", result);
    }

    [Fact]
    public void Inject_HtmlEncodesBaseUrl()
    {
        var result = HtmlBaseUrlInjector.Inject("<head></head>", new Uri("https://base.test/?a=1&b=\"2\""));

        Assert.Equal(
            "<head><base href=\"https://base.test/?a=1&amp;b=&quot;2&quot;\" /></head>",
            result);
    }
}
