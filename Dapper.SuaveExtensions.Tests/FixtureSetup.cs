using System;
using System.Data.SqlClient;
using System.Threading;

using NUnit.Framework;

namespace Dapper.SuaveExtensions.Tests
{
    /// <summary>
    /// Class that contains Setup methods for the Test Fixtures in this Assembly
    /// Contains the OneTimeSetup and OneTimeTearDown operations
    /// </summary>
    [SetUpFixture]
    public class FixtureSetup
    {
        private const string localDbDataSource = @"(LocalDb)\MSSQLLocalDB";
        private const string localDbCatalog = @"SuaveExtensionsTextDb";

        /// <summary>
        /// Gets the local database master connection string.
        /// </summary>
        /// <value>
        /// The local database master connection string.
        /// </value>
        private static string LocalDbMasterConnectionString
        {
            get { return $"Data Source={localDbDataSource};Initial Catalog=master;Integrated Security = True"; }
        }

        /// <summary>
        /// Gets the local database connecting string.
        /// </summary>
        /// <value>
        /// The local database connecting string.
        /// </value>
        public static string LocalDbConnectionString
        {
            get { return $"Data Source={localDbDataSource};Initial Catalog={localDbCatalog};Integrated Security = True"; }
        }

        /// <summary>
        /// Gets or sets a Lock Object used with a Monitor to ensure tests run one at a time
        /// </summary>
        public static SemaphoreSlim LockObject { get; set; } = new SemaphoreSlim(1, 1);

        [OneTimeSetUp]
        public static void OneTimeSetup()
        {
            SetupLocalDb();
        }

        [OneTimeTearDown]
        public static void OneTimeTearDown()
        {
        }

        internal static void TestDataTearDown()
        {
            // delete values created during tests
            // TODO: replace with Delete extension methods
            using (SqlConnection connection = new SqlConnection(FixtureSetup.LocalDbConnectionString))
            {
                connection.Open();

                connection.Execute("DELETE FROM Cities");
                connection.Execute("DELETE FROM CitiesManual");
                connection.Execute("DELETE FROM CitiesSequential");
                connection.Execute("DELETE FROM Itinerary");
                connection.Execute("DELETE FROM ElementTable");
                connection.Execute("DELETE FROM DateStampTest");
            }
        }

        private static void SetupLocalDb()
        {
            using (SqlConnection connection = new SqlConnection(LocalDbMasterConnectionString))
            {
                connection.Open();
                try
                {
                    connection.Execute($@"DROP DATABASE {localDbCatalog};");
                }
                catch (Exception)
                { }

                connection.Execute($@"CREATE DATABASE {localDbCatalog};");
            }

            using (SqlConnection connection = new SqlConnection(LocalDbConnectionString))
            {
                connection.Open();

                connection.Execute(
                    @"CREATE TABLE Cities
                      (
                        CityId INT IDENTITY(1, 1) NOT NULL,
                        CityCode NVARCHAR(5) NOT NULL,
                        Name NVARCHAR(100) NOT NULL,
                        Area NVARCHAR(100) NOT NULL,
                        PRIMARY KEY (CityId),
                        CONSTRAINT AK_Cities_CityCode UNIQUE(CityCode)
                      )");

                connection.Execute(
                    @"CREATE TABLE CitiesManual
                        (
                            CityCode NVARCHAR(5) NOT NULL,
                            Name NVARCHAR(100) NOT NULL,
                            PRIMARY KEY (CityCode),
                            CONSTRAINT AK_CitiesManual_CityCode UNIQUE(CityCode)
                        )");

                connection.Execute(
                    @"CREATE TABLE CitiesSequential
                        (
                            CityId SMALLINT NOT NULL,
                            CityCode NVARCHAR(5) NOT NULL,
                            Name NVARCHAR(100) NOT NULL,
                            PRIMARY KEY (CityId),
                            CONSTRAINT AK_CitiesSequential_CityCode UNIQUE(CityCode)
                        )");

                connection.Execute(
                    @"CREATE TABLE Itinerary
                        (
                            BookingId INT NOT NULL,
                            ItineraryId INT NOT NULL,
                            Title NVARCHAR(100) NOT NULL,
                            PRIMARY KEY (BookingId, ItineraryId),
                        )");

                connection.Execute(
                    @"CREATE TABLE ElementTable
                        (
                            BookingId INT NOT NULL,
                            ItinId INT NOT NULL,
                            ElementId INT NOT NULL,
                            Title NVARCHAR(100) NOT NULL,
                            PRIMARY KEY (BookingId, ItinId, ElementId),
                        )");

                connection.Execute(
                    @"CREATE TABLE DateStampTest
                        (
                            Name NVARCHAR(50) NOT NULL,
                            Value NVARCHAR(50) NOT NULL,
                            InsertDate DATETIME2 NOT NULL,
                            UpdateDate DATETIME2 NOT NULL,
                            PRIMARY KEY (Name),
                        )");

                connection.Execute(@"CREATE SCHEMA Suave");
                connection.Execute(@"CREATE TABLE [Suave].[SoftDeleteTest]
                                    (
                                        SoftDeleteId INT IDENTITY(1, 1) NOT NULL,
                                        RecordStatus INT NOT NULL
                                        PRIMARY KEY (SoftDeleteId),
                                    )");

                connection.Execute(@"CREATE TABLE ReadOnly
                                    (
                                        SequentialId INT NOT NULL,
                                        Editable NVARCHAR(100) NOT NULL,
                                        ReadOnly NVARCHAR(100) NOT NULL,
                                        PRIMARY KEY (SequentialId),
                                    )");
            }
        }
    }
}
