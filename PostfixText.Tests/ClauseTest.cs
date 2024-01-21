using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace TCore.PostfixText.Tests
{
    [TestFixture]
    public class ClauseTest
    {
        static Clause CreateClauseForString(string s)
        {
            if (!Clause.FAcceptParseStart(s[0], out Clause clause))
                return null;

            int ich = 1;

            while (ich < s.Length && clause.ParseNextValueChar(s[ich++], out bool fUnget))
                ;

            if (ich >= s.Length)
            {
                if (clause.FFinishParse())
                    return clause;

                return null;
            }

            return clause;
        }

        [Test]
        public static void ClauseTest_SingleExpression()
        {
            Clause clause = CreateClauseForString("[foo]<1");

            Assert.AreEqual(1, clause.m_items.Count);
            Assert.AreEqual(Clause.Item.Type.Expression, clause.m_items[0].ItemType);
            Assert.AreEqual(Value.ValueType.Field, clause.m_items[0].ItemExpression.LHS.m_type);
            Assert.AreEqual("foo", clause.m_items[0].ItemExpression.LHS.m_value);

            Assert.AreEqual(Value.ValueType.Number, clause.m_items[0].ItemExpression.RHS.m_type);
            Assert.AreEqual("1", clause.m_items[0].ItemExpression.RHS.m_value);
        }

        static void AssertTwoExpressionsCaseValid(Clause clause)
        {
            Assert.AreEqual(3, clause.m_items.Count);

            Assert.AreEqual(Clause.Item.Type.Expression, clause.m_items[0].ItemType);
            Assert.AreEqual(Value.ValueType.Field, clause.m_items[0].ItemExpression.LHS.m_type);
            Assert.AreEqual("bar", clause.m_items[0].ItemExpression.LHS.m_value);
            Assert.AreEqual(Value.ValueType.DateTime, clause.m_items[0].ItemExpression.RHS.m_type);
            Assert.AreEqual("123", clause.m_items[0].ItemExpression.RHS.m_value);

            Assert.AreEqual(Clause.Item.Type.Expression, clause.m_items[1].ItemType);
            Assert.AreEqual(Value.ValueType.Field, clause.m_items[1].ItemExpression.LHS.m_type);
            Assert.AreEqual("foo", clause.m_items[1].ItemExpression.LHS.m_value);
            Assert.AreEqual(Value.ValueType.Number, clause.m_items[1].ItemExpression.RHS.m_type);
            Assert.AreEqual("1", clause.m_items[1].ItemExpression.RHS.m_value);

            Assert.AreEqual(Clause.Item.Type.Operation, clause.m_items[2].ItemType);
            Assert.AreEqual(PostfixOperator.Op.And, clause.m_items[2].ItemOp.Operator);
        }

        [Test]
        public static void ClauseTest_TwoExpressions()
        {
            Clause clause;

            clause = CreateClauseForString("[bar]=={123}[foo]<1&&");
            AssertTwoExpressionsCaseValid(clause);

            clause = CreateClauseForString("[bar] == {123}[foo]<1&&");
            AssertTwoExpressionsCaseValid(clause);

            clause = CreateClauseForString("[bar]=={123} [foo]<1&&");
            AssertTwoExpressionsCaseValid(clause);

            clause = CreateClauseForString("[bar]=={123}\n[foo]<1\n&&");
            AssertTwoExpressionsCaseValid(clause);

            clause = CreateClauseForString("[bar]=={123} [foo]<1 &&");
            AssertTwoExpressionsCaseValid(clause);
        }

        [TestCase("[bar]   ==   123  ", "[bar] == 123 ")]
        [TestCase("[bar]   ==   123||  '2' :!= '3'  ", "[bar] == 123 || '2' :!= '3' ")]
        [Test]
        public static void Test_ToString(string sParse, string sToStringExpected)
        {
            Clause clause = CreateClauseForString(sParse);

            Assert.AreEqual(sToStringExpected, clause.ToString());
        }
    }
}
