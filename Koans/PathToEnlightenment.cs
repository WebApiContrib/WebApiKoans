using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FSharpKoans.Core;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;
using Roslyn.Services.CSharp;

namespace Koans
{
    class Program
    {
        static void Main(string[] args)
        {
            var runner = new KoanRunner();
            var result = runner.ExecuteKoans();

            if (result is KoanResult.Success)
            {
                var success = (KoanResult.Success)result;
                Console.WriteLine(success.Item);
            }
            else if (result is KoanResult.Failure)
            {
                var failure = (KoanResult.Failure)result;
                var ex = failure.Item2;
                Console.Write(failure.Item1);
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("You have not yet reached enlightenment ...");
                Console.WriteLine(ex.Message);
                Console.WriteLine();
                Console.WriteLine("Please meditate on the following code:");
                Console.WriteLine(ex.StackTrace);
            }

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}
