using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

using System.Linq;
using System.Linq.Dynamic.Core;
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
                // alias the property info
                PropertyInfo pi = type.SequentialKey.PropertyInfo;

                // initialise the key value
                dynamic keyValue = Activator.CreateInstance(pi.PropertyType);

                // check for existing entities
                if (list.Any())
                {
                    List<T> existingEntities = new List<T>(list);
                    if (type.AssignedKeys.Count() > 0)
                    {
                        // if this type has assigned keys then filter out objects from our candidates that do not have
                        // matching assigned keys
                        IDictionary<string, object> assignedValues = type.AssignedKeys
                            .Select(kvp => new KeyValuePair<string, object>(kvp.Property, kvp.PropertyInfo.GetValue(entity)))
                            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                        existingEntities = this.ReadList<T>(assignedValues).GetAwaiter().GetResult().ToList();
                    }

                    // now get the last item that was added to the list in order of the key
                    T lastIn = existingEntities.AsQueryable().OrderBy($"{pi.Name} DESC").FirstOrDefault();
                    keyValue = lastIn != null ? pi.GetValue(lastIn) : keyValue;
                }

                // increment and set the key value on the object
                Expression incrementExpr = Expression.Increment(Expression.Constant(keyValue));
                pi.SetValue(entity, Expression.Lambda(incrementExpr).Compile().DynamicInvoke());
            }
            else if (type.HasIdentityKey)
            {
                // alias the property info
                PropertyInfo pi = type.IdentityKey.PropertyInfo;

                // initialise the key value to zero for the key type
                dynamic keyValue = Activator.CreateInstance(pi.PropertyType);

                // now get the last item that was added to the list in order of the key
                T lastIn = linqList.OrderBy($"{pi.Name} DESC").FirstOrDefault();
                keyValue = lastIn != null ? pi.GetValue(lastIn) : keyValue;

                // increment and set the key value on the object
                Expression incrementExpr = Expression.Increment(Expression.Constant(keyValue));
                pi.SetValue(entity, Expression.Lambda(incrementExpr).Compile().DynamicInvoke());
            }

            // now set any date stamp properties
            if (type.DateStampProperties.Any())
            {
                DateTime timeStamp = DateTime.Now;
                foreach (PropertyMap dateStampProperty in type.DateStampProperties)
                {
                    dateStampProperty.PropertyInfo.SetValue(entity, timeStamp);
                }
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
            IEnumerable<T> objects = new List<T>(this.ReadList<T>(whereConditions).GetAwaiter().GetResult());

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
            IDictionary<string, object> id = type.ValidateKeyProperties(properties);

            // get the existing object
            T obj = this.ReadWhere<T>(id).SingleOrDefault();

            if (obj != null)
            {
                // find the properties to update
                IDictionary<string, object> allProps = type.CoalesceObject(properties);
                IDictionary<string, object> updateProps = allProps.Where(kvp => !id.ContainsKey(kvp.Key))
                                                                  .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                // update the properties that are not date stamps
                foreach (string propertyName in updateProps.Keys)
                {
                    PropertyMap propertyMap = type.UpdateableProperties.Where(x => x.Property == propertyName).SingleOrDefault();
                    if (propertyMap != null)
                    {
                        propertyMap.PropertyInfo.SetValue(obj, updateProps[propertyName]);
                    }
                }

                // update the properties that are date stamps (and updateable)
                if (type.DateStampProperties.Any(x => !x.IsReadOnly))
                {
                    DateTime dateStamp = DateTime.Now;
                    foreach (PropertyMap propertyMap in type.DateStampProperties.Where(x => !x.IsReadOnly))
                    {
                        propertyMap.PropertyInfo.SetValue(obj, dateStamp);
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

            return data.Where(finalExpression).AsEnumerable<T>();
        }
    }
}
