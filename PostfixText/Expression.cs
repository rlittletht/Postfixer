using System;
using System.Collections.Generic;
using System.Text;

namespace TCore.PostfixText
{
	// Expression is a single expression as in:

	// [EXPRESSION]
	// [EXPRESSION] AND
	public class Expression
	{
		public Value LHS { get; set; }
		public Value RHS { get; set; }
		public ComparisonOperator.Op Operator => m_comparisonOp.Operator;

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

		public static Expression Create(Value lhs, Value rhs, ComparisonOperator op)
		{
			Expression expression = new Expression();

			expression.LHS = lhs;
			expression.RHS = rhs;
			expression.m_comparisonOp = op;

			return expression;
		}

		#region Parsing

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
				expression.LHS = value;
				expression.m_state = ParsingState.Value1;

				return true;
			}

			return false;
		}

		/*----------------------------------------------------------------------------
		    %%Function: ParseNextValueChar
		    %%Qualified: PostfixText.Parser.Expression.ParseNextValueChar
		    
		    Parse the next value character, delegating to whatever parser we are
		    currently working with. when done with value 2, we're done
		----------------------------------------------------------------------------*/
		public bool ParseNextValueChar(char ch, out bool fUnget)
		{
			fUnget = false;

			if (m_state == ParsingState.Value1)
			{
				if (LHS.ParseNextValueChar(ch, out bool fUngetValueChar))
					return true;

				m_state = ParsingState.PreCmpOp;
				// we're done parsing Value1. if we're not supposed to unget
				// the character, then return and we'll continue with the
				// next char
				if (!fUngetValueChar)
					return true;
			}

			if (m_state == ParsingState.PreCmpOp)
			{
				if (ComparisonOperator.FAcceptParseStart(ch, out ComparisonOperator cmpOperator))
				{
					m_comparisonOp = cmpOperator;
					m_state = ParsingState.CmpOp;
					return true;
				}

				if (!Char.IsWhiteSpace(ch))
					throw new Exception($"Illegal character {ch} encountered in PreCmpOp");

				return true;
			}

			if (m_state == ParsingState.CmpOp)
			{
				if (m_comparisonOp.ParseNextValueChar(ch, out bool fUngetCmpOp))
					return true;

				m_state = ParsingState.PreValue2;
				// we're done parsing. if we aren't supposed to unget, then return
				if (!fUngetCmpOp)
					return true;

				// otherwise fallthrough
			}

			if (m_state == ParsingState.PreValue2)
			{
				if (Value.FAcceptParseStart(ch, out Value value))
				{
					RHS = value;
					m_state = ParsingState.Value2;
					return true;
				}

				if (!Char.IsWhiteSpace(ch))
					throw new Exception($"Illegal character {ch} encountered in PreCmpOp");

				return true;
			}

			if (m_state == ParsingState.Value2)
			{
				if (RHS.ParseNextValueChar(ch, out bool fUngetValueChar))
					return true;

				m_state = ParsingState.Initial;
				// we're done parsing Value2. if we're not supposed to unget
				// the character, then return and we'll continue with the
				// next char
				fUnget = fUngetValueChar;
				return false;
			}

			throw new Exception("unknown state parsing expression");
		}

		#endregion

		public override string ToString()
		{
			return $"{LHS.ToString()} {m_comparisonOp.ToString()} {RHS.ToString()}";
		}
	}
}
