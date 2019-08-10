using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;
using Dapper.TotalCRUD.Test.Models;

namespace Dapper.TotalCRUD.Test
{
    [TestFixture]
    public class InsertTests
    {
        /// <summary>
        /// Initialises routine for each Test Fixture
        /// Use a Monitor to ensure that only one test can run at a time
        /// </summary>
        [SetUp]
        public void Setup()
        {
            Monitor.Enter(FixtureSetup.LockObject);
        }

        /// <summary>
        /// Tear down routine for each Test Fixture
        /// Release the Monitor so the next test can run
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            FixtureSetup.TestDataTearDown();
            Monitor.Exit(FixtureSetup.LockObject);
        }

        /// <summary>
        /// Test that we can insert an entity that has single key which is an identity column.
        /// </summary>
        [Test]
        public async Task Insert_With_Identity()
        {
            using (SqlConnection connection = new SqlConnection(FixtureSetup.LocalDbConnectionString))
            {
                connection.Open();

                City city = await connection.Insert<City>(new City() { CityCode = "BRI", CityName = "Brighton", Area = "Sussex" })
                    .ConfigureAwait(false);

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
        public async Task Insert_With_Manual_Key()
        {
            using (SqlConnection connection = new SqlConnection(FixtureSetup.LocalDbConnectionString))
            {
                connection.Open();

                CityManual city = await connection.Insert<CityManual>(new CityManual()
                {
                    CityCode = "BRI",
                    CityName = "Brighton"
                }).ConfigureAwait(false);

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
            using (SqlConnection connection = new SqlConnection(FixtureSetup.LocalDbConnectionString))
            {
                connection.Open();

                CitySequential city = await connection.Insert<CitySequential>(new CitySequential()
                {
                    CityCode = "BRI",
                    CityName = "Brighton"
                }).ConfigureAwait(false);

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
        public async Task Insert_With_Composite_Key_One_Manual_And_One_Sequential()
        {
            using (SqlConnection connection = new SqlConnection(FixtureSetup.LocalDbConnectionString))
            {
                connection.Open();

                // Act
                Itinerary one = await connection.Insert(new Itinerary() { BookingId = 1, ItineraryTitle = "One" });
                Itinerary two = await connection.Insert(new Itinerary() { BookingId = 1, ItineraryTitle = "Two" });

                // Assert
                Assert.AreEqual(1, one.ItineraryId);
                Assert.AreEqual(2, two.ItineraryId);
            }
        }

        /// <summary>
        /// Test that we can insert an entity which has two manual keys and one sequential key
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task Insert_With_Composite_Key_Two_Manual_And_One_Sequential()
        {
            using (SqlConnection connection = new SqlConnection(FixtureSetup.LocalDbConnectionString))
            {
                connection.Open();

                // Act
                Element oneOneOne = await connection.Insert(new Element() { BookingId = 1, ItineraryId = 1, ElementTitle = "One" });
                Element oneOneTwo = await connection.Insert(new Element() { BookingId = 1, ItineraryId = 1, ElementTitle = "Two" });
                Element oneTwoOne = await connection.Insert(new Element() { BookingId = 1, ItineraryId = 2, ElementTitle = "One" });

                // Assert
                Assert.AreEqual(1, oneOneOne.ElementId);
                Assert.AreEqual(2, oneOneTwo.ElementId);
                Assert.AreEqual(1, oneTwoOne.ElementId);
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
            using (SqlConnection connection = new SqlConnection(FixtureSetup.LocalDbConnectionString))
            {
                connection.Open();

                // Act
                DateTime now = DateTime.Now;
                DateStamp row = await connection.Insert(new DateStamp() { Name = "Key", Value = "Value" })
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
            using (SqlConnection connection = new SqlConnection(FixtureSetup.LocalDbConnectionString))
            {
                connection.Open();

                // Act
                SoftDelete softDelete = await connection.Insert<SoftDelete>(new SoftDelete()).ConfigureAwait(false);

                // Assert
                Assert.Greater(softDelete.SoftDeleteId, 0);
                Assert.AreEqual(1, softDelete.RecordStatus);
            }
        }
    }
}