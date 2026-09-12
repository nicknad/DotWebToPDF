using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace DotWebToPdf;

internal static partial class HtmlBaseUrlInjector
{
    internal static string Inject(string html, Uri? baseUrl)
    {
        if (baseUrl is null)
        {
            return html;
        }

        var masked = MaskNonMarkupContent(html);
        if (BaseWithHrefRegex().IsMatch(masked))
        {
            return html;
        }

        var element = $"<base href=\"{WebUtility.HtmlEncode(baseUrl.OriginalString)}\" />";

        var headMatch = HeadTagRegex().Match(masked);
        if (headMatch.Success)
        {
            return html.Insert(headMatch.Index + headMatch.Length, element);
        }

        var htmlMatch = HtmlTagRegex().Match(masked);
        if (htmlMatch.Success)
        {
            return html.Insert(htmlMatch.Index + htmlMatch.Length, $"<head>{element}</head>");
        }

        var doctypeMatch = DoctypeRegex().Match(masked);
        if (doctypeMatch.Success)
        {
            return html.Insert(doctypeMatch.Index + doctypeMatch.Length, element);
        }

        return element + html;
    }

    private static string MaskNonMarkupContent(string html)
    {
        var buffer = new StringBuilder(html);
        MaskRanges(buffer, "<!--", "-->");
        MaskRanges(buffer, "<script", "</script");
        MaskRanges(buffer, "<style", "</style");

        return buffer.ToString();
    }

    private static void MaskRanges(StringBuilder buffer, string open, string close)
    {
        var text = buffer.ToString();
        var index = 0;

        while (index < text.Length)
        {
            var openIndex = text.IndexOf(open, index, StringComparison.OrdinalIgnoreCase);
            if (openIndex < 0)
            {
                return;
            }

            var closeIndex = text.IndexOf(close, openIndex + open.Length, StringComparison.OrdinalIgnoreCase);
            if (closeIndex < 0)
            {
                return;
            }

            var end = closeIndex + close.Length;
            for (var position = openIndex; position < end; position++)
            {
                buffer[position] = ' ';
            }

            index = end;
        }
    }

    [GeneratedRegex(@"<base\b[^>]*?(?<![\w-])href\s*=", RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 1000)]
    private static partial Regex BaseWithHrefRegex();

    [GeneratedRegex("<head\\b(?:\"[^\"]*\"|'[^']*'|[^>\"'])*>", RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 1000)]
    private static partial Regex HeadTagRegex();

    [GeneratedRegex("<html\\b(?:\"[^\"]*\"|'[^']*'|[^>\"'])*>", RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 1000)]
    private static partial Regex HtmlTagRegex();

    [GeneratedRegex("<!doctype[^>]*>", RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 1000)]
    private static partial Regex DoctypeRegex();
}
