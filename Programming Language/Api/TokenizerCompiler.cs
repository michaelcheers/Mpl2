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
                default:
                    {
                        bool hasMore = true;
                        if (IsLetter(enumerator.Current) || enumerator.Current == '_')
                        {
                            string text = "";
                            // Identifier
                            do
                            {
                                text += enumerator.Current;
                                if (!enumerator.MoveNext())
                                {
                                    hasMore = false;
                                    break;
                                }
                            } while (
                                    IsLetter(enumerator.Current) ||
                                    IsDigit(enumerator.Current) ||
                                    enumerator.Current == '_'
                                );

                            tokens.Add(new Identifier(text));
                        }
                        else if (IsSymbol(enumerator.Current))
                        {
                            string opToMatch = "";
                            do
                            {
                                opToMatch += enumerator.Current;
                                if (!enumerator.MoveNext())
                                {
                                    hasMore = false;
                                    break;
                                }
                            } while (IsSymbol(enumerator.Current));

                            string remainder = "";
                            while (opToMatch != "")
                            {
                                bool found = false;

                                foreach (Operator spec in operators)
                                {
                                    if (spec.symbol == opToMatch)
                                    {
                                        tokens.Add(spec);
                                        found = true;
                                        break;
                                    }
                                }

                                if (!found)
                                {
                                    if (opToMatch.Length > 1)
                                    {
                                        remainder = opToMatch.Last() + remainder;
                                        opToMatch = opToMatch.Substring(0, opToMatch.Length - 1);
                                    }
                                    else
                                    {
                                        //error, unknown operator
                                    }
                                }
                                else
                                {
                                    opToMatch = remainder;
                                    remainder = "";
                                }
                            }
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }

                        return hasMore;
                    }
            }
        }

        bool IsLetter(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        }

        bool IsDigit(char c)
        {
            return (c >= '0' && c <= '9');
        }

        bool IsSymbol(char c)
        {
            return symbols.Contains(c);
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