using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace TCore.PostfixText
{
    public interface IParserClient
    {
        bool FGetNextChar(out char ch);
    }

    // turns an input stream into a compiled clause
    public partial class Parser
    {
        public static Clause BuildClause(IParserClient client)
        {
            Clause clause;

            char ch;
            if (!client.FGetNextChar(out ch))
                return null;

            if (!Parser.Clause.FAcceptParseStart(ch, out clause))
                return null;

            while (client.FGetNextChar(out ch))
            {
                if (!clause.ParseNextValueChar(ch, out bool fUnget))
                    return clause;
            }

            if (!clause.FFinishParse())
                return null;
            return clause;
        }
    }

    public class StringParserClient : IParserClient
    {
        private string source;
        private int ich = 0;

        public StringParserClient(string s)
        {
            source = s;
        }

        public bool FGetNextChar(out char ch)
        {
            ch = '\0';

            if (ich >= source.Length)
                return false;

            ch = source[ich++];
            return true;
        }
    }

    public class StringArrayParserClient : IParserClient
    {
        private IEnumerator<string> sourceLines;
        private int ich = 0;
        private int cchMax = 0;

        public StringArrayParserClient(IEnumerable<string> sourceLinesIn)
        {
            sourceLines = sourceLinesIn.GetEnumerator();
            if (!sourceLines.MoveNext())
                cchMax = 0;
            else
                cchMax = sourceLines.Current.Length;
        }

        public bool FGetNextChar(out char ch)
        {
            ch = '\0';

            while (ich >= cchMax)
            {
                if (!sourceLines.MoveNext())
                    return false;

                cchMax = sourceLines.Current.Length;
                ich = 0;
            }

            ch = sourceLines.Current[ich++];
            return true;
        }
    }
}
