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
        }

        private Clause m_clause;

        public PostfixText() { }

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

        internal static bool AlwaysTrue()
        {
            return true;
        }
    }
}
