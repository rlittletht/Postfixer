using System;
using System.Collections.Generic;
using System.Text;

namespace PostfixText
{
    public partial class Parser
    {
        public class PostfixOperator
        {
            public enum Op
            {
                And,
                Or
            }

            internal char m_chLast;

            public Op Operator { get; set; }

            public PostfixOperator(char ch)
            {
                m_chLast = ch;
            }

            public static bool FAcceptParseStart(char ch, out PostfixOperator cmpOperator)
            {
                cmpOperator = null;
                if (ch == '&' || ch == '|')
                {
                    cmpOperator = new PostfixOperator(ch);
                    return true;
                }

                return false;
            }

            public bool ParseNextValueChar(char ch, out bool fUnget)
            {
                fUnget = false;

                if (m_chLast != ch)
                    throw new Exception($"illegal character {ch} trying to parse PostfixOperator {m_chLast}");

                if (m_chLast == '&')
                {
                    Operator = Op.And;
                    return false;
                }

                Operator = Op.Or;
                return false;
            }

        }
    }
}
