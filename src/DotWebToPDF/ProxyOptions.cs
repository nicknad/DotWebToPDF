namespace DotWebToPdf;

/// <summary>
/// Proxy server settings used when launching the browser.
/// </summary>
public sealed class ProxyOptions
{
    /// <summary>
    /// Gets or sets the proxy server URL, such as <c>http://myproxy.com:3128</c> or <c>socks5://myproxy.com:3128</c>.
    /// </summary>
    public required string Server { get; set; }

    /// <summary>
    /// Gets or sets the optional comma-separated list of domains to bypass the proxy,
    /// such as <c>.com, chromium.org, .domain.com</c>.
    /// </summary>
    public string? Bypass { get; set; }

    /// <summary>
    /// Gets or sets the optional username used for proxy authentication.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the optional password used for proxy authentication.
    /// </summary>
    public string? Password { get; set; }
}
