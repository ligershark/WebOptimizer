using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Bundler.Transformers
{
    public interface ITransform
    {
        string Path { get; }
        IEnumerable<string> SourceFiles { get; }
        string ContentType { get; }

        string Transform(HttpContext context, string source);
        ITransform Include(params string[] sourceFiles);
    }
}
