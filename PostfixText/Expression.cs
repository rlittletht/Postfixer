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

            private CmpOp m_comparisonOp;

            public static bool FAcceptParseStart(char ch, out Expression expression)
            {
                expression = null;

                return false;
            }
        }
    }
}
