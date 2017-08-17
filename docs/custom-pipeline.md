---
title: Custom pipeline
---

You can string together the various components of the pipeline yourself. This is what methods like `AddJsBundle` and `MinifyCssFiles` are doing under the hood.

Imaging you had a bunch of `.txt` files that contained CSS and you wanted to bundle that up as a single CSS output file on the URL `http://localhost/bundle.css`. Here's what that could look like:

```csharp
services.AddWebOptimizer(pipeline =>
{
    pipeline.AddBundle("/bundle.css", "text/css; charset=utf-8", "/dir/*.txt")
            .AdjustRelativePaths()
            .Concatenate()
            .FingerprintUrls()
            .MinifyCss();
});
```

The `AddBundle` method is the base method used by `AddJsBundle` and `AddCssBundle` and takes a content type as the second parameter before the list of source files.

Any extension on top of WebOptimizer that bundles files would use `AddBundle` under the hood as well.