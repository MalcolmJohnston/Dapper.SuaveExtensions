using System;
using System.Collections.Generic;
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
        /// Test that we can execute Get All when the Model is mapped with an Identity column.
        /// </summary>
        [TestCase(typeof(InMemoryDataContext))]
        [TestCase(typeof(SqlServerDataContext))]
        public async Task Read_All_With_Identity(Type dataContextType)
        {
            // Arrange
            IDataContext dataContext = DataContextTestHelper.GetDataContext(dataContextType);
            await dataContext.Create(new City() { CityCode = "PUP", CityName = "Portsmouth", Area = "Hampshire" });
            await dataContext.Create(new City() { CityCode = "BOU", CityName = "Bournemouth", Area = "Dorset" });

            // Act
            IEnumerable<City> cities = await dataContext.ReadAll<City>();

            // Assert
            Assert.AreEqual(2, cities.Count());
            Assert.Greater(cities.ElementAt(0).CityId, 0);
            Assert.Greater(cities.ElementAt(1).CityId, cities.ElementAt(0).CityId);
        }

        /// <summary>
        /// Test that we can execute Get All when the Model is mapped with a Manual key column.
        /// </summary>
        /// <returns></returns>
        [TestCase(typeof(InMemoryDataContext))]
        [TestCase(typeof(SqlServerDataContext))]
        public async Task Read_All_With_Assigned(Type dataContextType)
        {
            // Arrange
            IDataContext dataContext = DataContextTestHelper.GetDataContext(dataContextType);
            await dataContext.Create(new CityManual() { CityCode = "PUP", CityName = "Portsmouth" });
            await dataContext.Create(new CityManual() { CityCode = "NYC", CityName = "New York City" });

            // Act
            IEnumerable<CityManual> cities = dataContext.ReadAll<CityManual>().GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(2, cities.Count());
        }

        /// <summary>
        /// Test that we can get an entity with a single identity key using the property bag approach.
        /// </summary>
        [TestCase(typeof(InMemoryDataContext))]
        [TestCase(typeof(SqlServerDataContext))]
        public async Task Read_By_Id_With_Identity_Property_Bag(Type dataContextType)
        {
            // Arrange
            IDataContext dataContext = DataContextTestHelper.GetDataContext(dataContextType);
            City pup = await dataContext.Create(new City() { CityCode = "PUP", CityName = "Portsmouth", Area = "Hampshire" });
            await dataContext.Create(new City() { CityCode = "NYC", CityName = "New York City", Area = "New York" });

            // Act
            City city = await dataContext.Read<City>(new { pup.CityId });

            // Assert
            Assert.AreEqual(pup.CityId, city.CityId);
            Assert.AreEqual("PUP", city.CityCode);
            Assert.AreEqual("Portsmouth", city.CityName);
        }

        /// <summary>
        /// Test that we can get an entity with a single identity key by passing a single typed argument
        /// rather than using a property bag.
        /// </summary>
        [TestCase(typeof(InMemoryDataContext))]
        [TestCase(typeof(SqlServerDataContext))]
        public async Task Read_By_Id_With_Identity_Single_Typed_Argument(Type dataContextType)
        {
            // Arrange
            IDataContext dataContext = DataContextTestHelper.GetDataContext(dataContextType);
            City pup = await dataContext.Create(new City() { CityCode = "PUP", CityName = "Portsmouth", Area = "Hampshire" });
            await dataContext.Create(new City() { CityCode = "NYC", CityName = "New York City", Area = "New York" });

            // Act
            City city = await dataContext.Read<City>(pup.CityId);

            // Assert
            Assert.AreEqual(pup.CityId, city.CityId);
            Assert.AreEqual("PUP", city.CityCode);
            Assert.AreEqual("Portsmouth", city.CityName);
        }

        /// <summary>
        /// Test we can retrieve an entity with a single assigned key.
        /// </summary>
        /// <returns></returns>
        [TestCase(typeof(InMemoryDataContext))]
        [TestCase(typeof(SqlServerDataContext))]
        public async Task Read_By_Id_With_Assigned_Property_Bag(Type dataContextType)
        {
            // Arrange
            IDataContext dataContext = DataContextTestHelper.GetDataContext(dataContextType);
            await dataContext.Create(new CityManual() { CityCode = "PUP", CityName = "Portsmouth" });
            await dataContext.Create(new CityManual() { CityCode = "NYC", CityName = "New York City" });

            // Act
            CityManual city = await dataContext.Read<CityManual>(new { CityCode = "NYC" });

            // Assert
            Assert.AreEqual("NYC", city.CityCode);
            Assert.AreEqual("New York City", city.CityName);
        }

        /// <summary>
        /// Test we can retrive an entity with a single assigned key using a single typed argument.
        /// </summary>
        /// <returns></returns>
        [TestCase(typeof(InMemoryDataContext))]
        [TestCase(typeof(SqlServerDataContext))]
        public async Task Read_By_Id_With_Assigned_Single_Typed_Argument(Type dataContextType)
        {
            // Arrange
            IDataContext dataContext = DataContextTestHelper.GetDataContext(dataContextType);
            await dataContext.Create(new CityManual() { CityCode = "PUP", CityName = "Portsmouth" });
            await dataContext.Create(new CityManual() { CityCode = "NYC", CityName = "New York City" });

            // Act
            CityManual city = await dataContext.Read<CityManual>("NYC");

            // Assert
            Assert.AreEqual("NYC", city.CityCode);
            Assert.AreEqual("New York City", city.CityName);
        }

        /// <summary>
        /// Gets the by where conditions.
        /// </summary>
        /// <returns></returns>
        [TestCase(typeof(InMemoryDataContext))]
        [TestCase(typeof(SqlServerDataContext))]
        public async Task Read_By_Where_Condition(Type dataContextType)
        {
            // Arrange
            IDataContext dataContext = DataContextTestHelper.GetDataContext(dataContextType);
            await dataContext.Create(new City() { CityCode = "PUP", CityName = "Portsmouth", Area = "Hampshire" });
            await dataContext.Create(new City() { CityCode = "SOU", CityName = "Southampton", Area = "Hampshire" });
            await dataContext.Create(new City() { CityCode = "BOU", CityName = "Bournemouth", Area = "Dorset" });

            // Act
            IEnumerable<City> cities = await dataContext.ReadList<City>(new { Area = "Hampshire" });

            // Assert
            // NOTE: there is only one team in Hampshire
            Assert.AreEqual(2, cities.Count());
        }
    }
}
