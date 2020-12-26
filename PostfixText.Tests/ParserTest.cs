using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace TCore.PostfixText.Tests
{
	[TestFixture]
	public class ParserTest
	{
		[TestCase("[field]==123", 1)]
		[TestCase("[field]=={1} ", 1)]
		[Test]
		public static void TestParseString(string source, int cClauseExpected)
		{
			StringParserClient client = new StringParserClient(source);
			Clause clause = Parser.BuildClause(client);

			Assert.AreEqual(cClauseExpected, clause.m_items.Count);
		}

		[TestCase(new[] {"[foo]", "==", "123"}, 1)]
		[TestCase(new[] {"[foo", "]==", "123"}, 1)]
		[TestCase(new[] {"[foo]", "==", "123"}, 1)]
		[Test]
		public static void TestParseArrayString(string[] sourceLines, int cClauseExpected)
		{
			StringArrayParserClient client = new StringArrayParserClient(sourceLines);
			Clause clause = Parser.BuildClause(client);

			Assert.AreEqual(cClauseExpected, clause.m_items.Count);
		}

	}
}