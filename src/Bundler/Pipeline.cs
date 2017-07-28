using System.Collections.Generic;
using Bundler.Processors;
using Microsoft.AspNetCore.Hosting;

namespace Bundler
{
    internal class Pipeline : IPipeline
    {
        public Pipeline(IHostingEnvironment env)
        {
            EnableCaching = !env.IsDevelopment();
        }

        public bool EnabledBundling { get; set; } = true;

        public bool EnableCaching { get; set; }

        public IPipeline Add(IAsset asset)
        {
            AssetManager.Assets.Add(asset);

            return this;
        }

        public IPipeline Add(IEnumerable<IAsset> asset)
        {
            AssetManager.Assets.AddRange(asset);

            return this;
        }

        public IAsset Add(string route, string contentType, params string[] sourceFiles)
        {
            string[] sources = sourceFiles;

            if (sourceFiles.Length == 0)
            {
                sources = new[] { route };
            }

            IAsset asset = Asset.Create(route, contentType, sources);
            AssetManager.Assets.Add(asset);

            return asset;
        }

        public IEnumerable<IAsset> AddFiles(string contentType, params string[] sourceFiles)
        {
            var list = new List<IAsset>();

            foreach (string file in sourceFiles)
            {
                IAsset asset = Add(file, contentType, file);
                asset.Processors.Add(new Concatinator());
                list.Add(asset);
            }

            return list;
        }
    }
}
