using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dapper.SuaveExtensions.DataContext;
using Dapper.SuaveExtensions.Tests.Models;

using NUnit.Framework;

namespace Dapper.SuaveExtensions.Tests.DataContext.InMemory
{
    public class DeleteTests
    {
        /// <summary>
        /// Test that we can delete a single entity.
        /// </summary>
        [Test]
        public async Task Delete_Entity()
        {
            // Arrange
            InMemoryDataContext dataContext = new InMemoryDataContext();
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
        [Test]
        public async Task Delete_Entity_Single_Typed_Argument()
        {
            // Arrange
            InMemoryDataContext dataContext = new InMemoryDataContext();
            City city = await dataContext.Create<City>(new City() { CityCode = "BAS", CityName = "Basingstoke", Area = "Hampshire" });

            // Act
            await dataContext.Delete<City>(city.CityId);

            // Assert
            Assert.IsNull(await dataContext.Read<City>(new { city.CityId }));
        }

        /// <summary>
        /// Test that we can delete a multiple entities using a where condition.
        /// </summary>
        [Test]
        public async Task Delete_List_Of_Entities()
        {
            // Arrange
            InMemoryDataContext dataContext = new InMemoryDataContext();
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
        [Test]
        public async Task Delete_List_No_Conditions()
        {
            // Arrange
            InMemoryDataContext dataContext = new InMemoryDataContext();
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
        [Test]
        public async Task Delete_List_Non_Existing_Conditions()
        {
            // Arrange
            InMemoryDataContext dataContext = new InMemoryDataContext();
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
