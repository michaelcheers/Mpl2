using Api;
using System;
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
            JSONTable compilerSettings = JSONTable.parse(new System.IO.StreamReader(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Programming_Language.CompilerSettings.json")).ReadToEnd());
            Compiler c = new Compiler(compilerSettings);

            c.Tokenize("hello ( cruel , world )");
            Console.Write(String.Join("\r\n", c.tokens.ConvertAll(ConvertToString)));

            Expression exp = c.Parse(c.tokens);

            Expression fnArgs = exp["fncall"]["args"];

            while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }
        }
    }
}
