using Solnet.Programs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Solnet.Serum.Examples
{
    public class ExampleExplorer
    {
        public static void Main(string[] args)
        {
            InstructionDecoder.Register(SerumProgram.MainNetProgramIdKeyV3, SerumProgram.Decode);
            List<Type> examples = Assembly.GetEntryAssembly()?.GetExportedTypes().Where(t => t.IsAssignableTo(typeof(IRunnableExample))).ToList();
            if (examples == null)
                return;

            while(true)
            {
                Console.WriteLine("Choose an example to run: ");
                int i = 0;
                foreach(Type ex in examples)
                {
                    Console.WriteLine($"{i++}){ex.Name}");
                }

                string option = Console.ReadLine();

                if(int.TryParse(option, out int res) && res <= examples.Count && res >= 0)
                {
                    Type t = examples[res];
                    IRunnableExample example = (IRunnableExample)t.GetConstructor(Type.EmptyTypes)?.Invoke(null);

                    example?.Run();
                }
                else
                {
                    Console.WriteLine("invalid option");
                }
            }

        }
    }
}