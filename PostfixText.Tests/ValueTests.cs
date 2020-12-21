using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace PostfixText.Tests
{
    [TestFixture]
    public partial class ValueTests
    {
        [TestCase('[', true)] // leading field
        [TestCase('{', true)] // leading date
        [TestCase('\'', true)] // leading literal
        [TestCase('0', true)] // leading number
        [TestCase('1', true)] // leading number
        [TestCase('9', true)] // leading number
        [TestCase(' ', false)] // not leading
        [TestCase('_', false)] // not leading
        [TestCase('.', false)] // not leading
        [TestCase(',', false)] // not leading
        [TestCase('\n', false)] // not leading
        [Test]
        public static void FAcceptValueStart(char chValue, bool fExpected)
        {
            bool fActual = Parser.Value.FAcceptValueStart(chValue, out Parser.Value value);

            Assert.AreEqual(fExpected, fActual);
            Assert.AreEqual(fExpected, value != null);
        }

        [TestCase('1', '2', true, false, "12")]
        [TestCase('1', ' ', false, false, null)] // null because it was propagated to the final string
        [TestCase('[', 'D', true, false, "D")]
        [TestCase('{', '2', true, false, "2")]
        [TestCase('\'', 'a', true, false, "a")]
        [Test]
        public static void FParseValue_SingleChar(char chLeading, char chNext, bool fExpected, bool fUngetExpected, string sExpected)
        {
            Parser.Value.FAcceptValueStart(chLeading, out Parser.Value value);

            Assert.AreEqual(fExpected, value.ParseNextValueChar(chNext, out bool fUngetActual));
            Assert.AreEqual(fUngetExpected, fUngetActual);
            Assert.AreEqual(sExpected, value.m_sbValue?.ToString());
        }

        [TestCase("123 ", false, "123")]
        [TestCase("[field]", false, "field")]
        [TestCase("{123} ", false, "123")]
        [TestCase("'foo'", false, "foo")]
        [TestCase("'\\\'foo'", false, "'foo")]
        [TestCase("'foo\\\''", false, "foo'")]
        [Test]
        public static void FParseValue_CompleteValue(string sParse, bool fUngetExpected, string sExpected)
        {
            Parser.Value.FAcceptValueStart(sParse[0], out Parser.Value value);

            int ich = 1;
            bool fUngetActual = false;
            while (value.ParseNextValueChar(sParse[ich++], out fUngetActual))
                ;

            Assert.AreEqual(fUngetExpected, fUngetActual);
            Assert.AreEqual(sExpected, value.m_value);
        }
    }
}
