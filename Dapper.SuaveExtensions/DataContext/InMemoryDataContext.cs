using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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

        /// <summary>
        /// Adds test data for the specific type.
        /// If data already exists it will be overwritten.
        /// </summary>
        /// <typeparam name="T">The type of the test data.</typeparam>
        /// <param name="data">The data.</param>
        public static void AddOrUpdateData<T>(IEnumerable<T> data)
        {
            dataStore[typeof(T).FullName] = new List<T>(data);
        }

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
            // get the type map
            TypeMap type = TypeMap.GetTypeMap<T>();

            // validate the key
            id = type.ValidateKeyProperties(id);

            // get the data to query
            IList<T> list = GetData<T>();
            IQueryable<T> data = GetData<T>().AsQueryable<T>();

            // if no items then nothing to query
            if (data.Count() == 0)
            {
                return null;
            }

            // x =>
            ParameterExpression parameter = Expression.Parameter(typeof(T), "x");
            BinaryExpression body = null;
            foreach (string key in ((IDictionary<string, object>)id).Keys)
            {
                // add a paramater equals expression for each property in the key
                PropertyMap pm = type.AllKeys.Single(x => x.Property == key);
                object value = ((IDictionary<string, object>)id)[key];

                // x.Property
                MemberExpression member = Expression.Property(parameter, key);
                ConstantExpression constant = Expression.Constant(value);

                // x.Property = Value
                if (body == null)
                {
                    body = Expression.Equal(member, constant);
                }
                else
                {
                    body = Expression.AndAlso(body, Expression.Equal(member, constant));
                }
            }

            var finalExpression = Expression.Lambda<Func<T, bool>>(body, parameter);

            return Task.FromResult(data.Where(finalExpression).SingleOrDefault());
        }

        /// <inheritdoc />
        public Task<IEnumerable<T>> ReadAll<T>()
        {
            if (dataStore.ContainsKey(typeof(T).FullName))
            {
                return Task.FromResult((IEnumerable<T>)dataStore[typeof(T).FullName]);
            }

            return Task.FromResult(new T[0].AsEnumerable());
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

        private static IList<T> GetData<T>()
        {
            string cacheKey = typeof(T).FullName;

            if (!dataStore.ContainsKey(cacheKey))
            {
                dataStore[cacheKey] = new List<T>();
            }

            return dataStore[cacheKey] as IList<T>;
        }
    }
}
