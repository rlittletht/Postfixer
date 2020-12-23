using System;
using System.Collections.Generic;
using System.Text;

namespace TCore.PostfixText
{
    // The clause is the entire compiled PostFix string

    /*
     * CLAUSE:
     * [EXPRESSION] [EXPRESSION] [BOOLEAN_OP]
     *
     * EXPRESSION:
     * [TOKEN] [COMPARISON_OP] [TOKEN]
     *
     * VALUE:
     *   [a-zA-Z_][\w\[\]]+     // field value
     * | \d+                    // constant number value
     * | '.*'                   // constant string value (including string dates)
     *
     * COMPARISON_OP:
     * '==' | '!=' | '<' | '<=' | '>=' | '>' | ':=' | ':<' | ':<=' | ':>' | ':>'
     *
     * BOOLEAN_OP
     * 'AND' | 'OR'
     */
    public class Clause
    {
        internal enum ParseState
        {
            Pre,
            Expression,
            Operator
        }

        // ============================================================================
        // I T E M
        //
        // A single item in the clause -- either an expression or a PostFix operator
        // ============================================================================
        internal class Item
        {
            public enum Type
            {
                Expression,
                Operation
            }

            public Expression ItemExpression { get; set; }
            public PostfixOperator ItemOp { get; set; }
            public Type ItemType { get; set; }

            public Item(Expression expression)
            {
                ItemType = Type.Expression;
                ItemExpression = expression;
            }

            public Item(PostfixOperator op)
            {
                ItemType = Type.Operation;
                ItemOp = op;
            }
        }

        internal List<Item> m_items;
        internal ParseState m_state;
        internal Item m_itemBuilding;

        public Clause()
        {
            m_items = new List<Item>();
        }

        /*----------------------------------------------------------------------------
            %%Function: Clause
            %%Qualified: PostfixText.Parser.Clause.Clause
            
            Create a new clause, starting with an expression
        ----------------------------------------------------------------------------*/
        public Clause(Expression expression)
        {
            m_items = new List<Item>();
            StartItem(expression);
        }

        #region Parsing

        /*----------------------------------------------------------------------------
            %%Function: StartItem
            %%Qualified: PostfixText.Parser.Clause.StartItem
            
            Start a new item for the expression we are parsing. Set parse state
        ----------------------------------------------------------------------------*/
        void StartItem(Expression expression)
        {
            m_itemBuilding = new Item(expression);
            m_state = ParseState.Expression;
        }

        /*----------------------------------------------------------------------------
            %%Function: StartItem
            %%Qualified: PostfixText.Parser.Clause.StartItem
            
            Start a new item for the operator we are parsing. Set parse state
        ----------------------------------------------------------------------------*/
        void StartItem(PostfixOperator op)
        {
            m_itemBuilding = new Item(op);
            m_state = ParseState.Operator;
        }

        /*----------------------------------------------------------------------------
            %%Function: FAcceptParseStart
            %%Qualified: PostfixText.Parser.Clause.FAcceptParseStart
            
            Can we start parsing with this character?
        ----------------------------------------------------------------------------*/
        public static bool FAcceptParseStart(char ch, out Clause clause)
        {
            clause = null;

            // in order to start a clause, we have to start an expression
            if (Expression.FAcceptParseStart(ch, out Expression expression))
            {
                clause = new Clause(expression);
                return true;
            }

            return false;
        }

        /*----------------------------------------------------------------------------
            %%Function: ParseNextValueChar
            %%Qualified: PostfixText.Parser.Clause.ParseNextValueChar
            
            Parse the next character. Return false if we are done parsing, true if
            we should continue dispatching to this parser
        ----------------------------------------------------------------------------*/
        public bool ParseNextValueChar(char ch, out bool fUnget)
        {
            fUnget = false;

            if (m_state == ParseState.Expression)
            {
                if (m_itemBuilding.ItemExpression.ParseNextValueChar(ch, out bool fUngetExpression))
                    return true;

                // done parsing the expression
                m_items.Add(m_itemBuilding);
                m_itemBuilding = null;
                m_state = ParseState.Pre;

                if (!fUngetExpression)
                    return true;
            }

            if (m_state == ParseState.Operator)
            {
                if (m_itemBuilding.ItemOp.ParseNextValueChar(ch, out bool fUngetExpression))
                    return true;

                m_items.Add(m_itemBuilding);
                m_itemBuilding = null;
                m_state = ParseState.Pre;

                if (!fUngetExpression)
                    return true;
            }

            if (m_state == ParseState.Pre)
            {
                if (Expression.FAcceptParseStart(ch, out Expression expression))
                {
                    StartItem(expression);
                    return true;
                }

                if (PostfixOperator.FAcceptParseStart(ch, out PostfixOperator op))
                {
                    StartItem(op);
                    return true;
                }

                // this better be whitespace
                if (Char.IsWhiteSpace(ch))
                    return true; // just eat the whitespace

                throw new Exception($"unexpected character {ch} found in Pre parse state");
            }

            throw new Exception($"unhandled char {ch} in fallthrough");
        }

        /*----------------------------------------------------------------------------
            %%Function: FFinishParse
            %%Qualified: PostfixText.Parser.Clause.FFinishParse
            
            Can the parse be considered 'complete' at this point, with no further 
            input? (used at EOF)
        ----------------------------------------------------------------------------*/
        public bool FFinishParse()
        {
            if (m_state != ParseState.Pre)
            {
                if (!ParseNextValueChar('\0', out bool fUnget))
                {
                    // interestingly it thinks we're done. that's fine
                    return true;
                }

                // we successfully ended what we were parsing. now we
                // can end if we are in the pre state
            }

            if (m_state == ParseState.Pre)
                return true;

            return false;
        }
        #endregion
    }
}
