# ASP.NET Core Web Optimizer

[![Build status](https://ci.appveyor.com/api/projects/status/twj2lkgnm4th6qh9?svg=true)](https://ci.appveyor.com/project/madskristensen/weboptimizer)
[![NuGet](https://img.shields.io/nuget/v/LigerShark.WebOptimizer.Core.svg)](https://nuget.org/packages/LigerShark.WebOptimizer.Core/)

ASP.NET Core middleware for bundling, minification and localization of CSS and JavaScript files.

## Content
- [How it works](#how-it-works)
- [Install](#install)
- [Configure](#configure)
- [Use](#use)
- [API reference](#api-reference)
- [Extend the pipeline](#extend)
- [Plugins](#plugins)

## How it works
Web Optimizer will concatinate (bundle) and minify any text based files such as JavaScript and CSS. This results in fewer HTTP requests and smaller payloads which increases the performance of your web application.

Consider a scenario where you have 3 JavaScript files in the `wwwroot` folder:

- a.js
- b.js
- c.js

Those files can be dynamically bundled, minified and served up from a new route called `/all.js` simply by registrating it in `Startup.cs`:

```c#
assets.AddJs("/all.js", "a.js", "b.js", "c.js");
```

The new route name is the first parameter, and the other parameters are source file routes. You can list as many as needed.

Then simply reference the new bundle route from the HTML like any other file:

```html
<script src="/all.js"></script>
```

Check out the [sample application](https://github.com/ligershark/WebOptimizer/blob/master/samples/WebOptimizer.Core.Sample/Startup.cs).

## Install
Add the NuGet package [LigerShark.WebOptimizer.Core](https://preview.nuget.org/packages/LigerShark.WebOptimizer.Core/) to any ASP.NET Core project supporting .NET Standard 1.6 or higher.

> &gt; dotnet add package LigerShark.WebOptimizer.Core

## Configure
First you need to configure Web Optimizer in `Startup.cs` and then register the TagHelpers.

### Step 1

In the `ConfigureServices` method, add a call to `Services.AddWebOptimizer` and create the assets (or bundles) for your application.

```c#
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc();
    services.AddWebOptimizer(assets =>
    {
        assets.AddCss("all.css", "a.css", "b.css");
        assets.AddJs("all.js", "a.js", "b.js", "c.js");
    });
}
```

### Step 2
Then register the middleware in the `Configure` method:

```c#
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

Make sure the call to `app.UseWebOptimizer` is before the call to `app.UseStaticFiles`.

### Step 3
In `_ViewImports.cshtml` register the TagHelpers by adding `@addTagHelper *, WebOptimizer.Core` to the file.

```text
@addTagHelper *, WebOptimizer.Core
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
```

Web Optimizer is now configured and we are ready to use it.

## Use
To use the two bundles we created in [step 1](#step-1), we must reference them from the HTML page. 

### Reference the bundles
We do that like we would reference any file that exists on disk:

```html
<link rel="stylesheet" href="/all.css" />
<scrpt src="/all.js"></script>
```

This will automatically add version strings to the paths and that looks like this:

```html
<link rel="stylesheet" href="all.css?v=OFNUnL-rtjZYOQwGomkVMwO415EOHtJ_Tu_s0SIlm9s" />
```

### Inline files
We can also use Web Optimizer to inline the content of the files directly into the Razor page. This is useful for creating high-performance websites that inlines the above-the-fold CSS and lazy loads the rest later.

To do this, simply add the attribute `inline` to any `<link>` or `<script>` element like so:

```html
<link rel="stylesheet" href="/a/bundle.css" inline />
<scrpt src="/any/file.js" inline></script>
```

### API reference
coming soon...

### Extend
coming soon...

### Plugins

- [Sass/Scss compiler](https://github.com/ligershark/WebOptimizer.Sass)
- [Markdown parser](https://github.com/ligershark/WebOptimizer.Markdown)
- [i18n (localization of .js files)](https://github.com/ligershark/WebOptimizer.i18n)