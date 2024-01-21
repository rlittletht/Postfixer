using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("PostfixText.Tests")]

namespace TCore.PostfixText
{
	// A PostfixText object holds a compiled clause (either obtained
	// by parsing a string, or by constructing directly). 

	// FEvaluate() will evaluate the clause, using the provided 
	// interface to resolve variables
	public class PostfixText
	{
		public interface IValueClient
		{
			string GetStringFromField(string field);
			int? GetNumberFromField(string field);
			DateTime? GetDateTimeFromField(string field);
			
			// some fields have a strong opinion about their type
			Value.ValueType GetFieldValueType(string sField);
			
		}

		private Clause m_clause;

		public Clause Clause => m_clause;
		
		public PostfixText(Clause clause)
		{
			m_clause = clause;
		}

		public PostfixText()
		{
			m_clause = new Clause();
		}
		
		public static PostfixText CreateFromParserClient(IParserClient client)
		{
			Clause clause = Parser.BuildClause(client);

			if (clause == null)
				return null;

			PostfixText postfix = new PostfixText();
			postfix.m_clause = clause;

			return postfix;
		}

		public bool FEvaluate(IValueClient valueClient)
		{
			return m_clause.FEvaluate(valueClient);
		}

		public override string ToString()
		{
			return m_clause.ToString();
		}

		public string[] ToStrings()
		{
			return m_clause.ToStrings();
		}
		
		public PostfixText Clone()
		{
			return CreateFromParserClient(new StringParserClient(this.ToString()));
		}

		public void AddExpression(Expression expression)
		{
			m_clause.AddExpression(expression);
		}
		
		public void AddOperator(PostfixOperator op)
		{
			m_clause.AddOperator(op);
		}
		
		internal static bool AlwaysTrue()
		{
			return true;
		}
	}
}
