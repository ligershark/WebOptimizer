# ASP.NET Core Web Optimizer

[![Build status](https://ci.appveyor.com/api/projects/status/twj2lkgnm4th6qh9?svg=true)](https://ci.appveyor.com/project/madskristensen/weboptimizer)
[![NuGet](https://img.shields.io/nuget/v/LigerShark.WebOptimizer.Core.svg)](https://nuget.org/packages/LigerShark.WebOptimizer.Core/)
 
ASP.NET Core middleware for bundling and minification of CSS and JavaScript files at runtime. With full server-side and client-side caching to ensure high performance. No complicated build process and no hassle.

Check out the **[demo website](https://weboptimizer.azurewebsites.net/)** or its **[source code](https://github.com/ligershark/WebOptimizer/tree/master/sample)**.

## Versions
Master is being updated for ```ASP.NET Core 3.0```
For ```ASP.NET Core 2.x```, use the **[2.0 branch.](https://github.com/ligershark/WebOptimizer/tree/2.0)**
## Content

- [How it works](#how-it-works)
- [Install and setup](#install-and-setup)
- [Minification](#minification)
- [Bundling](#bundling)
- [Tag Helpers](#tag-helpers)
  - [Cache busting](#cache-busting)
  - [Inlining content](#inlining-content)
- [Compiling Scss](#compiling-scss)
- [Options](#options)
- [Custom pipeline](#custom-pipeline) 
- [Extend](#extend) 
- [Plugins](#plugins)

## How it works
WebOptimizer sets up a pipeline for static files so they can be transformed (minified, bundled, etc.) before sent to the browser. This pipeline is highly flexible and can be used to combine many different transformations to the same files. 

For instance, the pipeline for a single .css file could be orchestrated to first goes through minification, then through fingerprinting and finally through image inlining before being sent to the browser. 

WebOptimizer makes sure that the pipeline is orchestrated for maximum performance and compatability out of the box, yet at the same time allows for full customization and extensibility. 

The pipeline is set up when the ASP.NET web application starts, but no output is being generated until the first time they are requested by the browser. The output is then being stored in memory and served very fast on all subsequent requests. This also means that no output files are being generated on disk.

## Install and setup
Add the NuGet package [LigerShark.WebOptimizer.Core](https://nuget.org/packages/LigerShark.WebOptimizer.Core/) to any ASP.NET Core 2.0 project.

```cmd
dotnet add package LigerShark.WebOptimizer.Core 
```

Then in **Startup.cs**, add `app.UseWebOptimizer()` to the `Configure` method anywhere before `app.UseStaticFiles` (if present), like so:

```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseWebOptimizer();

    app.UseStaticFiles();
    app.UseMvc(routes =>
    {
        routes.MapRoute(
            name: "default",
            template: "{controller=Home}/{action=Index}/{id?}");
    });
}
```

That sets up the middleware that handles the requests and transformation of the files. 

And finally modify the *ConfigureServices* method by adding a call to `services.AddWebOptimizer()`:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc();
    services.AddWebOptimizer();
}
```

The service contains all the configuration used by the middleware and allows your app to interact with it as well.

That's it. You have now enabled automatic CSS and JavaScript minification. No other code changes are needed for enabling this. 

Try it by requesting one of your .css or .js files in the browser and see if it has been minified.

**Disabling minification:**  
If you want to disable minification (e.g. in development), the following overload for AddWebOptimizer() can be used:
```
if (env.IsDevelopment())
{
    services.AddWebOptimizer(minifyJavaScript:false,minifyCss:false);
}
```

## Minification
To control the minification in more detail, we must interact with the pipeline that manipulates the file content. 

> Minification is the process of removing all unnecessary characters from source code without changing its functionality in order to make it as small as possible.

For example, perhaps we only want a few certain JavaScript files to be minified automatically. Then we would write something like this:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc();
    services.AddWebOptimizer(pipeline =>
    {
        pipeline.MinifyJsFiles("js/a.js", "js/b.js", "js/c.js");
    });
}
```

Notice that the paths to the .js files are relative to the wwwroot folder.

We can do the same for CSS, but this time we're using a globbing pattern to allow minification for all .css files in a particular folder and its sub folders:

```csharp
pipeline.MinifyCssFiles("css/**/*.css");
```

When using globbing patterns, you still request the .css files on their relative path such as `http://localhost:1234/css/site.css`.

Setting up automatic minification like this doesn't require any other code changes in your web application to work.

> Under the hood, WebOptimizer uses [NUglify](https://github.com/xoofx/NUglify) to minify JavaScript and CSS.

## Bundling
To bundle multiple source file into a single output file couldn't be easier.

> Bundling is the process of taking multiple source files and combining them into a single output file. All CSS and JavaScript bundles are also being automatically minified.

Let's imagine we wanted to bundle `/css/a.css` and `/css/b.css` into a single output file and we want that output file to be located at `http://localhost/css/bundle.css`.

Then we would call the `AddCssBundle` method:

```csharp
services.AddWebOptimizer(pipeline =>
{
    pipeline.AddCssBundle("/css/bundle.css", "css/a.css", "css/b.css");
});
```

The `AddCssBundle` method will combine the two source files in the order they are listed and then minify the resulting output file. The output file `/css/bundle.css` is created and kept in memory and not as a file on disk.

To bundle all files from a particular folder, we can use globbing patterns like this:

```csharp
services.AddWebOptimizer(pipeline =>
{
    pipeline.AddCssBundle("/css/bundle.css", "css/**/*.css");
});
```

When using bundling, we have to update our `<script>` and `<link>` tags to point to the bundle route. It could look like this:

```html
<link rel="stylesheet" href="/css/bundle.css" />
```

### Content Root vs. Web Root
By default, all bundle source files are relative to the Web Root (*wwwroot*) folder, but you can change it to be relative to the Content Root instead.

> The Content Root folder is usually the project root directory, which is the parent directory of *wwwroot*.

As an example, lets create a bundle of files found in a folder called node_modules that exist in the Content Root:

```csharp
services.AddWebOptimizer(pipeline =>
{
    pipeline.AddCssBundle("/css/bundle.css", "node_modules/jquery/dist/*.js")
            .UseContentRoot();
});
```


The `UseContentRoot()` method makes the bundle look for source files in the Content Root rather than in the Web Root.

To use a completely custom `IFileProvider`, you can use the `UseFileProvider` pipeline method.

```csharp
services.AddWebOptimizer(pipeline =>
{
    var provider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(@"C:\path\to\my\root\folder");
    pipeline.AddJavaScriptBundle("/js/scripts.js", "a.js", "b.js")
        .UseFileProvider(provider);
});

```

## Tag Helpers
WebOptimizer ships with a few [Tag Helpers](https://docs.microsoft.com/en-us/aspnet/core/mvc/views/tag-helpers/intro) that helps with a few important tasks.

> Tag Helpers enable server-side code to participate in creating and rendering HTML elements in Razor files.

First, we need to register the TagHelpers defined in *LigerShark.WebOptimizer.Core* in our project.

To do that, go to `_ViewImports.cshtml` and register the Tag Helpers by adding `@addTagHelper *, WebOptimizer.Core` to the file.

```text
@addTagHelper *, WebOptimizer.Core
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
```
### Cache busting
As soon as the Tag Helpers are registered in your project, you'll notice how the `<script>` and `<link>` tags starts to render a little differently when they are referencing a file or bundle.

**NOTE:** Unlike other ASP.NET Core Tag Helpers, `<script>` and `<link>` tags don't need an `asp-` attribute to be rendered as a Tag Helper. 

They will get a version string added as a URL parameter:

```html
<link rel="stylesheet" href="/css/bundle.css?v=OFNUnL-rtjZYOQwGomkVMwO415EOHtJ_Tu_s0SIlm9s" />
```

This version string changes every time one or more of the source files are modified. 

This technique is called *cache busting* and is a critical component to achieving high performance, since we cannot utilize browser caching of the CSS and JavaScript files without it. That is also why it can not be disabled when using WebOptimizer.

### Inlining content
We can also use Web Optimizer to inline the content of the files directly into the Razor page. This is useful for creating high-performance websites that inlines the above-the-fold CSS and lazy loads the rest later.

To do this, simply add the attribute `inline` to any `<link>` or `<script>` element like so:

```html
<link rel="stylesheet" href="/css/bundle.css" inline />
<script src="/any/file.js" inline></script>
```

There is a Tag Helper that understands what the `inline` attribute means and handles the inlining automatically.

## Compiling Scss
WebOptimizer can also compile [Scss](http://sass-lang.com/) files into CSS. For that you need to install the `LigerShark.WebOptimizer.Sass` NuGet package and hooking it up is a breeze. Read more on the [WebOptimizer.Sass](https://github.com/ligershark/WebOptimizer.sass) website.

## Options
You can control the options from the appsettings.json file.

```json
{
  "webOptimizer": {
    "enableCaching": true,
    "enableMemoryCache": true,
    "enableDiskCache": true,
    "cacheDirectory": "/var/temp/weboptimizercache",
    "enableTagHelperBundling": true,
    "cdnUrl": "https://my-cdn.com/",
    "allowEmptyBundle": false
  }
}
```

**enableCaching** determines if the `cache-control` HTTP headers should be set and if conditional GET (304) requests should be supported. This could be helpful to disable while in development mode.

Default: **true**

**enableTagHelperBundling** determines if `<script>` and `<link>` elements should point to the bundled path or a reference per source file should be created. This is helpful to disable when in development mode.

Default: **true**

**enableMemoryCache** determines if the `IMemoryCache` is used for caching.  Can be helpful to disable while in development mode.

Default: **true**

**enableDiskCache** determines if the pipeline assets are cached to disk.  This can speed up application restarts by loading pipline assets from the disk instead of re-executing the pipeline.  Can be helpful to disable while in development mode.

Default: **true**

**cacheDirectory** sets the directory where assets will be stored if `enableDiskCache` is **true**.  Must be read/write.

Default: `<ContentRootPath>/obj/WebOptimizerCache`

**cdnUrl** is an absolute URL that, if present, is automatically adds a prefix to any script, stylesheet or media file on the page. A Tag Helper adds the prefix automatically when the Tag Helpers have been registered. See how to [register the Tag Helpers here](#tag-helpers).

For example. if the cdnUrl is set to `"http://my-cdn.com"` then script and link tags will prepend the *cdnUrl* to the references. For instance, this script tag:

**allowEmptyBundle** determines the behavior when there is no content in source file of a bundle, 404 exception will be thrown when the bundle is requested, set to true to get a bundle with empty content instead.

Default: **false**

```html
<script src="/js/file.js"></script>
```

...will become this:

```html
<script src="http://my-cdn.com/js/file.js"></script>
```

**allowEmptyBundle** determines the behavior when there is no content in source file of a bundle, by default 404 exception will be thrown when the bundle is requested, set to true to get a bundle with empty content instead.

Default: **false**

### Custom pipeline
Read more in the [custom pipeline documentation](https://ligershark.github.io/WebOptimizer/custom-pipeline.html).

### Extend
Extensions can hook up new transformations and consume existing ones. 

A good extension to look at is the [WebOptimizer.Sass](https://github.com/ligershark/WebOptimizer.Sass) extension. It demonstrates how to write a processor and how to write extension methods that makes it easy to hook it up to the pipeline. 

### Plugins

- [WebOptimizer.TypeScript](https://github.com/ligershark/WebOptimizer.TypeScript) - compiles TypeScript/ES6+/JSX files to JavaScript (ES5)
- [WebOptimizer.Sass](https://github.com/ligershark/WebOptimizer.Sass) - compiles Scss files to CSS
- [WebOptimizer.Less](https://github.com/ligershark/WebOptimizer.Less) - compiles LESS files to CSS
- [WebOptimizer.Dotless](https://github.com/twenzel/WebOptimizer.Dotless) - compiles LESS files to CSS (using [dotless](https://github.com/dotless/dotless) instead of node.js/less).
- [WebOptimizer.AutoPrefixer](https://github.com/ligershark/WebOptimizer.AutoPrefixer) - Adds vendor prefixes to CSS
- [WebOptimizer.Markdown](https://github.com/ligershark/WebOptimizer.Markdown) - compiles markdown files to HTML
- [WebOptimizer.i18n](https://github.com/ligershark/WebOptimizer.i18n) - localization of .js files
