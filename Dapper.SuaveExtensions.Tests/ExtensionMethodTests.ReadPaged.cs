using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

using Dapper.SuaveExtensions;
using Dapper.SuaveExtensions.Tests.Models;

using NUnit.Framework;

namespace Dapper.SuaveExtensions.Tests
{
    [TestFixture]
    public partial class ExtensionMethodTests
    {
        /// <summary>
        /// Test that we can get the first page when not specifying where conditions or a sort order.
        /// </summary>
        [Test]
        public async Task Read_Paged_First_Page()
        {
            using (IDbConnection connection = LocalDbTestHelper.OpenTestConnection(TestContext.CurrentContext.Test.FullName))
            {
                // Arrange
                await connection.Create(new City() { CityCode = "PUP", CityName = "Portsmouth", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "SOU", CityName = "Southampton", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "BAS", CityName = "Basingstoke", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "WIN", CityName = "Winchester", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "PTR", CityName = "Petersfield", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "CHI", CityName = "Chichester", Area = "West Sussex" });
                await connection.Create(new City() { CityCode = "BOG", CityName = "Bognor Regis", Area = "West Sussex" });
                await connection.Create(new City() { CityCode = "BOU", CityName = "Bournemouth", Area = "Dorset" });

                // Act
                PagedList<City> pagedList = await connection.ReadList<City>(null, null, 4, 1);

                // Assert
                Assert.AreEqual(4, pagedList.Rows.Count());
                Assert.AreEqual(false, pagedList.HasPrevious);
                Assert.AreEqual(true, pagedList.HasNext);
                Assert.AreEqual(2, pagedList.TotalPages);
                Assert.AreEqual(8, pagedList.TotalRows);
            }
        }

        [Test]
        public async Task Read_Paged_Neither_First_Or_Last_Page()
        {
            using (IDbConnection connection = LocalDbTestHelper.OpenTestConnection(TestContext.CurrentContext.Test.FullName))
            {
                // Arrange
                await connection.Create(new City() { CityCode = "PUP", CityName = "Portsmouth", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "SOU", CityName = "Southampton", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "BAS", CityName = "Basingstoke", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "WIN", CityName = "Winchester", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "PTR", CityName = "Petersfield", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "CHI", CityName = "Chichester", Area = "West Sussex" });
                await connection.Create(new City() { CityCode = "BOG", CityName = "Bognor Regis", Area = "West Sussex" });
                await connection.Create(new City() { CityCode = "BOU", CityName = "Bournemouth", Area = "Dorset" });

                // Act
                PagedList<City> pagedList = await connection.ReadList<City>(null, null, 2, 2);

                // Assert
                Assert.AreEqual(2, pagedList.Rows.Count());
                Assert.AreEqual(true, pagedList.HasPrevious);
                Assert.AreEqual(true, pagedList.HasNext);
                Assert.AreEqual(4, pagedList.TotalPages);
                Assert.AreEqual(8, pagedList.TotalRows);
            }
        }

        /// <summary>
        /// Test that we can get the last page when not specifying where conditions or a sort order.
        /// </summary>
        [Test]
        public async Task Read_Paged_Last_Page()
        {
            using (IDbConnection connection = LocalDbTestHelper.OpenTestConnection(TestContext.CurrentContext.Test.FullName))
            {
                // Arrange - seed some data
                await connection.Create(new City() { CityCode = "PUP", CityName = "Portsmouth", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "SOU", CityName = "Southampton", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "BAS", CityName = "Basingstoke", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "WIN", CityName = "Winchester", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "PTR", CityName = "Petersfield", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "CHI", CityName = "Chichester", Area = "West Sussex" });
                await connection.Create(new City() { CityCode = "BOG", CityName = "Bognor Regis", Area = "West Sussex" });
                await connection.Create(new City() { CityCode = "BOU", CityName = "Bournemouth", Area = "Dorset" });

                // Arrange - read the first page to see how many pages there are
                int lastPage = (await connection.ReadList<City>(null, null, 3, 1)).TotalPages;

                // Act - read the last page
                PagedList<City> pagedList = await connection.ReadList<City>(null, null, 3, lastPage);

                // Assert
                Assert.AreEqual(pagedList.TotalRows - ((lastPage - 1) * 3), pagedList.Rows.Count());
                Assert.AreEqual(true, pagedList.HasPrevious);
                Assert.AreEqual(false, pagedList.HasNext);
                Assert.AreEqual(lastPage, pagedList.TotalPages);
            }
        }

