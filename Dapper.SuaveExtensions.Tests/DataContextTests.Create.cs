using System;
using System.Threading.Tasks;

using Dapper.SuaveExtensions.DataContext;
using Dapper.SuaveExtensions.Tests.Models;

using NUnit.Framework;

namespace Dapper.SuaveExtensions.Tests
{
    [TestFixture]
    public partial class DataContextTests
    {
        /// <summary>
        /// Test that we can insert an entity that has single key which is an identity column.
        /// </summary>
        [TestCase(typeof(InMemoryDataContext))]
        [TestCase(typeof(SqlServerDataContext))]
        
        public async Task Insert_With_Identity(Type dataContextType)
        {
            // Arrange
            IDataContext dataContext = DataContextTestHelper.GetDataContext(dataContextType);

            // Act
            City city = await dataContext.Create(new City() { CityCode = "BRI", CityName = "Brighton", Area = "Sussex" });

            // Assert
            Assert.Greater(city.CityId, 0);
            Assert.AreEqual("BRI", city.CityCode);
            Assert.AreEqual("Brighton", city.CityName);
            Assert.AreEqual("Sussex", city.Area);
        }

        /// <summary>
        /// Test that we can insert an entity which has a single key that is manually derived
        /// </summary>
        [TestCase(typeof(InMemoryDataContext))]
        [TestCase(typeof(SqlServerDataContext))]
        public async Task Insert_With_Assigned_Key(Type dataContextType)
        {
            // Arrange
            IDataContext dataContext = DataContextTestHelper.GetDataContext(dataContextType);

            // Act
            CityManual city = await dataContext.Create(new CityManual() { CityCode = "BRI", CityName = "Brighton" });

            // Assert
            Assert.AreEqual("BRI", city.CityCode);
            Assert.AreEqual("Brighton", city.CityName);
        }

        /// <summary>
        /// Test that we can insert an entity which has a sequentially calculated key
        /// </summary>
        [TestCase(typeof(InMemoryDataContext))]
        [TestCase(typeof(SqlServerDataContext))]
        public async Task Insert_With_Sequential_Key(Type dataContextType)
        {
            // Arrange
            IDataContext dataContext = DataContextTestHelper.GetDataContext(dataContextType);

            // Act
            CitySequential city = await dataContext.Create(new CitySequential() { CityCode = "BRI", CityName = "Brighton" });

            // Assert
            Assert.Greater(city.CityId, 0);
            Assert.AreEqual("BRI", city.CityCode);
            Assert.AreEqual("Brighton", city.CityName);
        }

        /// <summary>
        /// Test that we can insert an entity which has one manual key and one sequential key
        /// </summary>
        /// <returns></returns>
        [TestCase(typeof(InMemoryDataContext))]
        [TestCase(typeof(SqlServerDataContext))]
        public async Task Insert_With_Composite_Key_One_Assigned_And_One_Sequential(Type dataContextType)
        {
            // Arrange
            IDataContext dataContext = DataContextTestHelper.GetDataContext(dataContextType);

            // Act
            AssignedAndSequential one = await dataContext.Create(new AssignedAndSequential() { AssignedId = 1, Heading = "One" });
            AssignedAndSequential two = await dataContext.Create(new AssignedAndSequential() { AssignedId = 1, Heading = "Two" });

            // Assert
            Assert.AreEqual(1, one.SequentialId);
            Assert.AreEqual(2, two.SequentialId);
        }

        /// <summary>
        /// Test that we can insert an entity which has two manual keys and one sequential key
        /// </summary>
        /// <returns></returns>
        [TestCase(typeof(InMemoryDataContext))]
        [TestCase(typeof(SqlServerDataContext))]
        public async Task Insert_With_Composite_Key_Two_Assigned_And_One_Sequential(Type dataContextType)
        {
            // Arrange
            IDataContext dataContext = DataContextTestHelper.GetDataContext(dataContextType);

            // Act
            AssignedPairAndSequential oneOneOne = await dataContext.Create(new AssignedPairAndSequential() { FirstAssignedId = 1, SecondAssignedId = 1, Heading = "One" });
            AssignedPairAndSequential oneOneTwo = await dataContext.Create(new AssignedPairAndSequential() { FirstAssignedId = 1, SecondAssignedId = 1, Heading = "Two" });
            AssignedPairAndSequential oneTwoOne = await dataContext.Create(new AssignedPairAndSequential() { FirstAssignedId = 1, SecondAssignedId = 2, Heading = "One" });

            // Assert
            Assert.AreEqual(1, oneOneOne.SequentialId);
            Assert.AreEqual(2, oneOneTwo.SequentialId);
            Assert.AreEqual(1, oneTwoOne.SequentialId);
        }

        /// <summary>
        /// Test that insert and update date properties that have been marked as Date Stamps are automatically set
        /// to the time now on insert.
        /// </summary>
        /// <returns></returns>
        [TestCase(typeof(InMemoryDataContext))]
        [TestCase(typeof(SqlServerDataContext))]
        public async Task Insert_With_Datestamp(Type dataContextType)
        {
            // Arrange
            IDataContext dataContext = DataContextTestHelper.GetDataContext(dataContextType);

            // Act
            DateTime now = DateTime.Now;
            DateStamp row = await dataContext.Create(new DateStamp() { Name = "Key", Value = "Value" });

            // Assert
            Assert.AreEqual("Key", row.Name);
            Assert.AreEqual("Value", row.Value);
            Assert.AreEqual(row.InsertDate, row.UpdateDate);
            Assert.That(row.InsertDate, Is.EqualTo(now).Within(TimeSpan.FromSeconds(1)));
        }

        [TestCase(typeof(InMemoryDataContext))]
        [TestCase(typeof(SqlServerDataContext))]
        public async Task Insert_With_Soft_Delete(Type dataContextType)
        {
            // Arrange
            IDataContext dataContext = DataContextTestHelper.GetDataContext(dataContextType);

            // Act
            SoftDelete softDelete = await dataContext.Create<SoftDelete>(new SoftDelete()).ConfigureAwait(false);

            // Assert
            Assert.Greater(softDelete.SoftDeleteId, 0);
            Assert.AreEqual(1, softDelete.RecordStatus);
        }
    }
}