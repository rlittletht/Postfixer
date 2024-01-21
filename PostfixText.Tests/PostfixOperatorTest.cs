using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace TCore.PostfixText.Tests
{
    [TestFixture]
    public class PostfixOperatorTest
    {
        [TestCase('&', true)] // &&
        [TestCase('|', true)] // ||
        [TestCase(' ', false)] // not leading
        [TestCase('a', false)] // not leading
        [Test]
        public static void FAcceptParseStart(char chValue, bool fExpected)
        {
            bool fActual = PostfixOperator.FAcceptParseStart(chValue, out PostfixOperator op);

            Assert.AreEqual(fExpected, fActual);
            Assert.AreEqual(fExpected, op != null);
        }

        [TestCase('&', '&', false, false)] // &&
        [TestCase('|', '|', false, false)] // ||
        [Test]
        public static void FParseComparisionOperator_SingleChar(char chLeading, char chNext, bool fExpected, bool fUngetExpected)
        {
            PostfixOperator.FAcceptParseStart(chLeading, out PostfixOperator op);

            Assert.AreEqual(fExpected, op.ParseNextValueChar(chNext, out bool fUngetActual));
            Assert.AreEqual(fUngetExpected, fUngetActual);
        }

        [TestCase("&&", false, PostfixOperator.Op.And)]
        [TestCase("||", false, PostfixOperator.Op.Or)]
        [Test]
        public static void FParseComparisionOperator_CompleteValue(string sParse, bool fUngetExpected, PostfixOperator.Op opExpected)
        {
            PostfixOperator.FAcceptParseStart(sParse[0], out PostfixOperator op);

            int ich = 1;
            bool fUngetActual;
            while (op.ParseNextValueChar(sParse[ich++], out fUngetActual))
                ;

            Assert.AreEqual(fUngetExpected, fUngetActual);
            Assert.AreEqual(opExpected, op.Operator);
        }

        [TestCase("&&", "&&")]
        [TestCase("||", "||")]
        [Test]
        public static void Test_ToString(string sParse, string sToStringExpected)
        {
            PostfixOperator.FAcceptParseStart(sParse[0], out PostfixOperator op);

            int ich = 1;
            bool fUngetActual;
            while (op.ParseNextValueChar(sParse[ich++], out fUngetActual))
                ;

            Assert.AreEqual(sToStringExpected, op.ToString());
        }
    }
}