        /// <summary>
        /// Test that we can get the first page when specifying where conditions but no sort order.
        /// </summary>
        [Test]
        public async Task Read_Paged_With_Where()
        {
            using (IDbConnection connection = LocalDbTestHelper.OpenTestConnection(TestContext.CurrentContext.Test.FullName))
            {
                // Arrange
                await connection.Create(new City() { CityCode = "PUP", CityName = "Portsmouth", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "SOU", CityName = "Southampton", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "BAS", CityName = "Basingstoke", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "WIN", CityName = "Winchester", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "PTR", CityName = "Petersfield", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "CHI", CityName = "Chichester", Area = "West Sussex" });
                await connection.Create(new City() { CityCode = "BOG", CityName = "Bognor Regis", Area = "West Sussex" });
                await connection.Create(new City() { CityCode = "BOU", CityName = "Bournemouth", Area = "Dorset" });

                // Act
                PagedList<City> pagedList = await connection.ReadList<City>(new { Area = "Hampshire" }, null, 4, 1);

                // Assert
                Assert.AreEqual(4, pagedList.Rows.Count());
                Assert.AreEqual(false, pagedList.HasPrevious);
                Assert.AreEqual(true, pagedList.HasNext);
                Assert.AreEqual(2, pagedList.TotalPages);
                Assert.AreEqual(5, pagedList.TotalRows);
            }
        }

        /// <summary>
        /// Test that we can get the first page when specifying where conditions but no sort order.
        /// </summary>
        [Test]
        public async Task Read_Paged_With_Where_Dictionary()
        {
            using (IDbConnection connection = LocalDbTestHelper.OpenTestConnection(TestContext.CurrentContext.Test.FullName))
            {
                // Arrange
                await connection.Create(new City() { CityCode = "PUP", CityName = "Portsmouth", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "SOU", CityName = "Southampton", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "BAS", CityName = "Basingstoke", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "WIN", CityName = "Winchester", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "PTR", CityName = "Petersfield", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "CHI", CityName = "Chichester", Area = "West Sussex" });
                await connection.Create(new City() { CityCode = "BOG", CityName = "Bognor Regis", Area = "West Sussex" });
                await connection.Create(new City() { CityCode = "BOU", CityName = "Bournemouth", Area = "Dorset" });

                // Act
                PagedList<City> pagedList = await connection.ReadList<City>(
                    new Dictionary<string, object>() { { "Area", "Hampshire" } },
                    null,
                    4,
                    1);

                // Assert
                Assert.AreEqual(4, pagedList.Rows.Count());
                Assert.AreEqual(false, pagedList.HasPrevious);
                Assert.AreEqual(true, pagedList.HasNext);
                Assert.AreEqual(2, pagedList.TotalPages);
                Assert.AreEqual(5, pagedList.TotalRows);
            }
        }

        /// <summary>
        /// Test that we can get the rows in the required order.
        /// </summary>
        [Test]
        public async Task Read_Paged_With_OrderBy()
        {
            using (IDbConnection connection = LocalDbTestHelper.OpenTestConnection(TestContext.CurrentContext.Test.FullName))
            {
                // Arrange
                await connection.Create(new City() { CityCode = "PUP", CityName = "Portsmouth", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "SOU", CityName = "Southampton", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "BAS", CityName = "Basingstoke", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "WIN", CityName = "Winchester", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "PTR", CityName = "Petersfield", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "CHI", CityName = "Chichester", Area = "West Sussex" });
                await connection.Create(new City() { CityCode = "BOG", CityName = "Bognor Regis", Area = "West Sussex" });
                await connection.Create(new City() { CityCode = "BOU", CityName = "Bournemouth", Area = "Dorset" });

                // Act
                PagedList<City> pagedList = await connection.ReadList<City>(
                    null, 
                    new { Area = SortOrder.Descending, CityName = SortOrder.Ascending },
                    8,
                    1);

                // Assert
                string[] expectedOrder = new string[]
                {
                    "Bognor Regis",
                    "Chichester",
                    "Basingstoke",
                    "Petersfield",
                    "Portsmouth",
                    "Southampton",
                    "Winchester",
                    "Bournemouth"
                };
                
                Assert.IsTrue(pagedList.Rows.Select(x => x.CityName).SequenceEqual(expectedOrder));
            }
        }

