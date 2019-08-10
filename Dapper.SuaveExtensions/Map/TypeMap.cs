// <copyright file="TypeMap.cs" company="InsideTravel Technology Ltd">
// Copyright (c) InsideTravel Technology Ltd. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;

using Dapper.SuaveExtensions.DataAnnotations;

namespace Dapper.SuaveExtensions.Map
{
    /// <summary>
    /// A class representing a type to database table mapping.
    /// </summary>
    public class TypeMap
    {
        private IEnumerable<PropertyMap> allPropertyMaps;

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
        public IEnumerable<PropertyMap> IdentityKeys
        {
            get { return this.AllKeys.Where(x => x.KeyType == KeyType.Identity); }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has identity keys.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has identity keys; otherwise, <c>false</c>.
        /// </value>
        public bool HasIdentityKeys
        {
            get { return this.IdentityKeys.Count() > 0; }
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
        /// Loads the type mapping.
        /// </summary>
        /// <typeparam name="T">The type to create a mapping for.</typeparam>
        /// <returns>A type mapping object.</returns>
        public static TypeMap LoadTypeMapping<T>()
        {
            // get the type that we are mapping
            Type objectType = typeof(T);

            // create a new instance of the type mapping class with the default table information
            TypeMap typeMap = new TypeMap()
            {
                Schema = string.Empty,
                TableName = objectType.Name,
            };

            // override schema and table name if table attribute present
            TableAttribute tableAttribute = objectType.GetCustomAttribute(typeof(TableAttribute)) as TableAttribute;
            if (tableAttribute != null)
            {
                typeMap.Schema = tableAttribute.Schema;
                typeMap.TableName = tableAttribute.Name;
            }

            // load the property mappings
            // null property mappings have the NotMapped attribute and so should just be ignored
            typeMap.allPropertyMaps = objectType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                                                     .Select(pi => PropertyMap.LoadPropertyMap(pi))
                                                     .Where(x => x != null);

            // setup our helper collections of property maps
            typeMap.SelectProperties = typeMap.allPropertyMaps.ToList();

            // all key properties
            typeMap.AllKeys = typeMap.allPropertyMaps.Where(x => x.IsKey).ToList();

            // check whether we have more than one sequential key
            if (typeMap.AllKeys.Count(x => x.KeyType == KeyType.Sequential) > 1)
            {
                throw new ArgumentException("Type can only define a single Sequential key property.");
            }

            typeMap.SequentialKey = typeMap.AllKeys.SingleOrDefault(x => x.KeyType == KeyType.Sequential);

            // check whether our sequential key is paired with one or more identity keys - this is not supported
            if (typeMap.HasSequentialKey && typeMap.HasIdentityKeys)
            {
                throw new ArgumentException("Sequential keys are not compatible with Identity keys.");
            }

            // check whether we have a soft delete column
            if (typeMap.allPropertyMaps.Where(x => x.IsSoftDelete).Count() > 1)
            {
                throw new ArgumentException("Type can only define a single soft delete column.");
            }

            // set the soft delete property
            typeMap.SoftDeleteProperty = typeMap.allPropertyMaps.SingleOrDefault(x => x.IsSoftDelete);

            // set the required properties
            typeMap.RequiredProperties = typeMap.allPropertyMaps.Where(x => x.IsRequired).ToList();

            // set the insertable properties
            typeMap.InsertableProperties = typeMap.allPropertyMaps.Where(x => x.KeyType != KeyType.Identity)
                                                                  .ToList();

            // set the updateable properties
            typeMap.UpdateableProperties = typeMap.allPropertyMaps.Where(x => x.KeyType == KeyType.NotAKey &&
                                                                              x.IsEditable &&
                                                                              !x.IsSoftDelete &&
                                                                              !x.IsDateStamp)
                                                                  .ToList();

            // set the date stamp properties
            typeMap.DateStampProperties = typeMap.allPropertyMaps.Where(x => x.IsDateStamp).ToList();

            return typeMap;
        }

        /// <summary>
        /// Validates the key properties.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <exception cref="ArgumentException">Thrown if a key property is not passed on the object.</exception>
        public void ValidateKeyProperties(object id)
        {
            if (id == null)
            {
                throw new ArgumentException("Passed identifier object is null.");
            }

            PropertyInfo[] propertyInfos = id.GetType().GetProperties();
            foreach (PropertyMap propertyMap in this.AllKeys)
            {
                PropertyInfo pi = propertyInfos.Where(x => x.Name == propertyMap.Property).SingleOrDefault();

                if (pi == null)
                {
                    throw new ArgumentException($"Failed to find key property {propertyMap.Property}.");
                }
            }
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
        public IDictionary<string, object> CoalesceKeyObject(object propertyBag)
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

        public IList<PropertyMap> ValidateWhereProperties(object obj)
        {
            // get the passed properties
            PropertyInfo[] propertyInfos = obj.GetType().GetProperties();

            // check that we have at least one property/condition
            if (propertyInfos == null || propertyInfos.Length == 0)
            {
                throw new ArgumentException("Please specify at least one property for a WHERE condition.");
            }

            // setup our list of property mappings that we will create the where clause from
            List<PropertyMap> propertyMappings = new List<PropertyMap>();

            foreach (PropertyInfo property in propertyInfos)
            {
                PropertyMap propertyMap = this.SelectProperties
                                              .SingleOrDefault(x => x.Property == property.Name);

                if (propertyMap == null)
                {
                    throw new ArgumentException($"Failed to find property {property.Name}.");
                }

                propertyMappings.Add(propertyMap);
            }

            return propertyMappings;
        }
    }
}
