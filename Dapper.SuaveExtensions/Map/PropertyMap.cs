// <copyright file="PropertyMap.cs" company="InsideTravel Technology Ltd">
// Copyright (c) InsideTravel Technology Ltd. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

using Dapper.SuaveExtensions.DataAnnotations;

namespace Dapper.SuaveExtensions.Map
{
    /// <summary>
    /// Class the defines a property mapping.
    /// </summary>
    public class PropertyMap
    {
        public static PropertyMap LoadPropertyMap(PropertyInfo pi)
        {
            // if the property info is null or not mapped attribute present
            // then return null
            if (pi == null || pi.GetCustomAttribute<NotMappedAttribute>() != null)
            {
                return null;
            }

            PropertyMap pm = new PropertyMap() { PropertyInfo = pi };

            // read custom attributes from property info
            KeyAttribute key = pi.GetCustomAttribute(typeof(KeyAttribute)) as KeyAttribute;
            KeyTypeAttribute keyType = pi.GetCustomAttribute(typeof(KeyTypeAttribute)) as KeyTypeAttribute;
            ColumnAttribute column = pi.GetCustomAttribute(typeof(ColumnAttribute)) as ColumnAttribute;
            ReadOnlyAttribute readOnly = pi.GetCustomAttribute(typeof(ReadOnlyAttribute)) as ReadOnlyAttribute;
            EditableAttribute editable = pi.GetCustomAttribute(typeof(EditableAttribute)) as EditableAttribute;
            RequiredAttribute required = pi.GetCustomAttribute(typeof(RequiredAttribute)) as RequiredAttribute;
            DateStampAttribute dateStamp = pi.GetCustomAttribute(typeof(DateStampAttribute)) as DateStampAttribute;
            SoftDeleteAttribute softDeleteAttribute = pi.GetCustomAttribute(typeof(SoftDeleteAttribute)) as SoftDeleteAttribute;

            // check whether this property is an implied key
            bool impliedKey = pi.Name == "Id" || $"{pi.DeclaringType.Name}Id" == pi.Name;
            bool isKey = impliedKey || key != null || keyType != null;

            // is this an implied or explicit key then set the key type
            if (isKey)
            {
                // if key type not provided then imply key type
                if (keyType == null)
                {
                    if (pi.PropertyType == typeof(int))
                    {
                        // if integer then treat as identity by default
                        pm.KeyType = KeyType.Identity;
                    }
                    else if (pi.PropertyType == typeof(Guid))
                    {
                        // if guid then treat as guid
                        pm.KeyType = KeyType.Guid;
                    }
                    else
                    {
                        // otherwise treat as assigned
                        pm.KeyType = KeyType.Assigned;
                    }
                }
                else
                {
                    pm.KeyType = keyType.KeyType;
                }
            }

            // set remaining properties
            pm.Column = column != null ? column.Name : pi.Name;
            pm.IsRequired = required != null ? true : false;
            pm.IsDateStamp = dateStamp != null ? true : false;
            pm.InsertedValue = softDeleteAttribute?.ValueOnInsert;
            pm.DeleteValue = softDeleteAttribute?.ValueOnDelete;
            pm.IsSoftDelete = softDeleteAttribute != null;

            // set read-only / editable
            // if property is a date-stamp, identity, guid or sequential key then cannot be
            // editable and must be read-only
            if (pm.KeyType == KeyType.Identity || pm.KeyType == KeyType.Guid || 
                pm.KeyType == KeyType.Sequential || pm.IsDateStamp)
            {
                pm.IsReadOnly = true;
                pm.IsEditable = false;
            }
            else
            {
                // otherwise obey the attributes
                pm.IsReadOnly = readOnly != null ? readOnly.IsReadOnly : false;

                if (pm.IsReadOnly && editable == null)
                {
                    pm.IsEditable = false;
                }
                else
                {
                    pm.IsEditable = editable != null ? editable.AllowEdit : true;
                }
            }

            // if editable and readonly conflict throw an error
            if (pm.IsEditable && pm.IsReadOnly)
            {
                throw new ArgumentException("Readonly and Editable attributes specified with opposing values");
            }

            return pm;
        }

        /// <summary>
        /// Gets the property information.
        /// </summary>
        /// <value>
        /// The property information.
        /// </value>
        public PropertyInfo PropertyInfo { get; private set; }

        /// <summary>
        /// Gets the property.
        /// </summary>
        /// <value>
        /// The property.
        /// </value>
        public string Property
        {
            get { return this.PropertyInfo.Name; }
        }

        /// <summary>
        /// Gets the column the property is mapped to.
        /// </summary>
        /// <value>
        /// The column.
        /// </value>
        public string Column { get; private set; }

        /// <summary>
        /// Gets or the column select.
        /// </summary>
        /// <value>
        /// The column select.
        /// </value>
        public string ColumnSelect
        {
            get
            {
                return this.Property == this.Column ? this.Column : $"{this.Column} AS {this.Property}";
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is key.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is key; otherwise, <c>false</c>.
        /// </value>
        public bool IsKey
        {
            get { return this.KeyType != KeyType.NotAKey; }
        }

        /// <summary>
        /// Gets the type of the key.
        /// </summary>
        /// <value>
        /// The type of the key.
        /// </value>
        public KeyType KeyType { get; private set; } = KeyType.NotAKey;

        /// <summary>
        /// Gets a value indicating whether this property is editable, defaults to true.
        /// If is editable is false, then this property will be excluded from updates.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is editable; otherwise, <c>false</c>.
        /// </value>
        public bool IsEditable { get; private set; } = true;

        /// <summary>
        /// Gets a value indicating whether this property is read-only.
        /// This will exclude the property from inserts and updates.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is read-only; otherwise, <c>false</c>.
        /// </value>
        public bool IsReadOnly { get; private set; } = false;

        /// <summary>
        /// Gets a value indicating whether this instance is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is required; otherwise, <c>false</c>.
        /// </value>
        public bool IsRequired { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is date stamp.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is date stamp; otherwise, <c>false</c>.
        /// </value>
        public bool IsDateStamp { get; private set; }

        /// <summary>
        /// Gets the soft delete value on insertion.
        /// </summary>
        /// <value>
        /// The soft delete value on insertion.
        /// </value>
        public object InsertedValue { get; private set; }

        /// <summary>
        /// Gets the soft delete value on deletion.
        /// </summary>
        /// <value>
        /// The soft delete value on deletion.
        /// </value>
        public object DeleteValue { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is soft delete.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is soft delete; otherwise, <c>false</c>.
        /// </value>
        public bool IsSoftDelete { get; private set; }
    }
}
