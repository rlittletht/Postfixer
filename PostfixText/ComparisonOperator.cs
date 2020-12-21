using System;
using System.Collections.Generic;
using System.Text;

namespace PostfixText
{
    public partial class Parser
    {
        public class ComparisonOperator
        {
            public enum CmpOp
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

            public CmpOp Operator { get; set; }

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

                        Operator = m_fBuildingCaseInsensitive ? CmpOp.SEq : CmpOp.Eq;
                        return false;
                    case '<':
                        if (ch != '=')
                        {
                            Operator = m_fBuildingCaseInsensitive ? CmpOp.SLt : CmpOp.Lt;
                            fUnget = true; // we didn't process this char
                            return false;
                        }

                        Operator = m_fBuildingCaseInsensitive ? CmpOp.SLte : CmpOp.Lte;
                        return false;
                    case '>':
                        if (ch != '=')
                        {
                            Operator = m_fBuildingCaseInsensitive ? CmpOp.SGt : CmpOp.Gt;
                            fUnget = true; // we didn't process this char
                            return false;
                        }

                        Operator = m_fBuildingCaseInsensitive ? CmpOp.SGte : CmpOp.Gte;
                        return false;
                    case '!':
                        if (ch != '=')
                            throw new Exception($"ComparisonOperator.Parse: {ch} illegal after '!'");

                        Operator = m_fBuildingCaseInsensitive ? CmpOp.SNe : CmpOp.Ne;
                        return false;
                }
                throw new Exception($"unknown internal state in ComparisonOperator parse: {m_chLast}");
            }
        }
    }
}
