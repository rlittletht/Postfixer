using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;

namespace TCore.PostfixText.Tests
{
    [TestFixture]
    public class ValueTests
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
            bool fActual = Value.FAcceptParseStart(chValue, out Value value);

            Assert.AreEqual(fExpected, fActual);
            Assert.AreEqual(fExpected, value != null);
        }

        [TestCase('1', '2', true, false, "12")]
        [TestCase('1', '&', false, true, null)] // null because it was propagated to the final string
        [TestCase('1', '=', false, true, null)] // null because it was propagated to the final string
        [TestCase('1', ':', false, true, null)] // null because it was propagated to the final string
        [TestCase('1', '!', false, true, null)] // null because it was propagated to the final string
        [TestCase('1', '<', false, true, null)] // null because it was propagated to the final string
        [TestCase('1', '>', false, true, null)] // null because it was propagated to the final string
        [TestCase('1', ' ', false, false, null)] // null because it was propagated to the final string
        [TestCase('[', 'D', true, false, "D")]
        [TestCase('{', '2', true, false, "2")]
        [TestCase('\'', 'a', true, false, "a")]
        [Test]
        public static void FParseValue_SingleChar(char chLeading, char chNext, bool fExpected, bool fUngetExpected, string sExpected)
        {
            Value.FAcceptParseStart(chLeading, out Value value);

            Assert.AreEqual(fExpected, value.ParseNextValueChar(chNext, out bool fUngetActual));
            Assert.AreEqual(fUngetExpected, fUngetActual);
            Assert.AreEqual(sExpected, value.m_sbValue?.ToString());
        }

        [TestCase("123 ", false, "123", Value.ValueType.Number)]
        [TestCase("123&&", true, "123", Value.ValueType.Number)]
        [TestCase("[field]", false, "field", Value.ValueType.Field)]
        [TestCase("[_fld_fld_fld_]", false, "_fld_fld_fld_", Value.ValueType.Field)]
        [TestCase("{123} ", false, "123", Value.ValueType.DateTime)]
        [TestCase("'foo'", false, "foo", Value.ValueType.String)]
        [TestCase("'\\\'foo'", false, "'foo", Value.ValueType.String)]
        [TestCase("'foo\\\''", false, "foo'", Value.ValueType.String)]
        [Test]
        public static void FParseValue_CompleteValue(string sParse, bool fUngetExpected, string sExpected, ValueType typeExpected)
        {
            Value.FAcceptParseStart(sParse[0], out Value value);

            int ich = 1;
            bool fUngetActual = false;
            while (value.ParseNextValueChar(sParse[ich++], out fUngetActual))
                ;

            Assert.AreEqual(fUngetExpected, fUngetActual);
            Assert.AreEqual(sExpected, value.m_value);
            Assert.AreEqual(typeExpected, value.m_type);
        }

        public class ValueContextForText : PostfixText.IValueClient
        {
            // format is _[FieldToLookup]_[FieldAvailableToMatch]_Value_
            // (we have the field twice to allow simulating both a match for 
            // the field, and a non-match
            public string GetStringFromField(string sField)
            {
                string sExpression = @"_([^_]+)_([^_]+)_([^_]+)_";
                Regex rex = new Regex(sExpression);

                Match match = rex.Match(sField);
                if (!match.Success)
                    return null;

                if (match.Groups[1].Value != match.Groups[2].Value)
                    return null; // field lookup failed

                return match.Groups[3].Value;
            }

            public int? GetNumberFromField(string sField)
            {
                string sValue = GetStringFromField(sField);
                if (sValue != null)
                    return Int32.Parse(sValue);

                return null;
            }

            public DateTime? GetDateTimeFromField(string sField)
            {
                string sValue = GetStringFromField(sField);
                if (sValue != null)
                    return DateTime.Parse(sValue);

                return null;
            }

            public Value.ValueType GetFieldValueType(string sField)
            {
                // we don't care, so just return Field again -- it will keep looking
                return Value.ValueType.Field;
            }
        }

        [TestCase("123 ", "123")]
        [TestCase("'123'", "'123'")]
        [TestCase("{123}", "{123}")]
        [TestCase("[12 123]", "[12 123]")]
        [Test]
        public static void Test_ToString(string sParse, string sToStringExpected)
        {
            Value.FAcceptParseStart(sParse[0], out Value value);

            int ich = 1;
            bool fUngetActual = false;
            while (value.ParseNextValueChar(sParse[ich++], out fUngetActual))
                ;

            Assert.AreEqual(sToStringExpected, value.ToString());
        }
    }
}
