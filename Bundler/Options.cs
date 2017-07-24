using Bundler.Transformers;
using System.Collections.Generic;

namespace Bundler
{
    public class Options
    {
        public bool Enabled { get; set; } = true;
        public List<ITransform> Transforms { get; } = new List<ITransform>();
    }
}
