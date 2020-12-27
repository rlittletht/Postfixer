using System;
using System.Collections.Generic;
using System.Text;

namespace TCore.PostfixText
{
	// A token is a single token, as in:
	public class Value
	{
		public enum ValueType
		{
			String, // enclosed in literals
			Number, // leading digit
			DateTime, // enclosed in {}
			Field // enclosed in []
		}

		internal string m_value;

		// Value Caches -- these are the parsed equivalents, populated on demand based on the value type
		// (fields are always calculated and never cache -- they rely on the valueClient)
		internal DateTime? m_dttmValueCache;
		internal int? m_nValueCache;
		internal ValueType m_type;

		public ValueType Type => m_type;
		public string _Value => m_value;
		
		public Value() { }

		public static Value Create(ValueType type, string sValue)
		{
			Value value = new Value();

			value.m_type = type;
			value.m_value = sValue;
			
			return value;
		}

		public static Value Create(string value) => Create(ValueType.String, value);
		public static Value Create(int value) => Create(ValueType.Number, value.ToString());
		public static Value Create(DateTime value) => Create(ValueType.DateTime, value.ToString("MM/dd/yyyy"));
		public static Value CreateForField(string value) => Create(ValueType.Field, value);

		#region Parsing

		internal StringBuilder m_sbValue;
		internal bool m_fParsingEscape = false;

		/*----------------------------------------------------------------------------
		    %%Function: Value
		    %%Qualified: PostfixText.Parser.Value.Value
		    
		    Create a new value
		----------------------------------------------------------------------------*/
		public Value(char chFirst, ValueType type)
		{
			m_sbValue = new StringBuilder();
			// don't append the enclosing characters for strings or dates
			if (type != ValueType.DateTime && type != ValueType.String && type != ValueType.Field)
				m_sbValue.Append(chFirst);
			m_type = type;
		}

		/*----------------------------------------------------------------------------
		    %%Function: FAcceptValueStart
		    %%Qualified: PostfixText.Parser.Value.FAcceptValueStart
		    
		    will this value accept this character as the start of a value? if so, 
		    create a new value. otherwise, return false and value is null
		----------------------------------------------------------------------------*/
		public static bool FAcceptParseStart(char ch, out Value value)
		{
			value = null;

			if (ch == '[')
			{
				value = new Value(ch, ValueType.Field);
				return true;
			}

			if (char.IsDigit(ch))
			{
				value = new Value(ch, ValueType.Number);
				return true;
			}

			if (ch == '\'')
			{
				value = new Value(ch, ValueType.String);
				return true;
			}

			if (ch == '{')
			{
				value = new Value(ch, ValueType.DateTime);
				return true;
			}

			return false;
		}

		/*----------------------------------------------------------------------------
		    %%Function: FinishParse
		    %%Qualified: PostfixText.Parser.Value.FinishParse
		    
		    Finish this value parse (propagate sbValue to value, free sbValue)
		----------------------------------------------------------------------------*/
		private void FinishParse()
		{
			m_value = m_sbValue.ToString();
			m_sbValue = null;
		}

		const string operators = "&||!=:<>";

		/*----------------------------------------------------------------------------
		    %%Function: ParseNextValueChar
		    %%Qualified: PostfixText.Parser.Value.ParseNextValueChar
		    
		    Parse the next value character. Return true to continue parsing this
		    value, otherwise finish the value and return whether to unget this 
		    character or not
		----------------------------------------------------------------------------*/
		public bool ParseNextValueChar(char ch, out bool fUnget)
		{
			fUnget = false;

			switch (m_type)
			{
				case ValueType.DateTime:
				{
					if (ch == '}')
					{
						FinishParse();
						return false;
					}

					m_sbValue.Append(ch);
					return true;
				}

				case ValueType.Number:
					if (!char.IsDigit(ch))
					{
						if (char.IsWhiteSpace(ch) || ch == '\0')
						{
							FinishParse();
							return false;
						}

						if (operators.IndexOf(ch) != -1) // this terminates our parse and we push it back
						{
							fUnget = true;
							FinishParse();
							return false;
						}

						throw new Exception($"encountered non-digit {ch} while parsing number without intervening whitespace");
					}

					m_sbValue.Append(ch);
					return true;
				case ValueType.String:
					if (m_fParsingEscape)
					{
						// only allow escaped literal characters (for now)
						if (ch != '\'')
							throw new Exception($"only support escaped literal characters. encountered {ch}");

						m_fParsingEscape = false;
					}
					else if (ch == '\'')
					{
						// done with parse
						FinishParse();
						return false;
					}

					if (ch == '\\')
					{
						m_fParsingEscape = true;
						return true; // eat the escape character
					}

					// otherwise, add this character (including the fallthrough from the escaped literal)
					m_sbValue.Append(ch);
					return true;
				case ValueType.Field:
				{
					if (ch == ']')
					{
						FinishParse();
						return false;
					}

					m_sbValue.Append(ch);
					return true;
				}
			}

			throw new Exception("unknown value type");
		}

