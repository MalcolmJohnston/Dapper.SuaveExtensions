using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Dapper.SuaveExtensions.Map;

namespace Dapper.SuaveExtensions.DataContext
{
    /// <summary>
    /// In memory data context.  Provided to support a stub based approach to unit testing.
    /// Can also be used for quick proof of concepts.
    /// </summary>
    /// <seealso cref="Dapper.SuaveExtensions.DataContext.IDataContext" />
    public class InMemoryDataContext : IDataContext
    {
        private static ConcurrentDictionary<string, IList> dataStore = new ConcurrentDictionary<string, IList>();

        /// <inheritdoc />
        public Task<T> Create<T>(T entity)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task Delete<T>(object id)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task DeleteList<T>(object whereConditions)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<T> Read<T>(object id)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<IEnumerable<T>> ReadAll<T>()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<IEnumerable<T>> ReadList<T>(object whereConditions)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<T> Update<T>(object properties)
        {
            throw new NotImplementedException();
        }

        private static void InitialiseBag<T>()
        {
            string cacheKey = typeof(T).FullName;

            if (!dataStore.ContainsKey(cacheKey))
            {
                dataStore[cacheKey] = new List<T>();
            }
        }
    }
}
