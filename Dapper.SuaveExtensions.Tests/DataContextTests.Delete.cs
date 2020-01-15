using System;
using System.Linq;
using System.Threading.Tasks;

using Dapper.SuaveExtensions.DataContext;
using Dapper.SuaveExtensions.Tests.Models;

using NUnit.Framework;

namespace Dapper.SuaveExtensions.Tests
{
    public partial class DataContextTests
    {
        /// <summary>
        /// Test that we can delete a single entity.
        /// </summary>
        [TestCase(typeof(InMemoryDataContext))]
        [TestCase(typeof(SqlServerDataContext))]
        public async Task Delete_Entity(Type dataContextType)
        {
            // Arrange
            IDataContext dataContext = DataContextTestHelper.GetDataContext(dataContextType);
            City city = await dataContext.Create<City>(new City() { CityCode = "BAS", CityName = "Basingstoke", Area = "Hampshire" });

            // Act
            await dataContext.Delete<City>(new { city.CityId, });

            // Assert
            Assert.IsNull(await dataContext.Read<City>(new { city.CityId }));
        }

        /// <summary>
        /// Test that we can delete a single entity with an identity column by passing a single
        /// typed argument rather than a property bag.
        /// </summary>
        [TestCase(typeof(InMemoryDataContext))]
        [TestCase(typeof(SqlServerDataContext))]
        public async Task Delete_Entity_Single_Typed_Argument(Type dataContextType)
        {
            // Arrange
            IDataContext dataContext = DataContextTestHelper.GetDataContext(dataContextType);
            City city = await dataContext.Create<City>(new City() { CityCode = "BAS", CityName = "Basingstoke", Area = "Hampshire" });

            // Act
            await dataContext.Delete<City>(city.CityId);

            // Assert
            Assert.IsNull(await dataContext.Read<City>(new { city.CityId }));
        }

        /// <summary>
        /// Test that we can delete a multiple entities using a where condition.
        /// </summary>
        [TestCase(typeof(InMemoryDataContext))]
        [TestCase(typeof(SqlServerDataContext))]
        public async Task Delete_List_Of_Entities(Type dataContextType)
        {
            // Arrange
            IDataContext dataContext = DataContextTestHelper.GetDataContext(dataContextType);
            await dataContext.Create<City>(new City() { CityCode = "BAS", CityName = "Basingstoke", Area = "Hampshire" });
            await dataContext.Create<City>(new City() { CityCode = "PUP", CityName = "Portsmouth", Area = "Hampshire" });
            await dataContext.Create<City>(new City() { CityCode = "BOU", CityName = "Bournemouth", Area = "Dorset" });
            await dataContext.Create<City>(new City() { CityCode = "HAV", CityName = "Havant", Area = "Hampshire" });

            // Act
            await dataContext.DeleteList<City>(new { Area = "Hampshire" });

            // Assert
            Assert.AreEqual(0, (await dataContext.ReadList<City>(new { Area = "Hampshire" })).Count());
            Assert.AreEqual(1, (await dataContext.ReadAll<City>()).Count());   
        }

        /// <summary>
        /// Test that if we try to delete a list of entities without specifying a condition that and exception is thrown.
        /// </summary>
        /// <returns></returns>
        [TestCase(typeof(InMemoryDataContext))]
        [TestCase(typeof(SqlServerDataContext))]
        public async Task Delete_List_No_Conditions(Type dataContextType)
        {
            // Arrange
            IDataContext dataContext = DataContextTestHelper.GetDataContext(dataContextType);
            await dataContext.Create<City>(new City() { CityCode = "BAS", CityName = "Basingstoke", Area = "Hampshire" });
            await dataContext.Create<City>(new City() { CityCode = "PUP", CityName = "Portsmouth", Area = "Hampshire" });
            await dataContext.Create<City>(new City() { CityCode = "BOU", CityName = "Bournemouth", Area = "Dorset" });
            await dataContext.Create<City>(new City() { CityCode = "HAV", CityName = "Havant", Area = "Hampshire" });

            // Act / Assert
            Assert.ThrowsAsync<ArgumentException>(async () => { await dataContext.DeleteList<City>(null); });
            Assert.ThrowsAsync<ArgumentException>(async () => { await dataContext.DeleteList<City>(new object()); });
        }

        /// <summary>
        /// Test that if we try to delete a list of entities without specifying a condition that and exception is thrown.
        /// </summary>
        /// <returns></returns>
        [TestCase(typeof(InMemoryDataContext))]
        [TestCase(typeof(SqlServerDataContext))]
        public async Task Delete_List_Non_Existing_Conditions(Type dataContextType)
        {
            // Arrange
            IDataContext dataContext = DataContextTestHelper.GetDataContext(dataContextType);
            await dataContext.Create<City>(new City() { CityCode = "BAS", CityName = "Basingstoke", Area = "Hampshire" });
            await dataContext.Create<City>(new City() { CityCode = "PUP", CityName = "Portsmouth", Area = "Hampshire" });
            await dataContext.Create<City>(new City() { CityCode = "BOU", CityName = "Bournemouth", Area = "Dorset" });
            await dataContext.Create<City>(new City() { CityCode = "HAV", CityName = "Havant", Area = "Hampshire" });

            // Act / Assert
            Assert.ThrowsAsync<ArgumentException>(async () => { await dataContext.DeleteList<City>(new { Code = "BAS" }); });
            Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await dataContext.DeleteList<City>(new
                {
                    Name = "Portsmouth",
                    Area = "Hampshire"
                });
            });
        }
    }
}
