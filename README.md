# ASP.NET Core Web Optimizer

[![Build status](https://ci.appveyor.com/api/projects/status/twj2lkgnm4th6qh9?svg=true)](https://ci.appveyor.com/project/madskristensen/weboptimizer)
[![NuGet](https://img.shields.io/nuget/v/LigerShark.WebOptimizer.Core.svg)](https://nuget.org/packages/LigerShark.WebOptimizer.Core/)

ASP.NET Core middleware for bundling and minification of CSS and JavaScript files.

 - [Install and setup](#install-and-setup)
 - [Minification](#minification)
 - [Bundling](#bundling)
 - [Tag Helpers](#tag-helpers)
   - [Cache busing](#cache-busting)
   - [Inlining content](#inlining-content)
- [Compiling Scss](#compiling-scss)
- [Options](#options)
- [Custom pipeline](#custom-pipeline) 
- [Extend](#extend) 
- [Plugins](#plugins)

## Install and setup
Add the NuGet package [LigerShark.WebOptimizer.Core](https://nuget.org/packages/LigerShark.WebOptimizer.Core/) to any ASP.NET Core project supporting .NET Standard 2.0 or higher.

> &gt; dotnet add package LigerShark.WebOptimizer.Core

Then in **Startup.cs**, add two using statements:

```csharp
using WebOptimizer;
using WebOptimizer.Sass;
```

...and add `app.UseWebOptimizer()` to the `Configure` method anywhere before `app.UseStaticFiles`, like so:

```csharp
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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

...and finally modify the *ConfigureServices* method by adding a call to `services.AddWebOptimizer()`:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc();
    services.AddWebOptimizer();
}
```

That's it. You have now enabled automatic CSS and JavaScript minification. No other code changes are needed for enabling this. 

Any static .css and .js file reqested by the browser is now automatically minified and cached - both client-side and server-side caching.

## Minification
To control the minification in more detail, we must interact with the pipeline that manipulates the file content. 

For example, perhaps we only want a few particular JavaScript files to be minified automatically. Then we would write something like this:

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

Setting up automatic minification like this doesn't require any other code changes in your web application to work.

> WebOptimizer uses [NUglify](https://github.com/xoofx/NUglify) to minify JavaScript and CSS.

## Bundling
To bundle multiple source file into a single output file couldn't be easier.

Let's imagine we wanted to bundle `/css/a.css` and `/css/b.css` into a single output file and we want that output file to be located at `http://localhost/css/bundle.css`.

Then we would call the `AddCssBundle` method:

```csharp
services.AddWebOptimizer(pipeline =>
{
    pipeline.AddCssBundle("/css/bundle.css", "css/a.css", "css/b.css");
});
```

The `AddCssBundle` method will combine the two source files in the order they are listed and then minify the resulting output file. The file `/css/bundle.css` lives in memory only and not as a file on disk.

To bundle all files from a particular folder, we can use globbing patterns like this:

```csharp
services.AddWebOptimizer(pipeline =>
{
    pipeline.AddCssBundle("/css/bundle.css", "css/*.css");
});
```

When using bundling, we have to update our `<script>` and `<link>` tags to point to the bundle route. It could look like this:

```html
<link rel="stylesheet" href="/css/bundle.css" />
```

## Tag Helpers
WebOptimizer ships with a few Tag Helpers that helps with a few important tasks.

First of all, we need to register them in our project. 

To do that, go to `_ViewImports.cshtml` and register the Tag Helpers by adding `@addTagHelper *, WebOptimizer.Core` to the file.

```text
@addTagHelper *, WebOptimizer.Core
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
```
### Cache busting
As soon as the Tag Helpers are registered in your project, you'll notice how the `<script>` and `<link>` tags starts to render a little differently when they are referencing a bundle.

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
<scrpt src="/any/file.js" inline></script>
```

## Compiling Scss
WebOptimizer can also compile Scss files into CSS. For that you need to install the `LigerShark.WebOptimizer.Sass` NuGet package and hooking it up is a breeze. Read more on the [WebOptimizer.Sass](https://github.com/ligershark/WebOptimizer.sass) website.

## Options
You can control the options from the appsettings.json file.

```json
{
  "WebOptimizer": {
    "EnableCaching": true,
    "EnableTagHelperBundling": true,
    "UseContentRoot":  false
  }
}
```

**EnableCaching** determines if the `cache-control` HTTP headers should be set and if conditional GET (304) requests should be supported. This could be helpful to disable while in development mode.

Default: **true**

**EnableTagHelperBundling** determines if `<script>` and `<link>` elements should point to the bundled path or a reference per source file should be created. This is helpful to disable when in development mode.

Default: **true**

**UseContentRoot** allows you to use files from behind the `wwwroot` folder as source files to bundles.

Default: **false**

### Custom pipeline
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

### Extend
A good extension to look at is the [WebOptimizer.Sass](https://github.com/ligershark/WebOptimizer.Sass) extension. It demonstrates how to write a processor and how to write extension methods that makes it easy to hook it up to the pipeline. 

### Plugins

- [Sass/Scss compiler](https://github.com/ligershark/WebOptimizer.Sass)
- [Markdown parser](https://github.com/ligershark/WebOptimizer.Markdown)
- [i18n (localization of .js files)](https://github.com/ligershark/WebOptimizer.i18n)