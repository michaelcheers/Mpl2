using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Api
{
    enum OperatorEnum
    {
        CemiCollon,
        Plus,
        Minus,
        Divide,
        Times,
        Equals,
        Comma,
        LessThan,
        GreaterThan,
        LessThanLessThan,
        GreaterThanGreaterThen,
        PlusEquals,
        MinusEquals,
        LessThanOrEquals,
        GreaterThanOrEquals
    }
    partial class Compiler
    {
        public void Tokenize(string tokenize)
        {
            tokens = new List<object>();
            CharEnumerator tokenizeenumerator = tokenize.GetEnumerator();
            tokenizeenumerator.MoveNext();
            while (TokenizerGetNext(tokenizeenumerator)) { }
        }
        /// <summary>
        /// Gets the next token in the string.
        /// </summary>
        /// <param name="enumerator">The enumerator to move.</param>
        /// <returns>Whether at end of string.</returns>
        public bool TokenizerGetNext(CharEnumerator enumerator)
        {
                switch (enumerator.Current)
                {
                    case '\"':
                        {
                            string current = "";
                            while (true)
                            {
                                if (enumerator.MoveNext())
                                {
                                    if (enumerator.Current == '\"')
                                    {
                                        tokens.Add(current);
                                        if (!enumerator.MoveNext())
                                        {
                                            TokenizeError("Unexpected end of file.");
                                            return false;
                                        }
                                        return true;
                                    }
                                    current += enumerator.Current;
                                }
                                else
                                {
                                    TokenizeError("Unexpected end of file.");
                                    return false;
                                }
                            }
                        }
                    case '\'':
                        {
                            if (enumerator.MoveNext())
                            {
                                tokens.Add(enumerator.Current);
                                if (!enumerator.MoveNext())
                                {
                                    TokenizeError("Unexpected end of file.");
                                    return false;
                                }
                                if (enumerator.Current == '\'')
                                {
                                    if (!enumerator.MoveNext())
                                    {
                                        TokenizeError("Unexpected end of file.");
                                        return false;
                                    }
                                }
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        {
                            int current = 0;
                            while (true)
                            {
                                switch (enumerator.Current)
                                {
                                    case '0':
                                    case '1':
                                    case '2':
                                    case '3':
                                    case '4':
                                    case '5':
                                    case '6':
                                    case '7':
                                    case '8':
                                    case '9':
                                        {
                                            if (current != 0)
                                            current *= 10;
                                            current += enumerator.Current - '0';
                                            break;
                                        }
                                    default:
                                        {
                                            tokens.Add(current);
                                            return true;
                                        }
                                }
                                if (!enumerator.MoveNext())
                                {
                                    tokens.Add(current);
                                    return false;
                                }
                            }
                        }
                    case ' ':
                    case '\t':
                    case '\r':
                    case '\n':
                        {
                        Switch:
                            switch (enumerator.Current)
                            {
                                case ' ':
                                case '\t':
                                case '\r':
                                case '\n':
                                    {
                                        if (!enumerator.MoveNext())
                                        {
                                            TokenizeError("Unexpected end of file.");
                                            return false;
                                        }
                                        goto Switch;
                                    }
                            }
                            return true;
                        }
                    case 'a':
                    case 'b':
                    case 'c':
                    case 'd':
                    case 'e':
                    case 'f':
                    case 'g':
                    case 'h':
                    case 'i':
                    case 'j':
                    case 'k':
                    case 'l':
                    case 'm':
                    case 'n':
                    case 'o':
                    case 'p':
                    case 'q':
                    case 'r':
                    case 's':
                    case 't':
                    case 'u':
                    case 'v':
                    case 'w':
                    case 'x':
                    case 'y':
                    case 'z':
                    case 'A':
                    case 'B':
                    case 'C':
                    case 'D':
                    case 'E':
                    case 'F':
                    case 'G':
                    case 'H':
                    case 'I':
                    case 'J':
                    case 'K':
                    case 'L':
                    case 'M':
                    case 'N':
                    case 'O':
                    case 'P':
                    case 'Q':
                    case 'R':
                    case 'S':
                    case 'T':
                    case 'U':
                    case 'V':
                    case 'W':
                    case 'X':
                    case 'Y':
                    case 'Z':
                    case '<':
                    case '>':
                    case '+':
                    case '=':
                    case ';':
                    case '-':
                    case '*':
                    case '/':
                    case '(':
                    case ')':
                    case ',':
                        {
                            string result = "";
                            Switch:
                            switch (enumerator.Current)
                            {
                                case 'a':
                                case 'b':
                                case 'c':
                                case 'd':
                                case 'e':
                                case 'f':
                                case 'g':
                                case 'h':
                                case 'i':
                                case 'j':
                                case 'k':
                                case 'l':
                                case 'm':
                                case 'n':
                                case 'o':
                                case 'p':
                                case 'q':
                                case 'r':
                                case 's':
                                case 't':
                                case 'u':
                                case 'v':
                                case 'w':
                                case 'x':
                                case 'y':
                                case 'z':
                                case 'A':
                                case 'B':
                                case 'C':
                                case 'D':
                                case 'E':
                                case 'F':
                                case 'G':
                                case 'H':
                                case 'I':
                                case 'J':
                                case 'K':
                                case 'L':
                                case 'M':
                                case 'N':
                                case 'O':
                                case 'P':
                                case 'Q':
                                case 'R':
                                case 'S':
                                case 'T':
                                case 'U':
                                case 'V':
                                case 'W':
                                case 'X':
                                case 'Y':
                                case 'Z':
                                case '<':
                                case '>':
                                case '+':
                                case '=':
                                case ';':
                                case '-':
                                case '*':
                                case '/':
                                case '(':
                                case ')':
                                case ',':
                                    {
                                        result += enumerator.Current;
                                        if (!enumerator.MoveNext())
                                        {
                                            tokens.Add(new Identifier(result));
                                            return false;
                                        }
                                        goto Switch;
                                    }
                                default:
                                    {
                                        bool found = false;
                                        foreach(Operator spec in operators)
                                        {
                                            if (spec.symbol == result)
                                            {
                                                tokens.Add(spec);
                                                found = true;
                                                break;
                                            }
                                        }

                                        if (!found)
                                        {
                                            tokens.Add(new Identifier(result));
                                        }
                                        return true;
                                    }
                            }
                        }
                    default:
                        {
                            throw new NotImplementedException();
                        }
                }
        }
        public void TokenizeError(string error)
        {
            throw new ArgumentException(error);
        }
        public void TokenizeError(string error, string line, string column)
        {
            TokenizeError("{0} at line {1}, at column {2}", error, line, column);
        }
        public void TokenizeError(string error, params object[] args)
        {
            TokenizeError(String.Format(error, args));
        }
    }

    public class Identifier
    {
        public override string ToString()
        {
            return Internal;
        }
        public Identifier(string Internal)
        {
            this.Internal = Internal;
        }
        public string Internal;
    }
}