using System;
using System.Collections.Generic;
using System.Text;

using Dapper.SuaveExtensions.DataContext;

using NUnit.Framework;

namespace Dapper.SuaveExtensions.Tests
{
    public class DataContextTestHelper
    {
        public static IDataContext GetDataContext(Type dataContextType)
        {
            // check whether we are dealing with a type that implement IDataContext
            if (typeof(IDataContext).IsAssignableFrom(dataContextType) == false)
            {
                throw new ArgumentException($"Type {dataContextType.Name} does not implement IDataContext.");
            }

            // return the data context for the given type
            if (typeof(InMemoryDataContext).IsAssignableFrom(dataContextType))
            {
                return new InMemoryDataContext();
            }
            else if (typeof(SqlServerDataContext).IsAssignableFrom(dataContextType))
            {
                // create the test database
                LocalDbTestHelper.CreateTestDatabase(TestContext.CurrentContext.Test.FullName);
                
                // return data context
                return new SqlServerDataContext(LocalDbTestHelper.GetTestConnectionString(TestContext.CurrentContext.Test.FullName));
            }
            else
            {
                throw new ArgumentException($"Type {dataContextType} is not supported, add support in {nameof(DataContextTestHelper)}.cs");
            }
        }

        public static void DeleteDataContext(Type dataContextType)
        {
            // check whether we are dealing with a type that we need to dispose of
            if (typeof(SqlServerDataContext).IsAssignableFrom(dataContextType))
            {
                // create the test database
                LocalDbTestHelper.DeleteTestDatabase(TestContext.CurrentContext.Test.FullName);
            }
        }
    }
}
