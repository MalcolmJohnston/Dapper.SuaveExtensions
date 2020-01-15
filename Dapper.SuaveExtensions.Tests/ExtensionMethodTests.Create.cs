using System;
using System.Data;
using System.Threading.Tasks;

using NUnit.Framework;
using Dapper.SuaveExtensions.Tests.Models;

namespace Dapper.SuaveExtensions.Tests
{
    [TestFixture]
    public partial class ExtensionMethodTests
    {
        /// <summary>
        /// Test that we can insert an entity that has single key which is an identity column.
        /// </summary>
        [Test]
        public async Task Insert_With_Identity()
        {
            using (IDbConnection connection = LocalDbTestHelper.OpenTestConnection(TestContext.CurrentContext.Test.FullName))
            {
                // Act
                City city = await connection.Create<City>(new City() { CityCode = "BRI", CityName = "Brighton", Area = "Sussex" })
                    .ConfigureAwait(false);

                // Assert
                Assert.Greater(city.CityId, 0);
                Assert.AreEqual("BRI", city.CityCode);
                Assert.AreEqual("Brighton", city.CityName);
                Assert.AreEqual("Sussex", city.Area);
            }
        }

        /// <summary>
        /// Test that we can insert an entity which has a single key that is manually derived
        /// </summary>
        [Test]
        public async Task Insert_With_Assigned_Key()
        {
            using (IDbConnection connection = LocalDbTestHelper.OpenTestConnection(TestContext.CurrentContext.Test.FullName))
            {
                // Act
                CityManual city = await connection.Create<CityManual>(new CityManual()
                {
                    CityCode = "BRI",
                    CityName = "Brighton"
                }).ConfigureAwait(false);

                // Assert
                Assert.AreEqual("BRI", city.CityCode);
                Assert.AreEqual("Brighton", city.CityName);
            }
        }

        /// <summary>
        /// Test that we can insert an entity which has a sequentially calculated key
        /// </summary>
        [Test]
        public async Task Insert_With_Sequential_Key()
        {
            using (IDbConnection connection = LocalDbTestHelper.OpenTestConnection(TestContext.CurrentContext.Test.FullName))
            {
                // Act
                CitySequential city = await connection.Create<CitySequential>(new CitySequential()
                {
                    CityCode = "BRI",
                    CityName = "Brighton"
                }).ConfigureAwait(false);

                // Assert
                Assert.Greater(city.CityId, 0);
                Assert.AreEqual("BRI", city.CityCode);
                Assert.AreEqual("Brighton", city.CityName);
            }
        }

        /// <summary>
        /// Test that we can insert an entity which has one manual key and one sequential key
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task Insert_With_Composite_Key_One_Assigned_And_One_Sequential()
        {
            using (IDbConnection connection = LocalDbTestHelper.OpenTestConnection(TestContext.CurrentContext.Test.FullName))
            {
                // Act
                AssignedAndSequential one = await connection.Create(new AssignedAndSequential() { AssignedId = 1, Heading = "One" });
                AssignedAndSequential two = await connection.Create(new AssignedAndSequential() { AssignedId = 1, Heading = "Two" });

                // Assert
                Assert.AreEqual(1, one.SequentialId);
                Assert.AreEqual(2, two.SequentialId);
            }
        }

        /// <summary>
        /// Test that we can insert an entity which has two manual keys and one sequential key
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task Insert_With_Composite_Key_Two_Assigned_And_One_Sequential()
        {
            using (IDbConnection connection = LocalDbTestHelper.OpenTestConnection(TestContext.CurrentContext.Test.FullName))
            {
                // Act
                AssignedPairAndSequential oneOneOne = await connection.Create(new AssignedPairAndSequential() { FirstAssignedId = 1, SecondAssignedId = 1, Heading = "One" });
                AssignedPairAndSequential oneOneTwo = await connection.Create(new AssignedPairAndSequential() { FirstAssignedId = 1, SecondAssignedId = 1, Heading = "Two" });
                AssignedPairAndSequential oneTwoOne = await connection.Create(new AssignedPairAndSequential() { FirstAssignedId = 1, SecondAssignedId = 2, Heading = "One" });

                // Assert
                Assert.AreEqual(1, oneOneOne.SequentialId);
                Assert.AreEqual(2, oneOneTwo.SequentialId);
                Assert.AreEqual(1, oneTwoOne.SequentialId);
            }
        }

        /// <summary>
        /// Test that insert and update date properties that have been marked as Date Stamps are automatically set
        /// to the time now on insert.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task Insert_With_Datestamp()
        {
            using (IDbConnection connection = LocalDbTestHelper.OpenTestConnection(TestContext.CurrentContext.Test.FullName))
            {
                // Act
                DateTime now = DateTime.Now;
                DateStamp row = await connection.Create(new DateStamp() { Name = "Key", Value = "Value" })
                    .ConfigureAwait(false);

                // Assert
                Assert.AreEqual("Key", row.Name);
                Assert.AreEqual("Value", row.Value);
                Assert.AreEqual(row.InsertDate, row.UpdateDate);
                Assert.That(row.InsertDate, Is.EqualTo(now).Within(TimeSpan.FromSeconds(1)));
            }
        }

        [Test]
        public async Task Insert_For_Soft_Delete()
        {
            using (IDbConnection connection = LocalDbTestHelper.OpenTestConnection(TestContext.CurrentContext.Test.FullName))
            {
                // Act
                SoftDelete softDelete = await connection.Create<SoftDelete>(new SoftDelete()).ConfigureAwait(false);

                // Assert
                Assert.Greater(softDelete.SoftDeleteId, 0);
                Assert.AreEqual(1, softDelete.RecordStatus);
            }
        }
    }
}