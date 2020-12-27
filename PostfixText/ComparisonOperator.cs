using System;
using System.Collections.Generic;
using System.Text;

namespace TCore.PostfixText
{
	public class ComparisonOperator
	{
		public enum Op
		{
			Gt = 0,
			Gte = 1,
			Lt = 2,
			Lte = 3,
			Eq = 4,
			Ne = 5,
			SCaseInsensitiveFirst = 6,
			SGt = SCaseInsensitiveFirst + Gt,
			SGte = SCaseInsensitiveFirst + Gte,
			SLt = SCaseInsensitiveFirst + Lt,
			SLte = SCaseInsensitiveFirst + Lte,
			SEq = SCaseInsensitiveFirst + Eq,
			SNe = SCaseInsensitiveFirst + Ne
		}

		internal bool m_fBuildingCaseInsensitive;
		internal char m_chLast;

		public Op Operator { get; set; }

		public ComparisonOperator(char ch)
		{
			if (ch == ':')
			{
				m_fBuildingCaseInsensitive = true;
				m_chLast = ch;
				return;
			}

			m_chLast = ch;
		}

		#region Parsing

		public static bool FAcceptParseStart(char ch, out ComparisonOperator cmpOperator)
		{
			cmpOperator = null;
			if (ch == ':' || ch == '>' || ch == '!' || ch == '=' || ch == '<')
			{
				cmpOperator = new ComparisonOperator(ch);
				return true;
			}

			return false;
		}

		public bool ParseNextValueChar(char ch, out bool fUnget)
		{
			fUnget = false;

			switch (m_chLast)
			{
				case ':':
					if (ch != '<' && ch != '>' && ch != '!' && ch != '=')
						throw new Exception($"ComparisonOperator.Parse: {ch} illegal after ':'");

					m_chLast = ch;
					return true;
				case '=':
					if (ch != '=')
						throw new Exception($"ComparisonOperator.Parse: {ch} illegal after '='");

					Operator = m_fBuildingCaseInsensitive ? Op.SEq : Op.Eq;
					return false;
				case '<':
					if (ch != '=')
					{
						Operator = m_fBuildingCaseInsensitive ? Op.SLt : Op.Lt;
						fUnget = true; // we didn't process this char
						return false;
					}

					Operator = m_fBuildingCaseInsensitive ? Op.SLte : Op.Lte;
					return false;
				case '>':
					if (ch != '=')
					{
						Operator = m_fBuildingCaseInsensitive ? Op.SGt : Op.Gt;
						fUnget = true; // we didn't process this char
						return false;
					}

					Operator = m_fBuildingCaseInsensitive ? Op.SGte : Op.Gte;
					return false;
				case '!':
					if (ch != '=')
						throw new Exception($"ComparisonOperator.Parse: {ch} illegal after '!'");

					Operator = m_fBuildingCaseInsensitive ? Op.SNe : Op.Ne;
					return false;
			}

			throw new Exception($"unknown internal state in ComparisonOperator parse: {m_chLast}");
		}

		public static Op OpCompareGenericFromOpCompare(Op op, out bool fNoCase)
		{
			if ((int) op >= (int) Op.SCaseInsensitiveFirst)
			{
				fNoCase = true;
				return (Op) ((int) op - (int) Op.SCaseInsensitiveFirst);
			}

			fNoCase = false;
			return op;
		}
		#endregion

		public override string ToString()
		{
			switch (Operator)
			{
				case Op.Gt: return ">";
				case Op.Gte: return ">=";
				case Op.Lt: return "<";
				case Op.Lte: return "<=";
				case Op.Eq: return "==";
				case Op.Ne: return "!=";
				case Op.SGt: return ":>";
				case Op.SGte: return ":>=";
				case Op.SLt: return ":<";
				case Op.SLte: return ":<=";
				case Op.SEq: return ":==";
				case Op.SNe: return ":!=";
				default: throw new Exception("unknown operator");
			}
		}
	}
}
