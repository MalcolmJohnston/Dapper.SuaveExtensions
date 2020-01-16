using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Dynamic;
using System.Linq;
using System.Reflection;

using Dapper.SuaveExtensions.DataAnnotations;

namespace Dapper.SuaveExtensions.Map
{
    /// <summary>
    /// A class representing a type to database table mapping.
    /// </summary>
    public class TypeMap
    {
        private static ConcurrentDictionary<string, TypeMap> typeMapCache = new ConcurrentDictionary<string, TypeMap>();

        /// <summary>
        /// Gets or sets the mapped type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public Type Type { get; set; }

        /// <summary>
        /// Gets or sets the schema this type is mapped to.
        /// e.g. dbo.
        /// </summary>
        /// <value>
        /// The schema.
        /// </value>
        public string Schema { get; set; }

        /// <summary>
        /// Gets or sets the name of the table this type is mapped to.
        /// </summary>
        /// <value>
        /// The name of the table.
        /// </value>
        public string TableName { get; set; }

        /// <summary>
        /// Gets the table identifier.
        /// i.e. the schema and table name combination.
        /// </summary>
        /// <value>
        /// The table identifier.
        /// </value>
        public string TableIdentifier
        {
            get
            {
                return !string.IsNullOrWhiteSpace(this.Schema) ? $"[{this.Schema}].[{this.TableName}]" : $"[{this.TableName}]";
            }
        }

        /// <summary>
        /// Gets all the properties belonging to this type, indexed by property name.
        /// </summary>
        public IDictionary<string, PropertyMap> AllProperties { get; private set; }

        /// <summary>
        /// Gets the key properties.
        /// </summary>
        /// <value>
        /// The key properties.
        /// </value>
        public IList<PropertyMap> AllKeys { get; private set; }

