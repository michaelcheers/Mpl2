using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Programming_Language
{
    public abstract class Expression
    {
        public readonly List<Expression> children;

        public Expression(List<Expression> children)
        {
            this.children = children;
        }

        public Expression()
        {
            this.children = null;
        }
    }

    public class Expression_Literal: Expression
    {
        public readonly object value;

        public Expression_Literal(object value)
        {
            this.value = value;
        }
    }

    public class Expression_Identifier: Expression
    {
        public readonly string identifier;

        public Expression_Identifier(string identifier)
        {
            this.identifier = identifier;
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

    partial class Compiler
    {
        public Expression Parse(List<object> tokens)
        {
            //JSONTable patterns = settings.getJSON("patterns");
            int index = 0;
            return ParseLevel(null, null, tokens, ref index);
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
                        
                        if ( opNeedsRhs )
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
