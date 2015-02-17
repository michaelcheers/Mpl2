using Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Json_Reader;

namespace Api
{
    public class Expression
    {
        public override string ToString()
        {
            return (childrenByName.ConvertToString(ToString, ToString));
        }

        private static string ToString(string input)
        {
            return input;
        }

        private static string ToString(Expression input)
        {
            return input.ToString();
        }

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

    public class Expression_Literal : Expression
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

    public class Expression_Operator : Expression
    {
        public readonly Operator operatorType;

        public Expression_Operator(Expression lhs, Operator operatorType, Expression rhs) : base(new List<Expression>() { lhs, rhs })
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
            get { return children[0]; }
            set { children[0] = value; }
        }
        public Expression rhs
        {
            get { return children[1]; }
            set { children[1] = value; }
        }
    }

    public class Expression_MultiBind : Expression
    {
        public readonly List<Expression> binding = new List<Expression>();

        public void Add(Expression newBind)
        {
            binding.Add(newBind);
        }
    }

    public class PatternScope
    {
        JSONTable scope;
        Dictionary<string, Pattern> elements;

        public PatternScope(JSONTable scope)
        {
            this.scope = scope;
            this.elements = new Dictionary<string, Pattern>();
        }

        public Pattern getPattern(string name)
        {
            if (elements.ContainsKey(name))
            {
                return elements[name];
            }
            else
            {
                Pattern result = toPattern(scope.getArray(name));
                elements[name] = result;
                return result;
            }
        }

        public Pattern getPattern(JSONArray array, int index)
        {
            System.Object obj = array[index];
            if (obj is string)
            {
                return getPattern((string)obj);
            }
            else if (obj is System.Object[])
            {
                return toPattern(new JSONArray((System.Object[])obj));
            }
            else
            {
                return null;
            }
        }

        public Pattern toPattern(JSONArray template)
        {
            switch (template.getString(0))
            {
                case "ANY": return new Pattern_Any(template, this);
                case "SEQUENCE": return new Pattern_Sequence(template, this);
                case "LITERAL": return new Pattern_Literal(template, this);
                case "IDENTIFIER": return new Pattern_Identifier(template, this);
                case "BIND": return new Pattern_Bind(template, this);
                case "OPTION": return new Pattern_Optional(template, this);
                case "MULTI": return new Pattern_Multi(template, this);
                case "OPERATORS": return new Pattern_Operators(template, this);
                default:
                    //ReportError();
                    return null;
            }
        }
    }

    public interface Pattern
    {
        Expression Parse(List<object> tokens, ref int index);
    }

    class Pattern_Any : Pattern
    {
        PatternScope scope;
        JSONArray template;
        List<Pattern> options;

        public Pattern_Any(JSONArray template, PatternScope scope)
        {
            this.scope = scope;
            this.template = template;
        }

        private void PopulateOptions()
        {
            if (this.options != null)
                return;

            this.options = new List<Pattern>();
            for ( int Idx = 1; Idx < template.Length; ++Idx )
            {
                options.Add(scope.getPattern(template, Idx));
            }
        }

        public Expression Parse(List<object> tokens, ref int index)
        {
            PopulateOptions();
            foreach(Pattern p in options)
            {
                int subIndex = index;
                Expression subExpression = p.Parse(tokens, ref subIndex);
                if (subExpression != null)
                {
                    index = subIndex;
                    return subExpression;
                }
            }

            return null;
        }
    }

    class Pattern_Sequence : Pattern
    {
        PatternScope scope;
        JSONArray template;
        List<Pattern> sequence;

        public Pattern_Sequence(JSONArray template, PatternScope scope)
        {
            this.template = template;
            this.scope = scope;
        }

        private void PopulateSequence()
        {
            if (this.sequence != null)
                return;

            this.sequence = new List<Pattern>();
            for (int Idx = 1; Idx < template.Length; ++Idx)
            {
                sequence.Add(scope.getPattern(template, Idx));
            }
        }

        public Expression Parse(List<object> tokens, ref int index)
        {
            PopulateSequence();

            int subIndex = index;
            List<Expression> resultList = new List<Expression>();
            foreach( Pattern p in sequence )
            {
                Expression subExpression = p.Parse(tokens, ref subIndex);
                if (subExpression == null)
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
    }

    class Pattern_Literal : Pattern
    {
        string literal;

        public Pattern_Literal(JSONArray template, PatternScope scope)
        {
            literal = template.getString(1);
        }

        public Expression Parse(List<object> tokens, ref int index)
        {
            object token = tokens[index];
            bool match = false;

            if (token is Identifier && literal == ((Identifier)token).Internal)
            {
                match = true;
            }
            else if( token is Operator && literal == ((Operator)token).symbol)
            {
                match = true;
            }

            if (match)
            {
                index++;
                return new Expression_Literal(literal);
            }
            else
            {
                return null;
            }
        }
    }

    class Pattern_Identifier : Pattern
    {
        public Pattern_Identifier(JSONArray template, PatternScope scope)
        {
        }

        public Expression Parse(List<object> tokens, ref int index)
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
    }

    class Pattern_Multi : Pattern
    {
        Pattern body;
        public Pattern_Multi(JSONArray template, PatternScope scope)
        {
            body = scope.getPattern(template, 1);
        }

        public Expression Parse(List<object> tokens, ref int index)
        {
            List<Expression> resultList = new List<Expression>();
            while (true)
            {
                Expression result = body.Parse(tokens, ref index);
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
    }

    class Pattern_Optional : Pattern
    {
        Pattern body;

        public Pattern_Optional(JSONArray template, PatternScope scope)
        {
            body = scope.getPattern(template, 1);
        }

        public Expression Parse(List<object> tokens, ref int index)
        {
            int subIndex = index;
            Expression result = body.Parse(tokens, ref subIndex);
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
    }

    class Pattern_Operators : Pattern
    {
        public Pattern_Operators(JSONArray template, PatternScope scope)
        {
            
        }

       public Expression Parse(List<object> tokens, ref int index)
        {
            int subIndex = index;
            Expression result = ParseLevel(null, null, tokens, ref subIndex);
            if (result != null)
            {
                index = subIndex;
                return result;
            }
            else
            {
                return null;
            }
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
    }

    class Pattern_Bind : Pattern
    {
        Pattern body;
        string bindName;

        public Pattern_Bind(JSONArray template, PatternScope scope)
        {
            bindName = template.getString(1);
            body = scope.getPattern(template, 2);
        }

        public Expression Parse(List<object> tokens, ref int index)
        {
            int subIndex = index;
            Expression subExpression = body.Parse(tokens, ref subIndex);
            if (subExpression != null)
            {
                index = subIndex;
                subExpression.name = bindName;
            }

            return subExpression;
        }
    }

    partial class Compiler
    {
        public Expression parseTokens;

        // ANY pattern*
        // SEQUENCE pattern*
        // LITERAL string
        // OPTION pattern
        // MULTI pattern
        // OPERATORS leafnode operator*
        // BIND name pattern

        public void Parse (List<object> tokens)
        {
            parseTokens = ParseBase(tokens);
        }

        public Expression ParseBase(List<object> tokens)
        {
            int index = 0;
            string startName = settings.getString("START");
            PatternScope scope = new PatternScope(settings);
            Pattern rootPattern = scope.getPattern(startName);

            Expression result = rootPattern.Parse(tokens, ref index);
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
                return settings.getArray((string)patternObject);
            }
            else
            {
                return new JSONArray((System.Object[])patternObject);
            }
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