        /// <summary>
        /// Gets the sequential key.
        /// </summary>
        /// <value>
        /// The sequential key.
        /// </value>
        public PropertyMap SequentialKey { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance has sequential key.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has sequential key; otherwise, <c>false</c>.
        /// </value>
        public bool HasSequentialKey
        {
            get { return this.SequentialKey != null; }
        }

        /// <summary>
        /// Gets the manual keys.
        /// </summary>
        /// <value>
        /// The manual keys.
        /// </value>
        public IEnumerable<PropertyMap> AssignedKeys
        {
            get { return this.AllKeys.Where(x => x.KeyType == KeyType.Assigned); }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has manual keys.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has manual keys; otherwise, <c>false</c>.
        /// </value>
        public bool HasManualKeys
        {
            get { return this.AssignedKeys.Count() > 0; }
        }

        /// <summary>
        /// Gets the identity keys.
        /// </summary>
        /// <value>
        /// The identity keys.
        /// </value>
        public PropertyMap IdentityKey
        {
            get { return this.AllKeys.Where(x => x.KeyType == KeyType.Identity).SingleOrDefault(); }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has identity keys.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has identity keys; otherwise, <c>false</c>.
        /// </value>
        public bool HasIdentityKey
        {
            get { return this.IdentityKey != null; }
        }

        /// <summary>
        /// Gets the select properties.
        /// </summary>
        /// <value>
        /// The select properties.
        /// </value>
        public IList<PropertyMap> SelectProperties { get; private set; }

        /// <summary>
        /// Gets the required properties.
        /// </summary>
        /// <value>
        /// The required properties.
        /// </value>
        public IList<PropertyMap> RequiredProperties { get; private set; }

        /// <summary>
        /// Gets the insertable properties.
        /// </summary>
        /// <value>
        /// The insertable properties.
        /// </value>
        public IList<PropertyMap> InsertableProperties { get; private set; }

        /// <summary>
        /// Gets the updateable properties.
        /// </summary>
        /// <value>
        /// The updateable properties.
        /// </value>
        public IList<PropertyMap> UpdateableProperties { get; private set; }

        /// <summary>
        /// Gets or the date stamp properties.
        /// </summary>
        /// <value>
        /// The date stamp properties.
        /// </value>
        public IList<PropertyMap> DateStampProperties { get; private set; }

        /// <summary>
        /// Gets the soft delete property.
        /// </summary>
        /// <value>
        /// The soft delete property.
        /// </value>
        public PropertyMap SoftDeleteProperty { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance has soft delete.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has soft delete; otherwise, <c>false</c>.
        /// </value>
        public bool HasSoftDelete
        {
            get { return this.SoftDeleteProperty != null; }
        }

        /// <summary>
        /// Gets a dictionary indexed by property name paired with the default sort order (Ascending).
        /// </summary>
        public IDictionary<string, SortOrder> DefaultSortOrder { get; private set; }

        /// <summary>
        /// Gets a type map from the cache or adds a new one to the cache.
        /// </summary>
        /// <typeparam name="T">The type of the type map.</typeparam>
        /// <returns>The type map for this type.</returns>
        public static TypeMap GetTypeMap<T>()
        {
            string cacheKey = typeof(T).FullName;
            if (!typeMapCache.ContainsKey(cacheKey))
            {
                typeMapCache[cacheKey] = TypeMap.LoadTypeMapping<T>();
            }

            return typeMapCache[cacheKey];
        }

        /// <summary>
        /// Validates the key properties.
        /// Converts to a dictionary when value type is passed and we have an identity key.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <exception cref="ArgumentException">Thrown if a key property is not passed on the object.</exception>
        /// <returns>The validated key property bag.</returns>
        public IDictionary<string, object> ValidateKeyProperties(object id)
        {
            if (id == null)
            {
                throw new ArgumentException("Passed identifier object is null.");
            }

            // create dictionary
            ExpandoObject eo = new ExpandoObject();
            IDictionary<string, object> keys = eo as IDictionary<string, object>;

            // if we have a single key on our dto and the passed id object is the same type as the key
            // then return a dictionary indexed on the key property name
            if (this.AllKeys.Count() == 1 && this.AllKeys.Single().PropertyInfo.PropertyType == id.GetType())
            {
                keys.Add(this.AllKeys.Single().Property, id);
            }
            else
            {
                // otherwise iterate through all properties and check we have all key properties
                PropertyInfo[] propertyInfos = id.GetType().GetProperties();
                foreach (PropertyMap propertyMap in this.AllKeys)
                {
                    PropertyInfo pi = propertyInfos.Where(x => x.Name == propertyMap.Property).SingleOrDefault();

                    if (pi == null)
                    {
                        throw new ArgumentException($"Failed to find key property {propertyMap.Property}.");
                    }

                    keys.Add(propertyMap.Property, pi.GetValue(id));
                }
            }

            return eo;
        }

        /// <summary>
        /// Coalesces a dictionary from a property bag.
        /// </summary>
        /// <param name="propertyBag">The property bag.</param>
        /// <returns>Dictionary representing the objects properties.</returns>
        /// <exception cref="ArgumentException">
        /// Passed property bag is null
        /// or
        /// Failed to find property {propertyInfo.Name}.
        /// </exception>
        public IDictionary<string, object> CoalesceToDictionary(object propertyBag)
        {
            if (propertyBag == null)
            {
                return new Dictionary<string, object>();
            }

            // if we have an expando object or dictionary already then return it
            if (propertyBag is IDictionary<string, object>)
            {
                return propertyBag as IDictionary<string, object>;
            }

            IDictionary<string, object> obj = new Dictionary<string, object>();
            PropertyInfo[] propertyInfos = propertyBag.GetType().GetProperties();
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                if (!this.AllProperties.TryGetValue(propertyInfo.Name, out PropertyMap pm))
                {
                    throw new ArgumentException($"Failed to find property {propertyInfo.Name}.");
                }

                obj.Add(propertyInfo.Name, propertyInfo.GetValue(propertyBag));
            }

            return obj;
        }

        /// <summary>
        /// Coalesces a dictionary representing the primary key.
        /// </summary>
        /// <param name="propertyBag">The property bag.</param>
        /// <returns>Dictionary representing the primary key.</returns>
        /// <exception cref="ArgumentException">
        /// Passed property bag is null
        /// or
        /// Failed to find key property {propertyMap.Property}.
        /// </exception>
        public IDictionary<string, object> CoalesceKeyToDictionary(object propertyBag)
        {
            if (propertyBag == null)
            {
                throw new ArgumentException("Passed property bag is null.");
            }

            IDictionary<string, object> key = new Dictionary<string, object>();
            PropertyInfo[] propertyInfos = propertyBag.GetType().GetProperties();
            foreach (PropertyMap propertyMap in this.AllKeys)
            {
                PropertyInfo pi = propertyInfos.Where(x => x.Name == propertyMap.Property).SingleOrDefault();

                if (pi == null)
                {
                    throw new ArgumentException($"Failed to find key property {propertyMap.Property}.");
                }

                key.Add(propertyMap.Property, pi.GetValue(propertyBag));
            }

            return key;
        }

        /// <summary>
        /// Coalesces a dictionary representing the requested sort orders.
        /// If no sort orders are passed then return a default ordering (ascending for all key fields).
        /// </summary>
        /// <param name="sortOrders">The sort orders.</param>
        /// <returns>Dictionary representing the sort orders (indexed by column name).</returns>
        /// <exception cref="ArgumentException">
        /// Failed to find key property {propertyMap.Property}.
        /// or
        /// Must pass a sort order value.
        /// </exception>
        public IDictionary<string, SortOrder> CoalesceSortOrderDictionary(object sortOrders)
        {
            // only attempt to coalesce if we don't have a null reference
            if (sortOrders != null)
            {
                // initialise our dictionary
                IDictionary<string, SortOrder> sortOrdersDict = new Dictionary<string, SortOrder>();

                // if we were passed a dictionary of the same type then return it as long as it has one or more values
                if (sortOrders is IDictionary<string, SortOrder>)
                {
                    sortOrdersDict = sortOrders as IDictionary<string, SortOrder>;
                }
                else
                {
                    // coalesce the object
                    foreach (PropertyInfo propertyInfo in sortOrders.GetType().GetProperties())
                    {
                        if (propertyInfo.PropertyType != typeof(SortOrder))
                        {
                            throw new ArgumentException($"Must pass a valid sort order value.");
                        }

                        sortOrdersDict.Add(propertyInfo.Name, (SortOrder)propertyInfo.GetValue(sortOrders));
                    }
                }

                // validate and return
                if (sortOrdersDict.Any())
                {
                    return sortOrdersDict;
                }
            }

            return this.DefaultSortOrder;
        }

        /// <summary>
        /// Validates the where properties.
        /// </summary>
        /// <param name="whereConditions">The where conditions.</param>
        /// <returns>A list of validated property maps.</returns>
        /// <exception cref="ArgumentException">
        /// Please pass where conditions.
        /// or
        /// Please specify at least one property for a WHERE condition.
        /// or
        /// Failed to find property {property.Name}.
        /// </exception>
        public IList<PropertyMap> ValidateWhereProperties(object whereConditions)
        {
            // coalesce the object to a dictionary
            IDictionary<string, object> whereDict = this.CoalesceToDictionary(whereConditions);

            // check we have some conditions to create
            if (whereDict.Count == 0)
            {
                throw new ArgumentException("Please pass where conditions.");
            }

            // setup our list of property mappings that we will create the where clause from
            List<PropertyMap> propertyMappings = new List<PropertyMap>();

            foreach (string propertyName in whereDict.Keys)
            {
                PropertyMap propertyMap = this.SelectProperties
                                              .SingleOrDefault(x => x.Property == propertyName);

                if (propertyMap == null)
                {
                    throw new ArgumentException($"Failed to find property {propertyName}.");
                }

                propertyMappings.Add(propertyMap);
            }

            return propertyMappings;
        }

        /// <summary>
        /// Loads the type mapping.
        /// </summary>
        /// <typeparam name="T">The type to create a mapping for.</typeparam>
        /// <returns>A type mapping object.</returns>
        private static TypeMap LoadTypeMapping<T>()
        {
            // get the type that we are mapping
            Type objectType = typeof(T);

            // create a new instance of the type mapping class with the default table information
            TypeMap typeMap = new TypeMap()
            {
                Schema = string.Empty,
                TableName = objectType.Name,
                Type = objectType,
            };

            // override schema and table name if table attribute present
            if (objectType.GetCustomAttribute(typeof(TableAttribute)) is TableAttribute tableAttribute)
            {
                typeMap.Schema = tableAttribute.Schema;
                typeMap.TableName = tableAttribute.Name;
            }

            // load the property mappings
            // null property mappings have the NotMapped attribute and so should just be ignored
            typeMap.AllProperties = objectType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                                              .Select(pi => PropertyMap.LoadPropertyMap(pi))
                                              .Where(x => x != null)
                                              .ToDictionary(kvp => kvp.Property, kvp => kvp);

            // setup our helper collections of property maps
            typeMap.SelectProperties = typeMap.AllProperties.Values.ToList();

            // all key properties
            typeMap.AllKeys = typeMap.AllProperties.Values.Where(x => x.IsKey).ToList();

            // check whether we have more than one identity key
            if (typeMap.AllKeys.Count(x => x.KeyType == KeyType.Identity) > 1)
            {
                throw new ArgumentException("Type can only define a single Identity key property.");
            }

            // if we have an identity key then we should have no other keys
            if (typeMap.HasIdentityKey && typeMap.AllKeys.Count() > 1)
            {
                throw new ArgumentException("Type can only define a single key when using an Identity key property.");
            }

            // check whether we have more than one sequential key
            if (typeMap.AllKeys.Count(x => x.KeyType == KeyType.Sequential) > 1)
            {
                throw new ArgumentException("Type can only define a single Sequential key property.");
            }

            typeMap.SequentialKey = typeMap.AllKeys.SingleOrDefault(x => x.KeyType == KeyType.Sequential);

            // check whether we have a soft delete column
            if (typeMap.AllProperties.Values.Where(x => x.IsSoftDelete).Count() > 1)
            {
                throw new ArgumentException("Type can only define a single soft delete column.");
            }

            // set the soft delete property
            typeMap.SoftDeleteProperty = typeMap.AllProperties.Values.SingleOrDefault(x => x.IsSoftDelete);

            // set the required properties
            typeMap.RequiredProperties = typeMap.AllProperties.Values.Where(x => x.IsRequired).ToList();

            // set the insertable properties
            typeMap.InsertableProperties = typeMap.AllProperties.Values.Where(x => x.KeyType != KeyType.Identity)
                                                                       .ToList();

            // set the updateable properties
            typeMap.UpdateableProperties = typeMap.AllProperties.Values.Where(x => x.KeyType == KeyType.NotAKey &&
                                                                                   x.IsEditable &&
                                                                                   !x.IsSoftDelete &&
                                                                                   !x.IsDateStamp)
                                                                       .ToList();

            // set the date stamp properties
            typeMap.DateStampProperties = typeMap.AllProperties.Values.Where(x => x.IsDateStamp).ToList();

            // set the default sort order
            typeMap.DefaultSortOrder = typeMap.AllKeys.Select(x => new { Key = x.Column, Value = SortOrder.Ascending })
                                                      .ToDictionary(x => x.Key, x => x.Value);

            return typeMap;
        }
    }
}
