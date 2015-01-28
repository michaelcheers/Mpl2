﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Programming_Language
{
    class Program
    {
        static string ConvertToString(object input)
        {
            return String.Format("Type:  {0}\r\nValue: {1}", input.GetType(), input);
        }
        static void Main(string[] args)
        {
            JSONTable compilerSettings = JSONTable.parse(MCompiler.Properties.Resources.CompilerSettings);
            Compiler c = new Compiler(compilerSettings);

            c.Tokenize("1+1*0");
            Console.Write(String.Join("\r\n", c.tokens.ConvertAll(ConvertToString)));

            Expression exp = c.Parse(c.tokens);



            while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }
        }
    }
}