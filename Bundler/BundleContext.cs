namespace Bundler
{
    /// <summary>
    /// A configuration object for Bundler.
    /// </summary>
    public class BundleContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BundleContext"/> class.
        /// </summary>
        public BundleContext(IBundle bundle)
        {
            Bundle = bundle;
        }

        /// <summary>
        /// Gets or sets the content of the response.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets the transform.
        /// </summary>
        public IBundle Bundle { get; }
    }
}
