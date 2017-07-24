using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Bundler.Transformers
{
    public abstract class BaseTransform : ITransform
    {
        public BaseTransform(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new System.ArgumentException($"The \"{nameof(path)}\" parameter must be specified and not empty", nameof(path));
            }

            if (!path.StartsWith('/'))
            {
                throw new System.ArgumentException("Path must start with a /", nameof(path));
            }

            Path = path;
        }

        public string Path { get; }

        public IEnumerable<string> SourceFiles { get; set; }

        public abstract string ContentType {get;}

        public ITransform Include(params string[] sourceFiles)
        {
            SourceFiles = sourceFiles;

            return this;
        }

        public abstract string Transform(HttpContext context, string source);
    }
}