		#endregion

		#region Evaluation

		public string GetValueString(PostfixText.IValueClient valueClient)
		{
			if (m_type != ValueType.Field)
				return m_value;

			return valueClient.GetStringFromField(m_value);
		}

		public DateTime GetValueDateTime(PostfixText.IValueClient valueClient)
		{
			if (m_type != ValueType.Field)
			{
				if (m_dttmValueCache == null)
					m_dttmValueCache = DateTime.Parse(m_value);

				return m_dttmValueCache.Value;
			}

			DateTime? dttm = valueClient.GetDateTimeFromField(m_value);

			if (dttm == null)
				return DateTime.MinValue;

			return dttm.Value;
		}

		public int GetValueNumber(PostfixText.IValueClient valueClient)
		{
			if (m_type != ValueType.Field)
			{
				if (m_nValueCache == null)
					m_nValueCache = Int32.Parse(m_value);

				return m_nValueCache.Value;
			}

			int? n = valueClient.GetNumberFromField(m_value);

			if (n == null)
				return 0;

			return n.Value;
		}

		public bool FDoComparison(PostfixText.IValueClient valueClient, ComparisonOperator.Op opCompare, Value rhs)
		{
			int nCmp;

			opCompare = ComparisonOperator.OpCompareGenericFromOpCompare(opCompare, out bool fNoCase);

			if (m_type != rhs.m_type && m_type != ValueType.Field && rhs.m_type != ValueType.Field)
				throw new Exception("cannot evaluate dissimilar value types");

			Value.ValueType typeForCompare = m_type;

			// need to figure out what kind of comparison to do. Fields will be inferred from the
			// other value's type. If both values are fields, then its a string comparison

			if (typeForCompare == ValueType.Field)
				typeForCompare = valueClient.GetFieldValueType(m_value);

			if (typeForCompare == ValueType.Field)
				typeForCompare = rhs.m_type;

			if (typeForCompare == ValueType.Field)
				typeForCompare = valueClient.GetFieldValueType(rhs._Value);
			
			if (typeForCompare == ValueType.Field)
				typeForCompare = ValueType.String;

			if (typeForCompare == ValueType.String)
				nCmp = System.String.Compare(GetValueString(valueClient), rhs.GetValueString(valueClient), fNoCase);
			else if (typeForCompare == ValueType.DateTime)
				nCmp = DateTime.Compare(GetValueDateTime(valueClient), rhs.GetValueDateTime(valueClient));
			else if (typeForCompare == ValueType.Number)
				nCmp = GetValueNumber(valueClient) - rhs.GetValueNumber(valueClient);
			else
				throw new Exception("unkown value type in comparison");

			switch (opCompare)
			{
				case ComparisonOperator.Op.Eq:
					return nCmp == 0;
				case ComparisonOperator.Op.SEq:
					return nCmp == 0;
				case ComparisonOperator.Op.Ne:
					return nCmp != 0;
				case ComparisonOperator.Op.SNe:
					return nCmp == 0;
				case ComparisonOperator.Op.Gt:
					return nCmp > 0;
				case ComparisonOperator.Op.Gte:
					return nCmp >= 0;
				case ComparisonOperator.Op.Lt:
					return nCmp < 0;
				case ComparisonOperator.Op.Lte:
					return nCmp <= 0;
			}

			throw new Exception("unknown op for compare");
		}

		#endregion

		public override string ToString()
		{
			switch (m_type)
			{
			case ValueType.String:
				return $"'{m_value}'";
			case ValueType.Number:
				return m_value;
			case ValueType.DateTime:
				return $"{{{m_value}}}";
			case ValueType.Field:
				return $"[{m_value}]";
			default:
				throw new Exception("unknown type in tostring for value");
			}
		}
	}
}
