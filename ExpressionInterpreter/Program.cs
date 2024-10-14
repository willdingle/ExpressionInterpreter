using FsharpLib;
using System;

namespace CSharApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Simple Interpreter");
            var varTable = new Dictionary<string,double>();

            while (true)
            {
                Console.Write("-> ");
                string input = Console.ReadLine();

                var oList = Interpreter.lexer(input);
                var sList = Interpreter.printTList(oList);
                //var pList = Interpreter.printTList(Interpreter.parser(oList));
                var Out = Interpreter.parseNeval(oList, varTable);
                Console.WriteLine("Result = {0}", Out.Item2);
            }
        }
    }
}

