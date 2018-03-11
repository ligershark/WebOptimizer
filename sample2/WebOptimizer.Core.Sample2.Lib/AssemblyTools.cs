using System.Reflection;

namespace WebOptimizer.Core.Sample2.Lib
{
    public class AssemblyTools
    {
        public static Assembly GetCurrentAssembly()
        {
            return typeof(AssemblyTools).Assembly;
        }
    }
}
