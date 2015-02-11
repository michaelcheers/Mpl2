using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Json_Reader;

namespace Api
{
    public class Operator
    {
        public override string ToString()
        {
            return symbol;
        }
        public readonly string symbol;
        public readonly int precedence;
        public readonly bool infix;
        public readonly bool prefix;
        public readonly bool postfix;

//        static Dictionary<string, OperatorEnum> operatordictionary = null;
        public Operator(string symbol, int precedence, bool infix, bool prefix, bool postfix)
        {
            this.symbol = symbol;
            this.precedence = precedence;
            this.infix = infix;
            this.prefix = prefix;
            this.postfix = postfix;
        }

        public Operator(string description, int precedence)
        {
            this.precedence = precedence;

            string[] splits = description.Split(' ');

            if (splits.Length <= 1)
            {
                this.symbol = description;
                this.infix = true;
                this.prefix = false;
                this.postfix = false;
            }
            else
            {
                this.symbol = splits[0];

                foreach (string split in splits.Skip(1))
                {
                    if (split == "infix")
                        this.infix = true;
                    else if (split == "prefix")
                        this.prefix = true;
                    else if (split == "postfix")
                        this.postfix = true;
                    else
                        throw new ArgumentException("Found an invalid operator setting: \"" + split + "\"");
                }

                if (!infix && !prefix && !postfix)
                {
                    infix = true; // the default
                }
            }
        }
    }

    public partial class Compiler
    {
//        public static string[] operators = new string [] { ",", "+", "-", "=", "/", "<", ">", "<<", ">>", "+=", "-=" };
        public List<object> tokens;
        
        List<Operator> operators;
        JSONTable patterns;
        JSONArray startPattern;

        public Compiler () : this (JSONTable.parse(new System.IO.StreamReader(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Api.CompilerSettings.json")).ReadToEnd()))
        {

        }

        public Compiler(JSONTable patterns)
        {
            this.patterns = patterns;

            this.operators = new List<Operator>();
            int precedence = 0;
            foreach (string s in patterns.getArray("SYMBOLS").asStrings())
            {
                operators.Add(new Operator(s, precedence));
                precedence++;
            }

            string startPatternName = patterns.getString("START", null);
            if (startPatternName != null)
            {
                startPattern = patterns.getArray(startPatternName);
            }
            else
            {
                startPattern = patterns.getArray("START", null);
            }
        }

        /// <summary>
        /// Compiles the code and returns c.
        /// </summary>
        /// <param name="input">The code to compile.</param>
        /// <returns>The c code that was compiled.</returns>
        public string Compile(string input)
        {
            Tokenize(input);
            Parse(tokens);
            return CompileToC(parseTokens);
        }
    }
}
