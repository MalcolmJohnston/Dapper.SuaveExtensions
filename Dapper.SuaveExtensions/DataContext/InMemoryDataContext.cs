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
        private ConcurrentDictionary<string, IList> dataStore = new ConcurrentDictionary<string, IList>();

        /// <summary>
        /// Adds test data for the specific type.
        /// If data already exists it will be overwritten.
        /// </summary>
        /// <typeparam name="T">The type of the test data.</typeparam>
        /// <param name="data">The data.</param>
        public void AddOrUpdate<T>(IEnumerable<T> data)
        {
            this.dataStore[typeof(T).FullName] = new List<T>(data);
        }

        /// <inheritdoc />
        public Task<T> Create<T>(T entity)
        {
            // get the type map
            TypeMap type = TypeMap.GetTypeMap<T>();

            // get the current list for this type
            IList<T> list = this.GetData<T>();
            IQueryable<T> linqList = list.AsQueryable();

            // if we have a sequential key then set the value
            if (type.HasSequentialKey)
            {
                // initialise the sequential key value
                int sequentialKeyValue = 0;

                // create a list of objects that already have their sequential key set
                List<T> existingCandidates = new List<T>(list);

                if (existingCandidates.Count() > 0)
                {
                    if (type.AssignedKeys.Count() > 0)
                    {
                        // if this type has assigned keys then filter out objects from our candidates that do not have
                        // matching assigned keys
                        IDictionary<string, object> assignedValues = type.AssignedKeys
                            .Select(kvp => new KeyValuePair<string, object>(kvp.Property, kvp.PropertyInfo.GetValue(entity)))
                            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                        existingCandidates = this.ReadList<T>(assignedValues).GetAwaiter().GetResult().ToList();
                    }

                    // now get the last item that was added to the list in order of sequential key
                    T lastIn = ((IQueryable<T>)existingCandidates).OrderByDescending<T>(type.SequentialKey.Property).FirstOrDefault();
                    if (lastIn != null)
                    {
                        sequentialKeyValue = (int)type.SequentialKey.PropertyInfo.GetValue(lastIn);
                    }
                }

                // increment and set the sequential key value on the object
                type.SequentialKey.PropertyInfo.SetValue(entity, sequentialKeyValue + 1);
            }
            else if (type.HasIdentityKey)
            {
                // initialise the identity key value
                int identityKeyValue = 0;

                if (linqList.Count() > 0)
                {
                    // now get the last item that was added to the list in order of sequential key
                    T lastIn = linqList.OrderByDescending<T>(type.IdentityKey.Property).FirstOrDefault();
                    if (lastIn != null)
                    {
                        identityKeyValue = (int)type.IdentityKey.PropertyInfo.GetValue(lastIn);
                    }
                }

                // increment and set the sequential key value on the object
                type.IdentityKey.PropertyInfo.SetValue(entity, identityKeyValue + 1);
            }

            // now set any date stamp properties
            foreach (PropertyMap dateStampProperty in type.DateStampProperties)
            {
                dateStampProperty.PropertyInfo.SetValue(entity, DateTime.Now);
            }

            // and any soft delete properties
            if (type.HasSoftDelete)
            {
                type.SoftDeleteProperty.PropertyInfo.SetValue(
                    entity,
                    type.SoftDeleteProperty.InsertedValue);
            }

            // finally add the item to the list
            list.Add(entity);

            return Task.FromResult(entity);
        }

        /// <inheritdoc />
        public Task Delete<T>(object id)
        {
            // get the existing object
            T obj = this.Read<T>(id).GetAwaiter().GetResult();

            // check the object is not null (i.e. it exists in the collection)
            if (obj != null)
            {
                // remove the object from the collection
                IList<T> list = this.GetData<T>();
                list.Remove(obj);
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task DeleteList<T>(object whereConditions)
        {
            // get the existing objects
            IEnumerable<T> objects = this.ReadList<T>(whereConditions).GetAwaiter().GetResult();

            if (objects.Count() > 0)
            {
                IList<T> list = this.GetData<T>();
                foreach (T obj in objects)
                {
                    list.Remove(obj);
                }
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<T> Read<T>(object id)
        {
            // get the type map
            TypeMap type = TypeMap.GetTypeMap<T>();

            // validate the key
            id = type.ValidateKeyProperties(id);

            return Task.FromResult(this.ReadWhere<T>((IDictionary<string, object>)id).SingleOrDefault());
        }

        /// <inheritdoc />
        public Task<IEnumerable<T>> ReadAll<T>()
        {
            if (this.dataStore.ContainsKey(typeof(T).FullName))
            {
                return Task.FromResult((IEnumerable<T>)this.dataStore[typeof(T).FullName]);
            }

            return Task.FromResult(new T[0].AsEnumerable());
        }

        /// <inheritdoc />
        public Task<IEnumerable<T>> ReadList<T>(object whereConditions)
        {
            // get the type map
            TypeMap type = TypeMap.GetTypeMap<T>();

            // validate the where conditions
            whereConditions = type.CoalesceObject(whereConditions);
            type.ValidateWhereProperties(whereConditions);

            return Task.FromResult(this.ReadWhere<T>((IDictionary<string, object>)whereConditions));
        }

        /// <inheritdoc />
        public Task<T> Update<T>(object properties)
        {
            // get the type map
            TypeMap type = TypeMap.GetTypeMap<T>();

            // validate the key
            object id = type.ValidateKeyProperties(properties);

            // get the existing object
            T obj = this.Read<T>(id).GetAwaiter().GetResult();

            if (obj != null)
            {
                // find the properties to update
                IDictionary<string, object> allProps = (IDictionary<string, object>)properties;
                IDictionary<string, object> idProps = type.CoalesceKeyObject(id);
                IDictionary<string, object> updateProps = allProps.Where(kvp => !idProps.ContainsKey(kvp.Key))
                                                                  .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                // update the properties
                foreach (string propertyName in updateProps.Keys)
                {
                    PropertyMap propertyMap = type.UpdateableProperties.Where(x => x.Property == propertyName).SingleOrDefault();

                    if (propertyMap != null)
                    {
                        propertyMap.PropertyInfo.SetValue(obj, updateProps[propertyName]);
                    }
                }
            }

            return Task.FromResult(obj);
        }

        private IList<T> GetData<T>()
        {
            string cacheKey = typeof(T).FullName;

            if (!this.dataStore.ContainsKey(cacheKey))
            {
                this.dataStore[cacheKey] = new List<T>();
            }

            return this.dataStore[cacheKey] as IList<T>;
        }

        private IEnumerable<T> ReadWhere<T>(IDictionary<string, object> properties)
        {
            // get the type map
            TypeMap type = TypeMap.GetTypeMap<T>();

            // get the data to query
            IList<T> list = this.GetData<T>();
            IQueryable<T> data = this.GetData<T>().AsQueryable<T>();

            // return an empty enumerable if no objects in collection
            if (data.Count() == 0)
            {
                return new T[0];
            }

            // x =>
            ParameterExpression parameter = Expression.Parameter(typeof(T), "x");
            BinaryExpression body = null;
            foreach (string propertyName in properties.Keys)
            {
                // add a paramater equals expression for each property in the property bag
                PropertyMap pm = type.AllProperties[propertyName];

                // x.Property
                MemberExpression member = Expression.Property(parameter, pm.Property);
                ConstantExpression constant = Expression.Constant(properties[propertyName]);

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

            return data.Where(finalExpression).AsEnumerable();
        }
    }
}
