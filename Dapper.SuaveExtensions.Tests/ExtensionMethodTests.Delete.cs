using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

using NUnit.Framework;
using Dapper.SuaveExtensions.Tests.Models;

namespace Dapper.SuaveExtensions.Tests
{
    [TestFixture]
    public partial class ExtensionMethodTests
    {
        /// <summary>
        /// Test that we can delete a single entity.
        /// </summary>
        [Test]
        public async Task Delete_Entity()
        {
            using (IDbConnection connection = LocalDbTestHelper.OpenTestConnection(TestContext.CurrentContext.Test.FullName))
            {
                // Arrange
                City city = await connection.Create<City>(new City() { CityCode = "BAS", CityName = "Basingstoke", Area = "Hampshire" });

                // Act
                await connection.Delete<City>(new { city.CityId, });

                // Assert
                Assert.IsNull(await connection.Read<City>(new { city.CityId }));
            }
        }

        /// <summary>
        /// Test that we can delete a single entity with an identity column by passing a single
        /// typed argument rather than a property bag.
        /// </summary>
        [Test]
        public async Task Delete_Entity_Single_Typed_Argument()
        {
            using (IDbConnection connection = LocalDbTestHelper.OpenTestConnection(TestContext.CurrentContext.Test.FullName))
            {
                // Arrange
                City city = await connection.Create<City>(new City() { CityCode = "BAS", CityName = "Basingstoke", Area = "Hampshire" });

                // Act
                await connection.Delete<City>(city.CityId);

                // Assert
                Assert.IsNull(await connection.Read<City>(new { city.CityId }));
            }
        }

        /// <summary>
        /// Test that we can delete a multiple entities using a where condition.
        /// </summary>
        [Test]
        public async Task Delete_List_Of_Entities()
        {
            using (IDbConnection connection = LocalDbTestHelper.OpenTestConnection(TestContext.CurrentContext.Test.FullName))
            {
                // Arrange
                await connection.Create<City>(new City() { CityCode = "BAS", CityName = "Basingstoke", Area = "Hampshire" });
                await connection.Create<City>(new City() { CityCode = "PUP", CityName = "Portsmouth", Area = "Hampshire" });
                await connection.Create<City>(new City() { CityCode = "BOU", CityName = "Bournemouth", Area = "Dorset" });
                await connection.Create<City>(new City() { CityCode = "HAV", CityName = "Havant", Area = "Hampshire" });

                // Act
                await connection.DeleteList<City>(new { Area = "Hampshire" });

                // Assert
                Assert.AreEqual(0, (await connection.ReadList<City>(new { Area = "Hampshire" })).Count());
                Assert.AreEqual(1, (await connection.ReadAll<City>()).Count());
            }
        }

        /// <summary>
        /// Test that if we try to delete a list of entities without specifying a condition that and exception is thrown.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task Delete_List_No_Conditions()
        {
            using (IDbConnection connection = LocalDbTestHelper.OpenTestConnection(TestContext.CurrentContext.Test.FullName))
            {
                // Arrange
                await connection.Create<City>(new City() { CityCode = "BAS", CityName = "Basingstoke", Area = "Hampshire" });
                await connection.Create<City>(new City() { CityCode = "PUP", CityName = "Portsmouth", Area = "Hampshire" });
                await connection.Create<City>(new City() { CityCode = "BOU", CityName = "Bournemouth", Area = "Dorset" });
                await connection.Create<City>(new City() { CityCode = "HAV", CityName = "Havant", Area = "Hampshire" });

                // Act / Assert
                Assert.ThrowsAsync<ArgumentException>(async () => { await connection.DeleteList<City>(null); });
                Assert.ThrowsAsync<ArgumentException>(async () => { await connection.DeleteList<City>(new object()); });
            }
        }

        /// <summary>
        /// Test that if we try to delete a list of entities without specifying a condition that and exception is thrown.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task Delete_List_Non_Existing_Conditions()
        {
            using (IDbConnection connection = LocalDbTestHelper.OpenTestConnection(TestContext.CurrentContext.Test.FullName))
            {
                // Arrange
                await connection.Create<City>(new City() { CityCode = "BAS", CityName = "Basingstoke", Area = "Hampshire" });
                await connection.Create<City>(new City() { CityCode = "PUP", CityName = "Portsmouth", Area = "Hampshire" });
                await connection.Create<City>(new City() { CityCode = "BOU", CityName = "Bournemouth", Area = "Dorset" });
                await connection.Create<City>(new City() { CityCode = "HAV", CityName = "Havant", Area = "Hampshire" });

                // Act / Assert
                Assert.ThrowsAsync<ArgumentException>(async () => { await connection.DeleteList<City>(new { Code = "BAS" }); });
                Assert.ThrowsAsync<ArgumentException>(async () => 
                {
                    await connection.DeleteList<City>(new
                    {
                        Name = "Portsmouth",
                        Area = "Hampshire"
                    });
                });
            }
        }
    }
}