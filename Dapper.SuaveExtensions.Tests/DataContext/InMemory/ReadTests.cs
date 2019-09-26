using Dapper.SuaveExtensions.DataContext;
using Dapper.SuaveExtensions.Tests.Models;

using NUnit.Framework;

namespace Dapper.SuaveExtensions.Tests.DataContext.InMemory
{
    public class ReadTests
    {
        /// <summary>
        /// Gets the by identifier with identity.
        /// </summary>
        /// <returns></returns>
        [Test]
        public void Read_By_Id_With_Identity()
        {
            // Arrange
            InMemoryDataContext dataContext = new InMemoryDataContext();
            InMemoryDataContext.AddOrUpdateData<City>(new City[]
            {
                new City() { CityId = 1, CityCode = "PUP", CityName = "Portsmouth", Area = "Hampshire" },
                new City() { CityId = 2, CityCode = "NYC", CityName = "New York City", Area = "New York" }
            });

            // Act
            City city = dataContext.Read<City>(1).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(1, city.CityId);
            Assert.AreEqual("PUP", city.CityCode);
            Assert.AreEqual("Portsmouth", city.CityName);
        }
    }
}
