using System;
using System.Collections.Generic;
using System.Text;

namespace PostfixText
{
    // The clause is the entire compiled PostFix string

    /*
     * CLAUSE:
     * [EXPRESSION] [EXPRESSION] [BOOLEAN_OP]
     *
     * EXPRESSION:
     * [TOKEN] [COMPARISON_OP] [TOKEN]
     *
     * VALUE:
     *   [a-zA-Z_][\w\[\]]+     // field value
     * | \d+                    // constant number value
     * | '.*'                   // constant string value (including string dates)
     *
     * COMPARISON_OP:
     * '==' | '!=' | '<' | '<=' | '>=' | '>' | ':=' | ':<' | ':<=' | ':>' | ':>'
     *
     * BOOLEAN_OP
     * 'AND' | 'OR'
     */
    public partial class Parser
    {
        public class Clause
        {

        }
    }
}
