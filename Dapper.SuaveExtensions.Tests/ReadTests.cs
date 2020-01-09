using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Dapper.SuaveExtensions;
using Dapper.SuaveExtensions.Tests.Models;

using NUnit.Framework;

namespace Dapper.SuaveExtensions.Tests
{
    [TestFixture]
    public class ReadTests
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
        /// Test that we can execute Get All when the Model is mapped with an Identity column.
        /// </summary>
        [Test]
        public async Task Read_All_With_Identity()
        {
            using (SqlConnection connection = new SqlConnection(FixtureSetup.LocalDbConnectionString))
            {
                connection.Open();

                // Arrange
                await connection.Create(new City() { CityCode = "PUP", CityName = "Portsmouth", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "BOU", CityName = "Bournemouth", Area = "Dorset" });

                // Act
                IEnumerable<City> cities = await connection.ReadAll<City>();

                // Assert
                Assert.AreEqual(2, cities.Count());
                Assert.Greater(cities.ElementAt(0).CityId, 0);
                Assert.Greater(cities.ElementAt(1).CityId, cities.ElementAt(0).CityId);
            }
        }

        /// <summary>
        /// Test that we can execute Get All when the Model is mapped with a Manual key column.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task Read_All_With_Assigned()
        {
            using (SqlConnection connection = new SqlConnection(FixtureSetup.LocalDbConnectionString))
            {
                connection.Open();

                // Arrange
                await connection.Create(new CityManual() { CityCode = "PUP", CityName = "Portsmouth" });
                await connection.Create(new CityManual() { CityCode = "NYC", CityName = "New York City" });

                // Act
                IEnumerable<CityManual> cities = await connection.ReadAll<CityManual>();

                // Assert
                Assert.AreEqual(2, cities.Count());
            }
        }

        /// <summary>
        /// Test that we can get an entity with a single identity key using the property bag approach.
        /// </summary>
        [Test]
        public async Task Read_By_Id_With_Identity_Property_Bag()
        {
            using (SqlConnection connection = new SqlConnection(FixtureSetup.LocalDbConnectionString))
            {
                connection.Open();

                // Arrange
                City pup = await connection.Create(new City() { CityCode = "PUP", CityName = "Portsmouth", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "NYC", CityName = "New York City", Area = "New York" });

                // Act
                City city = await connection.Read<City>(new { pup.CityId });

                // Assert
                Assert.AreEqual(pup.CityId, city.CityId);
                Assert.AreEqual("PUP", city.CityCode);
                Assert.AreEqual("Portsmouth", city.CityName);
            }
        }

        /// <summary>
        /// Test that we can get an entity with a single identity key by passing a single type argument
        /// rather than using a property bag.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task Read_By_Id_With_Identity_Single_Typed_Argument()
        {
            using (SqlConnection connection = new SqlConnection(FixtureSetup.LocalDbConnectionString))
            {
                connection.Open();

                // Arrange
                City pup = await connection.Create(new City() { CityCode = "PUP", CityName = "Portsmouth", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "NYC", CityName = "New York City", Area = "New York" });

                // Act
                City city = await connection.Read<City>(pup.CityId);

                // Assert
                Assert.AreEqual(pup.CityId, city.CityId);
                Assert.AreEqual("PUP", city.CityCode);
                Assert.AreEqual("Portsmouth", city.CityName);
            }
        }

        /// <summary>
        /// Test we can retrive an entity with a single assigned key.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task Read_By_Id_With_Assigned()
        {
            using (SqlConnection connection = new SqlConnection(FixtureSetup.LocalDbConnectionString))
            {
                connection.Open();

                // Arrange
                await connection.Create(new CityManual() { CityCode = "PUP", CityName = "Portsmouth" });
                await connection.Create(new CityManual() { CityCode = "NYC", CityName = "New York City" });

                // Act
                CityManual city = await connection.Read<CityManual>(new { CityCode = "NYC" });

                // Assert
                Assert.AreEqual("NYC", city.CityCode);
                Assert.AreEqual("New York City", city.CityName);
            }
        }

        /// <summary>
        /// Test we can retrive an entity with a single assigned key using a single type argument.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task Read_By_Id_With_Assigned_Single_Typed_Argument()
        {
            using (SqlConnection connection = new SqlConnection(FixtureSetup.LocalDbConnectionString))
            {
                connection.Open();

                // Arrange
                await connection.Create(new CityManual() { CityCode = "PUP", CityName = "Portsmouth" });
                await connection.Create(new CityManual() { CityCode = "NYC", CityName = "New York City" });

                // Act
                CityManual city = await connection.Read<CityManual>("NYC");

                // Assert
                Assert.AreEqual("NYC", city.CityCode);
                Assert.AreEqual("New York City", city.CityName);
            }
        }

        /// <summary>
        /// Gets the by where conditions.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task Read_By_Where_Condition()
        {
            using (SqlConnection connection = new SqlConnection(FixtureSetup.LocalDbConnectionString))
            {
                connection.Open();

                // Arrange
                await connection.Create(new City() { CityCode = "PUP", CityName = "Portsmouth", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "SOU", CityName = "Southampton", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "BOU", CityName = "Bournemouth", Area = "Dorset" });

                IEnumerable<City> cities = await connection.ReadList<City>(new { Area = "Hampshire" });

                Assert.AreEqual(2, cities.Count());
            }
        }
    }
}