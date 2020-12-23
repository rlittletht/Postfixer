using System;
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
        // Parser.Clause 
        internal static bool AlwaysTrue()
        {
            return true;
        }
    }
}
