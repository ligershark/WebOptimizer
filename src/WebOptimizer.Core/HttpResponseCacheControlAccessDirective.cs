namespace WebOptimizer;

/// <summary>
/// RFC-9111 HTTP Caching defines support for HTTP response cache-control directives 'private' and 'public'.
/// </summary>
/// <remarks>See https://www.rfc-editor.org/rfc/rfc9111#name-private See https://www.rfc-editor.org/rfc/rfc9111#name-public</remarks>
public enum HttpResponseCacheControlAccessDirective
{
    /// <summary>
    /// See https://www.rfc-editor.org/rfc/rfc9111#name-private
    /// </summary>
    Private = 0,

    /// <summary>
    /// See https://www.rfc-editor.org/rfc/rfc9111#name-public
    /// </summary>
    Public
}
