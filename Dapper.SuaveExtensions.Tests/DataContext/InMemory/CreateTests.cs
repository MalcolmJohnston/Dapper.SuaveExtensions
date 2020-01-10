using System;
using System.Threading.Tasks;

using Dapper.SuaveExtensions.DataContext;
using Dapper.SuaveExtensions.Tests.Models;

using NUnit.Framework;

namespace Dapper.SuaveExtensions.Tests.DataContext.InMemory
{
    [TestFixture]
    public class CreateTests
    {
        /// <summary>
        /// Test that we can insert an entity that has single key which is an identity column.
        /// </summary>
        [Test]
        public async Task Insert_With_Identity()
        {
            // Arrange
            InMemoryDataContext dataContext = new InMemoryDataContext();

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
        [Test]
        public async Task Insert_With_Assigned_Key()
        {
            // Arrange
            InMemoryDataContext dataContext = new InMemoryDataContext();

            // Act
            CityManual city = await dataContext.Create(new CityManual() { CityCode = "BRI", CityName = "Brighton" });

            // Assert
            Assert.AreEqual("BRI", city.CityCode);
            Assert.AreEqual("Brighton", city.CityName);
        }

        /// <summary>
        /// Test that we can insert an entity which has a sequentially calculated key
        /// </summary>
        [Test]
        public async Task Insert_With_Sequential_Key()
        {
            // Arrange
            InMemoryDataContext dataContext = new InMemoryDataContext();

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
        [Test]
        public async Task Insert_With_Composite_Key_One_Assigned_And_One_Sequential()
        {
            // Arrange
            InMemoryDataContext dataContext = new InMemoryDataContext();

            // Act
            Itinerary one = await dataContext.Create(new Itinerary() { BookingId = 1, ItineraryTitle = "One" });
            Itinerary two = await dataContext.Create(new Itinerary() { BookingId = 1, ItineraryTitle = "Two" });

            // Assert
            Assert.AreEqual(1, one.ItineraryId);
            Assert.AreEqual(2, two.ItineraryId);
        }

        /// <summary>
        /// Test that we can insert an entity which has two manual keys and one sequential key
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task Insert_With_Composite_Key_Two_Assigned_And_One_Sequential()
        {
            // Arrange
            InMemoryDataContext dataContext = new InMemoryDataContext();

            // Act
            Element oneOneOne = await dataContext.Create(new Element() { BookingId = 1, ItineraryId = 1, ElementTitle = "One" });
            Element oneOneTwo = await dataContext.Create(new Element() { BookingId = 1, ItineraryId = 1, ElementTitle = "Two" });
            Element oneTwoOne = await dataContext.Create(new Element() { BookingId = 1, ItineraryId = 2, ElementTitle = "One" });

            // Assert
            Assert.AreEqual(1, oneOneOne.ElementId);
            Assert.AreEqual(2, oneOneTwo.ElementId);
            Assert.AreEqual(1, oneTwoOne.ElementId);
        }

        /// <summary>
        /// Test that insert and update date properties that have been marked as Date Stamps are automatically set
        /// to the time now on insert.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task Insert_With_Datestamp()
        {
            // Arrange
            InMemoryDataContext dataContext = new InMemoryDataContext();

            // Act
            DateTime now = DateTime.Now;
            DateStamp row = await dataContext.Create(new DateStamp() { Name = "Key", Value = "Value" });

            // Assert
            Assert.AreEqual("Key", row.Name);
            Assert.AreEqual("Value", row.Value);
            Assert.AreEqual(row.InsertDate, row.UpdateDate);
            Assert.That(row.InsertDate, Is.EqualTo(now).Within(TimeSpan.FromSeconds(1)));
        }

        [Test]
        public async Task Insert_With_Soft_Delete()
        {
            // Arrange
            InMemoryDataContext dataContext = new InMemoryDataContext();

            // Act
            SoftDelete softDelete = await dataContext.Create<SoftDelete>(new SoftDelete()).ConfigureAwait(false);

            // Assert
            Assert.Greater(softDelete.SoftDeleteId, 0);
            Assert.AreEqual(1, softDelete.RecordStatus);
        }
    }
}