﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace TCore.PostfixText.Tests
{
    [TestFixture]
    public class ExpressionTests
    {
        // we already tested all of the sub components, so we just have to test
        // how they stitch together

        [TestCase("[foo]=={123}", "foo", Value.ValueType.Field, ComparisonOperator.Op.Eq, "123", Value.ValueType.DateTime, false) ]
        [TestCase("[foo]<={123}", "foo", Value.ValueType.Field, ComparisonOperator.Op.Lte, "123", Value.ValueType.DateTime, false)]
        [TestCase("[foo]<{123}", "foo", Value.ValueType.Field, ComparisonOperator.Op.Lt, "123", Value.ValueType.DateTime, false)]
        [TestCase("[foo]  <=  {123}", "foo", Value.ValueType.Field, ComparisonOperator.Op.Lte, "123", Value.ValueType.DateTime, false)]
        [TestCase("[foo]  <  {123}", "foo", Value.ValueType.Field, ComparisonOperator.Op.Lt, "123", Value.ValueType.DateTime, false)]
        [TestCase("[foo]  :<  {123}", "foo", Value.ValueType.Field, ComparisonOperator.Op.SLt, "123", Value.ValueType.DateTime, false)]
        [TestCase("[foo]  :== {123}", "foo", Value.ValueType.Field, ComparisonOperator.Op.SEq, "123", Value.ValueType.DateTime, false)]
        [Test]
        public static void TestExpressionParserFull(string sInput, string sLhsExpected, ValueType lhsTypeExpected, ComparisonOperator.Op opExpected, string sRhsExpected, ValueType rhsTypeExpected, bool fUngetExpected)
        {
            Assert.IsTrue(Expression.FAcceptParseStart(sInput[0], out Expression expression));

            int ich = 1;
            bool fUngetActual = false;

            while (expression.ParseNextValueChar(sInput[ich++], out fUngetActual))
                ;

            Assert.AreEqual(sLhsExpected, expression.m_lhs.m_value);
            Assert.AreEqual(lhsTypeExpected, expression.m_lhs.m_type);
            Assert.AreEqual(opExpected, expression.m_comparisonOp.Operator);
            Assert.AreEqual(sRhsExpected, expression.m_rhs.m_value);
            Assert.AreEqual(rhsTypeExpected, expression.m_rhs.m_type);
            Assert.AreEqual(fUngetExpected, fUngetActual);
        }
    }
}