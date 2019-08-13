using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;
using Dapper.SuaveExtensions.Tests.Models;

namespace Dapper.SuaveExtensions.Tests
{
    [TestFixture]
    public class UpdateTests
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
        /// Test that we can update a single editable property
        /// </summary>
        [Test]
        public async Task Update_Editable_Property()
        {
            using (SqlConnection connection = new SqlConnection(FixtureSetup.LocalDbConnectionString))
            {
                connection.Open();

                // Arrange
                City city = await connection.Create<City>(new City() { CityCode = "BAS", CityName = "Basingstoke", Area = "Hampshire" });

                // Act
                City basVegas = await connection.Update<City>(new { city.CityId, CityName = "Bas Vegas!" });

                Assert.AreEqual(city.CityId, basVegas.CityId);
                Assert.AreEqual(city.CityCode, basVegas.CityCode);
                Assert.AreEqual(city.Area, basVegas.Area);
                Assert.AreEqual("Bas Vegas!", basVegas.CityName);
            }
        }

        /// <summary>
        /// Test that we can update multiple editable properties.
        /// </summary>
        [Test]
        public async Task Update_Editable_Properties()
        {
            using (SqlConnection connection = new SqlConnection(FixtureSetup.LocalDbConnectionString))
            {
                connection.Open();

                // Arrange
                City city = await connection.Create<City>(new City() { CityCode = "BAS", CityName = "Basingstoke", Area = "Hampshire" });

                // Act
                City basVegas = await connection.Update<City>(new
                {
                    city.CityId,
                    CityCode = "BV",
                    CityName = "Bas Vegas!",
                    Area = "The Strip"
                });

                Assert.AreEqual(city.CityId, basVegas.CityId);
                Assert.AreEqual("BV", basVegas.CityCode);
                Assert.AreEqual("The Strip", basVegas.Area);
                Assert.AreEqual("Bas Vegas!", basVegas.CityName);
            }
        }

        /// <summary>
        /// Test that insert and update date properties that have been marked as Date Stamps are automatically set
        /// to the time now on insert.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task Update_With_Datestamp()
        {
            using (SqlConnection connection = new SqlConnection(FixtureSetup.LocalDbConnectionString))
            {
                connection.Open();

                // Arrange

                // insert row
                DateStamp row = await connection.Create(new DateStamp() { Name = "Key", Value = "Value" })
                    .ConfigureAwait(false);

                // sleep so that insert date and update date will be different when update called
                Thread.Sleep(TimeSpan.FromMilliseconds(100));

                // Act
                DateTime updateDate = DateTime.Now;
                DateStamp updatedRow = await connection.Update<DateStamp>(new { row.Name, Value = "New Value" })
                    .ConfigureAwait(false);

                // Assert
                Assert.AreEqual(row.Name, updatedRow.Name);
                Assert.AreEqual("New Value", updatedRow.Value);
                Assert.AreEqual(0, (row.InsertDate - updatedRow.InsertDate).Seconds);
                Assert.AreNotEqual(row.InsertDate, updatedRow.UpdateDate);
                Assert.That(updatedRow.UpdateDate, Is.EqualTo(updateDate).Within(TimeSpan.FromSeconds(1)));
            }
        }

        /// <summary>
        /// Test that when we try to update a soft delete column that the value is ignored.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task Update_Soft_Delete_Column()
        {
            using (SqlConnection connection = new SqlConnection(FixtureSetup.LocalDbConnectionString))
            {
                connection.Open();

                // Arrange
                SoftDelete softDelete = await connection.Create<SoftDelete>(new SoftDelete()).ConfigureAwait(false);

                // Act / Assert
                Assert.ThrowsAsync<ArgumentException>(async () =>
                {
                    await connection.Update<SoftDelete>(new { softDelete.SoftDeleteId, RecordsStatus = 999 });
                });
            }
        }

        /// <summary>
        /// Test that when we try to update a read only column that our update is not persisted.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task Update_Read_Only_Column()
        {
            using (SqlConnection connection = new SqlConnection(FixtureSetup.LocalDbConnectionString))
            {
                connection.Open();

                // Arrange
                ReadOnly readOnly = await connection.Create<ReadOnly>(new ReadOnly()
                {
                    Editable = "Hello",
                    ReadOnlyProperty = "World"
                });

                // Act
                readOnly = await connection.Update<ReadOnly>(new
                {
                    readOnly.SequentialId,
                    Editable = "Goodbye",
                    ReadOnlyProperty = "Yesterday"
                });

                // Assert
                Assert.AreEqual("Goodbye", readOnly.Editable);
                Assert.AreEqual("World", readOnly.ReadOnlyProperty);
            }
        }
    }
}