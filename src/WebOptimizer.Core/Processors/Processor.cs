using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace WebOptimizer
{
    /// <summary>
    /// A base class for processors
    /// </summary>
    /// <seealso cref="WebOptimizer.IProcessor" />
    public abstract class Processor : IProcessor
    {
        /// <summary>
        /// Gets the custom key that should be used when calculating the memory cache key.
        /// </summary>
        public virtual string CacheKey(HttpContext context) => string.Empty;

        //private void foo(IAssetContext context)
        //{
        //    Type type = GetType();
        //    MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public);
        //    MethodInfo invokeMethod = methods.Where(m => m.Name.Equals("InvokeAsync", StringComparison.Ordinal)).FirstOrDefault();


        //    if (invokeMethod == null)
        //    {
        //        throw new InvalidOperationException("The processor must have an \"InvokeAsync\" method.");
        //    }

        //    if (!typeof(Task).IsAssignableFrom(invokeMethod.ReturnType))
        //    {
        //        throw new InvalidOperationException("The \"InvokeAsync\" mehtod must return a Task object.");
        //    }

        //    ParameterInfo[] parameters = invokeMethod.GetParameters();

        //    if (parameters.Length == 0 || parameters[0].ParameterType != typeof(IAssetContext))
        //    {
        //        throw new InvalidOperationException("The InvokeAsync method must take an IAssetContext as its first parameter.");
        //    }

        //    var list = new List<object>
        //    {
        //        context
        //    };

        //    foreach (ParameterInfo parameter in parameters)
        //    {
        //        object instance = ActivatorUtilities.GetServiceOrCreateInstance(context.HttpContext.RequestServices, parameter.ParameterType);
        //        list.Add(instance);
        //    }

        //    invokeMethod.Invoke(this, list.ToArray());

        //    //var instance = ActivatorUtilities.GetServiceOrCreateInstance<p.CreateInstance(context.HttpContext.RequestServices, type, context);
        //    //if (parameters.Length == 1)
        //    //{
        //    //    return (RequestDelegate)methodinfo.CreateDelegate(typeof(RequestDelegate), instance);
        //    //}

        //    //var factory = Compile<object>(methodinfo, parameters);

        //}

        /// <summary>
        /// Executes the processor on the specified configuration.
        /// </summary>
        public abstract Task ExecuteAsync(IAssetContext context);
    }
}
