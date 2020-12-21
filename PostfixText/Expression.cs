using System;
using System.Collections.Generic;
using System.Text;

namespace PostfixText
{
    // Expression is a single expression as in:

    // [EXPRESSION]
    // [EXPRESSION] AND

    public partial class Parser
    {
        public class Expression
        {
            internal Value m_lhs;
            internal Value m_rhs;

            internal ComparisonOperator m_comparisonOp;
            internal enum ParsingState
            {
                Initial,
                Value1,
                PreCmpOp,
                CmpOp,
                PreValue2,
                Value2
            }

            internal ParsingState m_state;

            public Expression() { }

            /*----------------------------------------------------------------------------
            	%%Function: FAcceptParseStart
            	%%Qualified: PostfixText.Parser.Expression.FAcceptParseStart
            	
                will this expression accept this char as the start of an expression?

                EXPRESSION:
                [VALUE] [CmpOp] [VALUE]
            ----------------------------------------------------------------------------*/
            public static bool FAcceptParseStart(char ch, out Expression expression)
            {
                expression = null;
                if (Value.FAcceptParseStart(ch, out Value value))
                {
                    expression = new Expression();
                    expression.m_lhs = value;
                    expression.m_state = ParsingState.Value1;

                    return true;
                }
                return false;
            }

            public bool ParseNextValueChar(char ch, bool fUnget)
            {
                if (m_state == ParsingState.Value1)
                {
                    if (m_lhs.ParseNextValueChar(ch, out bool fUngetValueChar))
                        return true;

                    m_state = ParsingState.PreCmpOp;
                    // we're done parsing Value1. if we're not supposed to unget
                    // the character, then return and we'll continue with the
                    // next char
                    if (!fUngetValueChar)
                        return true;
                }

                return false;
            }
        }
    }
}
