using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace PostfixText.Tests
{

    public class ComparisonOperatorTest
    {
        [TestCase(':', true)] // leading case insensitive compare
        [TestCase('<', true)] // <, <=
        [TestCase('>', true)] // >, >=
        [TestCase('!', true)] // !=
        [TestCase('=', true)] // ==
        [TestCase(' ', false)] // not leading
        [TestCase('a', false)] // not leading
        [TestCase('\'', false)] // not leading
        [TestCase(',', false)] // not leading
        [TestCase('\n', false)] // not leading
        [Test]
        public static void FAcceptParseStart(char chValue, bool fExpected)
        {
            bool fActual = Parser.ComparisonOperator.FAcceptParseStart(chValue, out Parser.ComparisonOperator op);

            Assert.AreEqual(fExpected, fActual);
            Assert.AreEqual(fExpected, op != null);
        }

        [TestCase(':', '<', true, false)] // leading case insensitive compare
        [TestCase(':', '>', true, false)] // leading case insensitive compare
        [TestCase(':', '!', true, false)] // leading case insensitive compare
        [TestCase(':', '=', true, false)] // leading case insensitive compare
        [TestCase('<', ' ', false, true)] // <
        [TestCase('<', '=', false, false)] // <=
        [TestCase('>', ' ', false, true)] // >
        [TestCase('>', '=', false, false)] // >=
        [TestCase('!', '=', false, false)] // !=
        [TestCase('=', '=', false, false)] // ==
        [Test]
        public static void FParseComparisionOperator_SingleChar(char chLeading, char chNext, bool fExpected, bool fUngetExpected)
        {
            Parser.ComparisonOperator.FAcceptParseStart(chLeading, out Parser.ComparisonOperator op);

            Assert.AreEqual(fExpected, op.ParseNextValueChar(chNext, out bool fUngetActual));
            Assert.AreEqual(fUngetExpected, fUngetActual);
        }

        [TestCase(":< ", true, Parser.ComparisonOperator.CmpOp.SLt) ]
        [TestCase(":<= ", false,Parser.ComparisonOperator.CmpOp.SLte) ]
        [TestCase(":> ", true, Parser.ComparisonOperator.CmpOp.SGt) ]
        [TestCase(":>=", false, Parser.ComparisonOperator.CmpOp.SGte) ]
        [TestCase(":==", false, Parser.ComparisonOperator.CmpOp.SEq) ]
        [TestCase(":!=", false, Parser.ComparisonOperator.CmpOp.SNe) ]
        [TestCase("< ", true,  Parser.ComparisonOperator.CmpOp.Lt) ]
        [TestCase("<= ", false, Parser.ComparisonOperator.CmpOp.Lte) ]
        [TestCase("> ", true,  Parser.ComparisonOperator.CmpOp.Gt) ]
        [TestCase(">= ", false, Parser.ComparisonOperator.CmpOp.Gte) ]
        [TestCase("!= ", false, Parser.ComparisonOperator.CmpOp.Ne) ]
        [TestCase("== ", false, Parser.ComparisonOperator.CmpOp.Eq) ]
        [Test]
        public static void FParseComparisionOperator_CompleteValue(string sParse, bool fUngetExpected, Parser.ComparisonOperator.CmpOp cmpOpExpected)
        {
            Parser.ComparisonOperator.FAcceptParseStart(sParse[0], out Parser.ComparisonOperator op);

            int ich = 1;
            bool fUngetActual;
            while (op.ParseNextValueChar(sParse[ich++], out fUngetActual))
                ;

            Assert.AreEqual(fUngetExpected, fUngetActual);
            Assert.AreEqual(cmpOpExpected, op.Operator);
        }
    }

}
