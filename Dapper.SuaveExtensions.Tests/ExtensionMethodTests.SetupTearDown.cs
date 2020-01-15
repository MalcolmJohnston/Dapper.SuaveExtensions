using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;

namespace Dapper.SuaveExtensions.Tests
{
    [TestFixture]
    public partial class ExtensionMethodTests
    {
        /// <summary>
        /// Initialises routine for each Test Fixture
        /// </summary>
        [SetUp]
        public void Setup()
        {
            LocalDbTestHelper.CreateTestDatabase(TestContext.CurrentContext.Test.FullName);
        }

        /// <summary>
        /// Tear down routine for each Test Fixture
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            LocalDbTestHelper.DeleteTestDatabase(TestContext.CurrentContext.Test.FullName);
        }
    }
}
