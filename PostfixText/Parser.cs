using System;
using System.Collections.Generic;
using System.Text;

namespace PostfixText
{
    public interface IParserClient
    {
        char GetNextChar();
        void Unget();
    }

    // turns an input stream into a compiled clause
    public partial class Parser
    {
        enum State
        {
            Initial,
            PreToken,
            Token,
            PreComparisonOp,
            ComparisonOn,
        }
    }
}