        /// <summary>
        /// Test that we can get the rows in the required order.
        /// </summary>
        [Test]
        public async Task Read_Paged_With_OrderBy_Dictionary()
        {
            using (IDbConnection connection = LocalDbTestHelper.OpenTestConnection(TestContext.CurrentContext.Test.FullName))
            {
                // Arrange
                await connection.Create(new City() { CityCode = "PUP", CityName = "Portsmouth", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "SOU", CityName = "Southampton", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "BAS", CityName = "Basingstoke", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "WIN", CityName = "Winchester", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "PTR", CityName = "Petersfield", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "CHI", CityName = "Chichester", Area = "West Sussex" });
                await connection.Create(new City() { CityCode = "BOG", CityName = "Bognor Regis", Area = "West Sussex" });
                await connection.Create(new City() { CityCode = "BOU", CityName = "Bournemouth", Area = "Dorset" });

                // Act
                PagedList<City> pagedList = await connection.ReadList<City>(
                    null,
                    new Dictionary<string, SortOrder>() { { "Area", SortOrder.Descending }, { "CityName", SortOrder.Ascending } },
                    8,
                    1);

                // Assert
                string[] expectedOrder = new string[]
                {
                    "Bognor Regis",
                    "Chichester",
                    "Basingstoke",
                    "Petersfield",
                    "Portsmouth",
                    "Southampton",
                    "Winchester",
                    "Bournemouth"
                };

                Assert.IsTrue(pagedList.Rows.Select(x => x.CityName).SequenceEqual(expectedOrder));
            }
        }

        /// <summary>
        /// Test that we can get the first page when specifying where conditions and sort order.
        /// </summary>
        [Test]
        public async Task Read_Paged_With_Where_And_OrderBy()
        {
            using (IDbConnection connection = LocalDbTestHelper.OpenTestConnection(TestContext.CurrentContext.Test.FullName))
            {
                // Arrange
                await connection.Create(new City() { CityCode = "PUP", CityName = "Portsmouth", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "SOU", CityName = "Southampton", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "BAS", CityName = "Basingstoke", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "WIN", CityName = "Winchester", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "PTR", CityName = "Petersfield", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "CHI", CityName = "Chichester", Area = "West Sussex" });
                await connection.Create(new City() { CityCode = "BOG", CityName = "Bognor Regis", Area = "West Sussex" });
                await connection.Create(new City() { CityCode = "BOU", CityName = "Bournemouth", Area = "Dorset" });

                // Act
                PagedList<City> pagedList = await connection.ReadList<City>(
                    new { Area = "Hampshire" },
                    new { CityName = SortOrder.Descending },
                    5,
                    1);

                // Assert
                string[] expectedOrder = { "Winchester", "Southampton", "Portsmouth", "Petersfield", "Basingstoke" };
                Assert.IsTrue(pagedList.Rows.Select(x => x.CityName).SequenceEqual(expectedOrder));
            }
        }

        /// <summary>
        /// Test that we can get the first page when specifying where conditions and sort order.
        /// </summary>
        [Test]
        public async Task Read_Paged_With_Where_Dictionary_And_OrderBy_Dictionary()
        {
            using (IDbConnection connection = LocalDbTestHelper.OpenTestConnection(TestContext.CurrentContext.Test.FullName))
            {
                // Arrange
                await connection.Create(new City() { CityCode = "PUP", CityName = "Portsmouth", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "SOU", CityName = "Southampton", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "BAS", CityName = "Basingstoke", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "WIN", CityName = "Winchester", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "PTR", CityName = "Petersfield", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "CHI", CityName = "Chichester", Area = "West Sussex" });
                await connection.Create(new City() { CityCode = "BOG", CityName = "Bognor Regis", Area = "West Sussex" });
                await connection.Create(new City() { CityCode = "BOU", CityName = "Bournemouth", Area = "Dorset" });

                // Act
                PagedList<City> pagedList = await connection.ReadList<City>(
                    new Dictionary<string, object>() { { "Area", "Hampshire" } },
                    new Dictionary<string, SortOrder>() { { "CityName", SortOrder.Descending } },
                    5,
                    1);

                // Assert
                string[] expectedOrder = { "Winchester", "Southampton", "Portsmouth", "Petersfield", "Basingstoke" };
                Assert.IsTrue(pagedList.Rows.Select(x => x.CityName).SequenceEqual(expectedOrder));
            }
        }

