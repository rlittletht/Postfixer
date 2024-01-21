using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Internal;
using TCore.PostfixText;

/*
 * This tests the netstandard2 component PostfixText.
 *
 * netstandard2 by definition have no host components included in them, so there
 * is no way to use nunit directly in them. in order to still effectively be able
 * to test them using nunit, we expose the internals of the class to this module
 * (which is a standard .net framework shared dll, so it can be invoked to run
 * nunit tests).
 *
 * we expose the internals to this class using
 *      [assembly: InternalsVisibleTo("PostfixText.Tests")]
 * which allows everything in this assembly to see the internals (though not
 * private) members of the class. to make this work, make sure the assembly
 * name of the test module matches what you are declaring in the above
 * directive.
 */
namespace TCore.PostfixText.Tests
{
    [TestFixture]
    public partial class PostFixTests
    {
        [Test]
        public static void TestAlwaysPass()
        {
            Assert.IsTrue(PostfixText.AlwaysTrue());
        }

        [Test]
        public static void TestExpressionEvaluate_NoPostfixOperator()
        {
            PostfixText pf = PostfixText.CreateFromParserClient(new StringParserClient("[_s1_s1_foo_]=='foo'"));

            ValueTests.ValueContextForText valueClient = new ValueTests.ValueContextForText();
            Assert.IsTrue(pf.FEvaluate(valueClient));
        }

        [Test]
        public static void TestExpressionEvaluate_SimpleStringWithAnd()
        {
            PostfixText pf = PostfixText.CreateFromParserClient(new StringParserClient("[_s1_s1_foo_]=='foo' 'bar':=='BAR' &&"));

            ValueTests.ValueContextForText valueClient = new ValueTests.ValueContextForText();
            Assert.IsTrue(pf.FEvaluate(valueClient));
        }

        [Test]
        public static void TestExpressionEvaluate_SimpleStringWithOr()
        {
            PostfixText pf = PostfixText.CreateFromParserClient(new StringParserClient("[_s1_s1_foo_]=='foo' 'bar':=='BAR2' ||"));

            ValueTests.ValueContextForText valueClient = new ValueTests.ValueContextForText();
            Assert.IsTrue(pf.FEvaluate(valueClient));
        }

        [TestCase("[_s1_s1_2020-01-01_]=={2020-01-01}", true)] // Field comparing to date value
        [TestCase("[_s1_s1_20_]==20", true)] // Field comparing to number
        [TestCase("[_s1_s2_20_]==20", false)] // Field lookup fail number
        [TestCase("[_s1_s2_20_]=='20'", false)] // Field lookup fail string
        [TestCase("[_s1_s2_20_]=={2020-01-01}", false)] // Field lookup fail date
        [TestCase("[_s1_s1_20_]==[_s2_s2_20_]", true)] // Field comparison

        // test all of the comparison operators for all types...
        // strings
        [TestCase("'foo'=='foo'", true)]
        [TestCase("'foo'<='foo'", true)]
        [TestCase("'boo'<='foo'", true)]
        [TestCase("'foo'>='foo'", true)]
        [TestCase("'foo'>='boo'", true)]
        [TestCase("'foo'!='boo'", true)]
        [TestCase("'boo'<'foo'", true)]
        [TestCase("'foo'>'boo'", true)]
        [TestCase("'fOO':=='foo'", true)]
        [TestCase("'fOO':<='foo'", true)]
        [TestCase("'bOO':<='foo'", true)]
        [TestCase("'fOO':>='foo'", true)]
        [TestCase("'fOO':>='boo'", true)]
        [TestCase("'fOO':!='boo'", true)]
        [TestCase("'bOO':<'foo'", true)]
        [TestCase("'fOO':>'boo'", true)]

        // numbers
        [TestCase("123==123", true)]
        [TestCase("123<=123", true)]
        [TestCase("100<=123", true)]
        [TestCase("123>=123", true)]
        [TestCase("123>=100", true)]
        [TestCase("123!=100", true)]
        [TestCase("100 <123", true)]
        [TestCase("123 >100", true)]

        // datetime
        [TestCase("'{2020-01-02}'=='{2020-01-02}'", true)]
        [TestCase("'{2020-01-02}'<='{2020-01-02}'", true)]
        [TestCase("'{2020-01-01}'<='{2020-01-02}'", true)]
        [TestCase("'{2020-01-02}'>='{2020-01-02}'", true)]
        [TestCase("'{2020-01-02}'>='{2020-01-01}'", true)]
        [TestCase("'{2020-01-02}'!='{2020-01-01}'", true)]
        [TestCase("'{2020-01-01}'<'{2020-01-02}'", true)]
        [TestCase("'{2020-01-02}'>'{2020-01-01}'", true)]

        // Test the false results as well (only need this for one type)
        [TestCase("123!=123", false)]
        [TestCase("123>=124", false)]
        [TestCase("100>=123", false)]
        [TestCase("123<=122", false)]
        [TestCase("123<=100", false)]
        [TestCase("123==100", false)]
        [TestCase("100 >123", false)]
        [TestCase("123 <100", false)]

        // now some postfix expressions (using constant numbers for simplicity)
        [TestCase("2>1", true)] // true

        // establish the boolean operations work
        [TestCase("2>1 2>1 &&", true)] // true && true
        [TestCase("2>1 1>2 &&", false)] // true && false
        [TestCase("1>2 2>1 &&", false)] // false && true
        [TestCase("1>2 1>2 &&", false)] // false && false
        [TestCase("2>1 2>1 ||", true)] // true || true
        [TestCase("2>1 1>2 ||", true)] // true || false
        [TestCase("1>2 2>1 ||", true)] // false || true
        [TestCase("1>2 1>2 ||", false)] // false || false
        [TestCase("2>1 2>1 && 2>1 &&", true)] // (true && true) && true
        [TestCase("2>1 2>1 2>1 && &&", true)] // (true && (true && true))
        [Test]
        public static void TestExpressionEvaluation(string sExpressionIn, bool fExpected)
        {
            PostfixText pf = PostfixText.CreateFromParserClient(new StringParserClient(sExpressionIn));

            ValueTests.ValueContextForText valueClient = new ValueTests.ValueContextForText();
            Assert.AreEqual(fExpected, pf.FEvaluate(valueClient));
        }

        [TestCase("[bar]   ==   123  ", "[bar] == 123 ")]
        [TestCase("[bar]   ==   123||  '2' :!= '3'  ", "[bar] == 123 || '2' :!= '3' ")]
        [TestCase("    [bar]   ==   123  ", "[bar] == 123 ")]
        [TestCase("\n\n  [bar]   ==   123||  '2' :!= '3'  ", "[bar] == 123 || '2' :!= '3' ")]
        [Test]
        public static void Test_ToString(string sParse, string sToStringExpected)
        {
            PostfixText pf = PostfixText.CreateFromParserClient(new StringParserClient(sParse));

            Assert.AreEqual(sToStringExpected, pf.ToString());
        }
    }
}
