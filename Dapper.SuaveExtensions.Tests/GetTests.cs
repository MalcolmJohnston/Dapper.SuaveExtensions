using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Dapper;
using NUnit.Framework;
using Dapper.TotalCRUD.Test.Models;

namespace Dapper.TotalCRUD.Test
{
    [TestFixture]
    public class GetTests
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
        public async Task Get_All_With_Identity()
        {
            using (SqlConnection connection = new SqlConnection(FixtureSetup.LocalDbConnectionString))
            {
                connection.Open();

                // Arrange
                await connection.Insert(new City() { CityCode = "PUP", CityName = "Portsmouth", Area = "Hampshire" });
                await connection.Insert(new City() { CityCode = "BOU", CityName = "Bournemouth", Area = "Dorset" });

                // Act
                IEnumerable<City> cities = await connection.GetAll<City>();

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
        public async Task Get_All_With_Manual()
        {
            using (SqlConnection connection = new SqlConnection(FixtureSetup.LocalDbConnectionString))
            {
                connection.Open();

                // Arrange
                await connection.Insert(new CityManual() { CityCode = "PUP", CityName = "Portsmouth" });
                await connection.Insert(new CityManual() { CityCode = "NYC", CityName = "New York City" });

                // Act
                IEnumerable<CityManual> cities = await connection.GetAll<CityManual>();

                // Assert
                Assert.AreEqual(2, cities.Count());
            }
        }

        /// <summary>
        /// Gets the by identifier with identity.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task Get_By_Id_With_Identity()
        {
            using (SqlConnection connection = new SqlConnection(FixtureSetup.LocalDbConnectionString))
            {
                connection.Open();

                // Arrange
                City pup = await connection.Insert(new City() { CityCode = "PUP", CityName = "Portsmouth", Area = "Hampshire" });
                await connection.Insert(new City() { CityCode = "NYC", CityName = "New York City", Area = "New York" });

                // Act
                City city = await connection.Get<City>(new { pup.CityId });

                // Assert
                Assert.AreEqual(pup.CityId, city.CityId);
                Assert.AreEqual("PUP", city.CityCode);
                Assert.AreEqual("Portsmouth", city.CityName);
            }
        }

        /// <summary>
        /// Gets the by identifier with manual.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task Get_By_Id_With_Manual()
        {
            using (SqlConnection connection = new SqlConnection(FixtureSetup.LocalDbConnectionString))
            {
                connection.Open();

                // Arrange
                await connection.Insert(new CityManual() { CityCode = "PUP", CityName = "Portsmouth" });
                await connection.Insert(new CityManual() { CityCode = "NYC", CityName = "New York City" });

                // Act
                CityManual city = await connection.Get<CityManual>(new { CityCode = "NYC" });

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
        public async Task Get_By_Where_Condition()
        {
            using (SqlConnection connection = new SqlConnection(FixtureSetup.LocalDbConnectionString))
            {
                connection.Open();

                // Arrange
                await connection.Insert(new City() { CityCode = "PUP", CityName = "Portsmouth", Area = "Hampshire" });
                await connection.Insert(new City() { CityCode = "SOU", CityName = "Southampton", Area = "Hampshire" });
                await connection.Insert(new City() { CityCode = "BOU", CityName = "Bournemouth", Area = "Dorset" });

                IEnumerable<City> cities = await connection.GetList<City>(new { Area = "Hampshire" });

                Assert.AreEqual(2, cities.Count());
            }
        }
    }
}