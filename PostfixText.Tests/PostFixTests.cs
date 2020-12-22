using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
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
    }
}
