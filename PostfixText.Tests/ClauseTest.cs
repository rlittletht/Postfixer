using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace PostfixText.Tests
{
    [TestFixture]
    public class ClauseTest
    {
        static Parser.Clause CreateClauseForString(string s)
        {
            if (!Parser.Clause.FAcceptParseStart(s[0], out Parser.Clause clause))
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
            Parser.Clause clause = CreateClauseForString("[foo]<1");

            Assert.AreEqual(1, clause.m_items.Count);
            Assert.AreEqual(Parser.Clause.Item.Type.Expression, clause.m_items[0].ItemType);
            Assert.AreEqual(Parser.Value.ValueType.Field, clause.m_items[0].ItemExpression.m_lhs.m_type);
            Assert.AreEqual("foo", clause.m_items[0].ItemExpression.m_lhs.m_value);

            Assert.AreEqual(Parser.Value.ValueType.Number, clause.m_items[0].ItemExpression.m_rhs.m_type);
            Assert.AreEqual("1", clause.m_items[0].ItemExpression.m_rhs.m_value);
        }

        static void AssertTwoExpressionsCaseValid(Parser.Clause clause)
        {
            Assert.AreEqual(3, clause.m_items.Count);

            Assert.AreEqual(Parser.Clause.Item.Type.Expression, clause.m_items[0].ItemType);
            Assert.AreEqual(Parser.Value.ValueType.Field, clause.m_items[0].ItemExpression.m_lhs.m_type);
            Assert.AreEqual("bar", clause.m_items[0].ItemExpression.m_lhs.m_value);
            Assert.AreEqual(Parser.Value.ValueType.DateTime, clause.m_items[0].ItemExpression.m_rhs.m_type);
            Assert.AreEqual("123", clause.m_items[0].ItemExpression.m_rhs.m_value);

            Assert.AreEqual(Parser.Clause.Item.Type.Expression, clause.m_items[1].ItemType);
            Assert.AreEqual(Parser.Value.ValueType.Field, clause.m_items[1].ItemExpression.m_lhs.m_type);
            Assert.AreEqual("foo", clause.m_items[1].ItemExpression.m_lhs.m_value);
            Assert.AreEqual(Parser.Value.ValueType.Number, clause.m_items[1].ItemExpression.m_rhs.m_type);
            Assert.AreEqual("1", clause.m_items[1].ItemExpression.m_rhs.m_value);

            Assert.AreEqual(Parser.Clause.Item.Type.Operation, clause.m_items[2].ItemType);
            Assert.AreEqual(Parser.PostfixOperator.Op.And, clause.m_items[2].ItemOp.Operator);
        }
        [Test]
        public static void ClauseTest_TwoExpressions()
        {
            Parser.Clause clause;

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

    }
}
