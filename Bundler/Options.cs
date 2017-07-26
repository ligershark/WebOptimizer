using System.Collections.Generic;

namespace Bundler
{
    /// <summary>
    /// Options for the bundler transform.
    /// </summary>
    public class Options
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Options"/> is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets a list of transforms added.
        /// </summary>
        public List<Bundle> Bundles { get; } = new List<Bundle>();
    }
}
