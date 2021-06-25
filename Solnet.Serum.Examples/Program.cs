using System;
using System.Linq;

namespace Solnet.Serum.Examples
{
    /// <summary>
    /// The console application.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// The examples entrypoint.
        /// <remarks>
        /// An argument equal to the name of the class that implements the example should be passed.
        /// </remarks>
        /// </summary>
        /// <param name="args">The arguments.</param>
        private static void Main(string[] args)
        {
            Console.WriteLine($"Attempting to run example: {args[0]}");
            Type type = typeof(IRunnableExample);             
            Type example = AppDomain.CurrentDomain                 
                .GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .FirstOrDefault(p => type.IsAssignableFrom(p) && p.Name == args[0]);
            
            if (example != null)
            {
                IRunnableExample runnable = Activator.CreateInstance(example) as IRunnableExample;
                runnable?.Run();
            }
            else
            {
                Console.WriteLine($"Example not found.");
            }
        }
    }
}