        /// <summary>
        /// Test that we get an ArgumentException thrown when we send an invalid whereConditions object.
        /// </summary>
        [Test]
        public async Task Read_Paged_With_Invalid_Where()
        {
            using (IDbConnection connection = LocalDbTestHelper.OpenTestConnection(TestContext.CurrentContext.Test.FullName))
            {
                // Arrange
                await connection.Create(new City() { CityCode = "PUP", CityName = "Portsmouth", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "SOU", CityName = "Southampton", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "BAS", CityName = "Basingstoke", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "WIN", CityName = "Winchester", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "PTR", CityName = "Petersfield", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "CHI", CityName = "Chichester", Area = "West Sussex" });
                await connection.Create(new City() { CityCode = "BOG", CityName = "Bognor Regis", Area = "West Sussex" });
                await connection.Create(new City() { CityCode = "BOU", CityName = "Bournemouth", Area = "Dorset" });

                // Act / Assert
                Assert.ThrowsAsync<ArgumentException>(async () => { await connection.ReadList<City>(new { City = "PUP" }, null, 1, 1); });
            }
        }

        /// <summary>
        /// Test that we get an ArgumentException thrown when we send an invalid whereConditions object.
        /// </summary>
        [Test]
        public async Task Read_Paged_With_Invalid_Where_Dictionary()
        {
            using (IDbConnection connection = LocalDbTestHelper.OpenTestConnection(TestContext.CurrentContext.Test.FullName))
            {
                // Arrange
                await connection.Create(new City() { CityCode = "PUP", CityName = "Portsmouth", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "SOU", CityName = "Southampton", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "BAS", CityName = "Basingstoke", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "WIN", CityName = "Winchester", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "PTR", CityName = "Petersfield", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "CHI", CityName = "Chichester", Area = "West Sussex" });
                await connection.Create(new City() { CityCode = "BOG", CityName = "Bognor Regis", Area = "West Sussex" });
                await connection.Create(new City() { CityCode = "BOU", CityName = "Bournemouth", Area = "Dorset" });

                // Act / Assert
                Assert.ThrowsAsync<ArgumentException>(async () => 
                { 
                    await connection.ReadList<City>(new Dictionary<string, object> { { "City", "PUP" } }, null, 1, 1); 
                });
            }
        }

        /// <summary>
        /// Test that we get an ArgumentException thrown when we send an invalid sort orders object.
        /// </summary>
        [Test]
        public async Task Read_Paged_With_Invalid_OrderBy()
        {
            using (IDbConnection connection = LocalDbTestHelper.OpenTestConnection(TestContext.CurrentContext.Test.FullName))
            {
                // Arrange
                await connection.Create(new City() { CityCode = "PUP", CityName = "Portsmouth", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "SOU", CityName = "Southampton", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "BAS", CityName = "Basingstoke", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "WIN", CityName = "Winchester", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "PTR", CityName = "Petersfield", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "CHI", CityName = "Chichester", Area = "West Sussex" });
                await connection.Create(new City() { CityCode = "BOG", CityName = "Bognor Regis", Area = "West Sussex" });
                await connection.Create(new City() { CityCode = "BOU", CityName = "Bournemouth", Area = "Dorset" });

                // Act / Assert
                Assert.ThrowsAsync<ArgumentException>(async () => 
                { 
                    await connection.ReadList<City>(null, new { County = SortOrder.Descending }, 1, 1); 
                });
            }
        }

        /// <summary>
        /// Test that we get an ArgumentException thrown when we send an invalid sort orders object.
        /// </summary>
        [Test]
        public async Task Read_Paged_With_Invalid_OrderBy_Dictionary()
        {
            using (IDbConnection connection = LocalDbTestHelper.OpenTestConnection(TestContext.CurrentContext.Test.FullName))
            {
                // Arrange
                await connection.Create(new City() { CityCode = "PUP", CityName = "Portsmouth", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "SOU", CityName = "Southampton", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "BAS", CityName = "Basingstoke", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "WIN", CityName = "Winchester", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "PTR", CityName = "Petersfield", Area = "Hampshire" });
                await connection.Create(new City() { CityCode = "CHI", CityName = "Chichester", Area = "West Sussex" });
                await connection.Create(new City() { CityCode = "BOG", CityName = "Bognor Regis", Area = "West Sussex" });
                await connection.Create(new City() { CityCode = "BOU", CityName = "Bournemouth", Area = "Dorset" });

                // Act / Assert
                Assert.ThrowsAsync<ArgumentException>(async () =>
                {
                    await connection.ReadList<City>(null, new Dictionary<string, SortOrder> { { "County", SortOrder.Descending } }, 1, 1);
                });
            }
        }
    }
}