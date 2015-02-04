using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Api
{
    public class Expression
    {
        public string name;
        public readonly List<Expression> children;
        public Dictionary<string, Expression> childrenByName;

        public Expression(List<Expression> children)
        {
            this.children = children;
        }

        public Expression()
        {
            this.children = null;
        }

        public Expression this[string searchName]
        {
            get
            {
                if (childrenByName == null)
                {
                    childrenByName = new Dictionary<string, Expression>();
                    AddChildrenByName(childrenByName);
                }

                if (childrenByName.ContainsKey(searchName))
                {
                    return childrenByName[searchName];
                }
                else
                {
                    return null;
                }
            }
        }

        void AddByName(Dictionary<string, Expression> dictionary)
        {
            if (name == null)
                return;

            if (name[0] == '*')
            {
                string trimmed = name.Substring(1);
                Expression_MultiBind elementContainer;
                if (dictionary.ContainsKey(trimmed))
                {
                    elementContainer = (Expression_MultiBind)dictionary[trimmed];
                }
                else
                {
                    elementContainer = new Expression_MultiBind();
                    dictionary[trimmed] = elementContainer;
                }
                elementContainer.Add(this);
            }
            else
            {
                dictionary[name] = this;
            }
        }

        public void AddChildrenByName(Dictionary<string, Expression> dictionary)
        {
            if (children == null)
                return;

            foreach (Expression child in children)
            {
                if (child.name != null)
                    child.AddByName(dictionary);
                else
                    child.AddChildrenByName(dictionary);
            }
        }

        public readonly static Expression ValidEmpty = new Expression();
    }

    public class Expression_Literal: Expression
    {
        public readonly object value;

        public Expression_Literal(object value)
        {
            this.value = value;
        }
    }

    public class Expression_Identifier : Expression
    {
        public readonly string text;

        public Expression_Identifier(string text)
        {
            this.text = text;
        }
    }

    public class Expression_Operator: Expression
    {
        public readonly Operator operatorType;

        public Expression_Operator(Expression lhs, Operator operatorType, Expression rhs): base(new List<Expression>(){lhs, rhs})
        {
            this.operatorType = operatorType;

            if (operatorType.infix && lhs != null && rhs != null)
            {
                // infix - check!
            }
            else if (operatorType.prefix && lhs != null && rhs == null)
            {
                // prefix - check!
            }
            else if (operatorType.postfix && lhs == null && rhs != null)
            {
                // postfix - check!
            }
            else
            {
                // invalid use of this operator.
            }
        }

        public Expression lhs
        {
            get{ return children[0]; }
            set{ children[0] = value; }
        }
        public Expression rhs
        {
            get{ return children[1]; }
            set{ children[1] = value; }
        }
    }

    public class Expression_MultiBind: Expression
    {
        public readonly List<Expression> binding = new List<Expression>();

        public void Add(Expression newBind)
        {
            binding.Add(newBind);
        }
    }

    partial class Compiler
    {
        // ANY pattern*
        // SEQUENCE pattern*
        // LITERAL string
        // OPTION pattern
        // MULTI pattern
        // OPERATORS leafnode operator*
        // BIND name pattern

        public Expression Parse(List<object> tokens)
        {
            //JSONTable patterns = settings.getJSON("patterns");
            int index = 0;
            //return ParseLevel(null, null, tokens, ref index);
            Expression result = ParsePattern(startPattern, tokens, ref index);
            if (index < tokens.Count)
            {
                return null;
            }
            else
            {
                return new Expression(new List<Expression>(){result});
            }
        }

        public JSONArray GetPattern(JSONArray parent, int index)
        {
            System.Object patternObject = parent.getProperty(index);
            if (patternObject is string)
            {
                return patterns.getArray((string)patternObject);
            }
            else
            {
                return new JSONArray((System.Object[])patternObject);
            }
        }

        public Expression ParsePattern(JSONArray pattern, List<object> tokens, ref int index)
        {
            switch (pattern.getString(0))
            {
                case "ANY":
                    {
                        for( int Idx = 1; Idx < pattern.Length; ++Idx )
                        {
                            JSONArray patternElement = GetPattern(pattern, Idx);
                            int subIndex = index;
                            Expression subExpression = ParsePattern(patternElement, tokens, ref subIndex);
                            if( subExpression != null )
                            {
                                index = subIndex;
                                return subExpression;
                            }
                        }

                        return null;
                    }
                
                case "SEQUENCE":
                    {
                        int subIndex = index;
                        List<Expression> resultList = new List<Expression>();
                        for( int Idx = 1; Idx < pattern.Length; ++Idx )
                        {
                            JSONArray patternElement = GetPattern(pattern, Idx);
                            Expression subExpression = ParsePattern(patternElement, tokens, ref subIndex);
                            if( subExpression == null )
                            {
                                return null;
                            }
                            else
                            {
                                resultList.Add(subExpression);
                            }
                        }
                        index = subIndex;
                        return new Expression(resultList);
                    }
                
                case "LITERAL":
                    {
                        string literal = pattern.getString(1);
                        object token = tokens[index];
                        bool match = false;

                        if( token is Identifier && literal == ((Identifier)token).Internal)
                        {
                            match = true;
                        }

                        if( match )
                        {
                            index++;
                            return new Expression_Literal(literal);
                        }
                        else
                        {
                            return null;
                        }
                    }

                case "IDENTIFIER":
                    {
                        object token = tokens[index];

                        if (token is Identifier)
                        {
                            index++;
                            return new Expression_Identifier(token.ToString());
                        }
                        else
                        {
                            return null;
                        }
                    }

                case "MULTI":
                    {
                        List<Expression> resultList = new List<Expression>();
                        JSONArray elementPattern = GetPattern(pattern, 1);
                        while(true)
                        {
                            Expression result = ParsePattern(elementPattern, tokens, ref index);
                            if (result != null)
                            {
                                resultList.Add(result);
                            }
                            else
                            {
                                break;
                            }
                        }
                        return new Expression(resultList);
                    }

                case "OPTION":
                    {
                        JSONArray patternElement = GetPattern(pattern, 1);
                        int subIndex = index;
                        Expression result = ParsePattern(patternElement, tokens, ref subIndex);
                        if (result != null)
                        {
                            index = subIndex;
                            return result;
                        }
                        else
                        {
                            return new Expression();
                        }
                   }

                case "OPERATORS":
                    {
                        int subIndex = index;
                        Expression result = ParseLevel(null, null, tokens, ref subIndex);
                        if( result != null )
                        {
                            index = subIndex;
                            return result;
                        }
                        else
                        {
                            return null;
                        }
                    }

                case "BIND":
                    {
                        string bindName = pattern.getString(1);
                        JSONArray patternElement = GetPattern(pattern, 2);
                        int subIndex = index;
                        Expression subExpression = ParsePattern(patternElement, tokens, ref subIndex);
                        if( subExpression != null )
                        {
                            index = subIndex;
                            subExpression.name = bindName;
                        }

                        return subExpression;
                    }
            }

            return null;
        }

        public Expression ParseLevel(Expression lhs, Operator op, List<object> tokens, ref int index)
        {
            Expression rhs = null;

            while (index < tokens.Count)
            {
                object token = tokens[index];
                if (token is int)
                {
                    index++;
                    rhs = new Expression_Literal(token);
                }
                else if (token is Operator)
                {
                    Operator newOp = (Operator)token;
                    bool startNewLevel = false;

                    if (op == null || op.precedence < newOp.precedence)
                    {
                        startNewLevel = true;
                    }
                    else if (rhs == null)
                    {
                        // we've got two operators in a row! e.g: 1 * -2
                        // Figure out what this means.
                        bool opNeedsRhs = lhs == null || !op.postfix;
                        bool newOpNeedsLhs = !op.prefix;

                        if (opNeedsRhs)
                        {
                            startNewLevel = true;
                        }

                        if (opNeedsRhs && newOpNeedsLhs)
                        {
                            // error: can't have these two operators together!
                        }
                    }

                    if (startNewLevel)
                    {
                        // the new operator is higher precedence. e.g: 1 + 2 * 3
                        // we need to _go deeper_...
                        index++;
                        rhs = ParseLevel(rhs, newOp, tokens, ref index);
                    }
                    else
                    {
                        // uh oh, the new operator is lower precedence, e.g: 1 * 2 + 3.
                        // bail out (without incrementing index) - we'll handle it in the outer level.
                        return new Expression_Operator(lhs, op, rhs);
                    }
                }
                else if (token is Identifier)
                {
                    index++;
                    return new Expression_Identifier(token.ToString());
                }
                else
                {
                    return null;
                }
            }

            if (op != null)
            {
                return new Expression_Operator(lhs, op, rhs);
            }

            if (lhs != null && rhs != null)
            {
                // error: need an operator between these values!
            }

            if (lhs != null)
                return lhs;
            else if (rhs != null)
                return rhs;
            else
                return null;
        }

        /*
        BEDMAS extended (), &&, ||, >, >=, <, <=, exponents, <<, >>, &, |, /, *, +, -.
Allow settings as json. Example:
settings
{
"foo":"bar",
"silly":"wash"
}
Simple c function declaration and calling.
All dots, Example: MyProgram.Program.Main(), Program.Main() or Main().
Array declaration [1, 3, 2].
c variable declaration, "IFast x = 0;".
c variable setting, "x = 0;
c pointer declaration "IFast* xPointer; = &x;"
c pointer setting getting "*xPointer = 7;" or "x = *xPointer;"
Allow types 
"bool" : "Boolean" ,
"byte" : "1 Byte Number",
"char" : "Character" ,
"I8" : "1 Byte Number",
"I16" : "2 Byte Number",
"I32" : "4 Byte Number",
"I64" : "8 Byte Number"
        */
        public void ParserGetNext()
        {
        }
    }
}
