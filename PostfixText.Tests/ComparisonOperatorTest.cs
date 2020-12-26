using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace TCore.PostfixText.Tests
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
			bool fActual = ComparisonOperator.FAcceptParseStart(chValue, out ComparisonOperator op);

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
			ComparisonOperator.FAcceptParseStart(chLeading, out ComparisonOperator op);

			Assert.AreEqual(fExpected, op.ParseNextValueChar(chNext, out bool fUngetActual));
			Assert.AreEqual(fUngetExpected, fUngetActual);
		}

		[TestCase(":< ", true, ComparisonOperator.Op.SLt)]
		[TestCase(":<= ", false, ComparisonOperator.Op.SLte)]
		[TestCase(":> ", true, ComparisonOperator.Op.SGt)]
		[TestCase(":>=", false, ComparisonOperator.Op.SGte)]
		[TestCase(":==", false, ComparisonOperator.Op.SEq)]
		[TestCase(":!=", false, ComparisonOperator.Op.SNe)]
		[TestCase("< ", true, ComparisonOperator.Op.Lt)]
		[TestCase("<= ", false, ComparisonOperator.Op.Lte)]
		[TestCase("> ", true, ComparisonOperator.Op.Gt)]
		[TestCase(">= ", false, ComparisonOperator.Op.Gte)]
		[TestCase("!= ", false, ComparisonOperator.Op.Ne)]
		[TestCase("== ", false, ComparisonOperator.Op.Eq)]
		[Test]
		public static void FParseComparisionOperator_CompleteValue(string sParse, bool fUngetExpected, ComparisonOperator.Op opExpected)
		{
			ComparisonOperator.FAcceptParseStart(sParse[0], out ComparisonOperator op);

			int ich = 1;
			bool fUngetActual;
			while (op.ParseNextValueChar(sParse[ich++], out fUngetActual))
				;

			Assert.AreEqual(fUngetExpected, fUngetActual);
			Assert.AreEqual(opExpected, op.Operator);
		}
	}

}