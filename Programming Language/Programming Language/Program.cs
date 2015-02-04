using Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Programming_Language
{
    static class Program
    {
        static string ConvertToString(object input)
        {
            return String.Format("Type:  {0}\r\nValue: {1}", input.GetType(), input);
        }
        static void Main(string[] args)
        {
            JSONTable compilerSettings = JSONTable.parse(new System.IO.StreamReader(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Programming_Language.CompilerSettings.json")).ReadToEnd());
            Compiler c = new Compiler(compilerSettings);

            c.Tokenize("hello(cruel, world)");
            Console.WriteLine("tokens:\n");
            Console.Write(String.Join("\r\n", c.tokens.ConvertAll(ConvertToString)));

            c.Parse(c.tokens);

            //Console.WriteLine("Parse Tokens:\n");
            //Console.Write(c.parseTokens);
            Expression fnArgs = c.parseTokens["fncall"]["args"];

            while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }
        }
    }
}
