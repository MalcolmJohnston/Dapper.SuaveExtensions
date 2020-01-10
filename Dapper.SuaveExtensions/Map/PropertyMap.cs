using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

using Dapper.SuaveExtensions.DataAnnotations;

namespace Dapper.SuaveExtensions.Map
{
    /// <summary>
    /// Class the defines a property mapping.
    /// </summary>
    public class PropertyMap
    {
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

        /// <summary>
        /// Loads the property map.
        /// </summary>
        /// <param name="propertyInfo">The proerty info.</param>
        /// <returns>The property map for this property.</returns>
        /// <exception cref="ArgumentException">Readonly and Editable attributes specified with opposing values.</exception>
        public static PropertyMap LoadPropertyMap(PropertyInfo propertyInfo)
        {
            // if the property info is null or not mapped attribute present
            // then return null
            if (propertyInfo == null || propertyInfo.GetCustomAttribute<NotMappedAttribute>() != null)
            {
                return null;
            }

            PropertyMap pm = new PropertyMap() { PropertyInfo = propertyInfo };

            // read custom attributes from property info
            KeyAttribute key = propertyInfo.GetCustomAttribute(typeof(KeyAttribute)) as KeyAttribute;
            KeyTypeAttribute keyType = propertyInfo.GetCustomAttribute(typeof(KeyTypeAttribute)) as KeyTypeAttribute;
            ColumnAttribute column = propertyInfo.GetCustomAttribute(typeof(ColumnAttribute)) as ColumnAttribute;
            ReadOnlyAttribute readOnly = propertyInfo.GetCustomAttribute(typeof(ReadOnlyAttribute)) as ReadOnlyAttribute;
            EditableAttribute editable = propertyInfo.GetCustomAttribute(typeof(EditableAttribute)) as EditableAttribute;
            RequiredAttribute required = propertyInfo.GetCustomAttribute(typeof(RequiredAttribute)) as RequiredAttribute;
            DateStampAttribute dateStamp = propertyInfo.GetCustomAttribute(typeof(DateStampAttribute)) as DateStampAttribute;
            SoftDeleteAttribute softDeleteAttribute = propertyInfo.GetCustomAttribute(typeof(SoftDeleteAttribute)) as SoftDeleteAttribute;

            // check whether this property is an implied key
            bool impliedKey = propertyInfo.Name == "Id" || $"{propertyInfo.DeclaringType.Name}Id" == propertyInfo.Name;
            bool isKey = impliedKey || key != null || keyType != null;

            // is this an implied or explicit key then set the key type
            if (isKey)
            {
                // if key type not provided then imply key type
                if (keyType == null)
                {
                    if (propertyInfo.PropertyType == typeof(int))
                    {
                        // if integer then treat as identity by default
                        pm.KeyType = KeyType.Identity;
                    }
                    else if (propertyInfo.PropertyType == typeof(Guid))
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
            pm.Column = column != null ? column.Name : propertyInfo.Name;
            pm.IsRequired = required != null ? true : false;
            pm.IsDateStamp = dateStamp != null ? true : false;
            pm.InsertedValue = softDeleteAttribute?.ValueOnInsert;
            pm.DeleteValue = softDeleteAttribute?.ValueOnDelete;
            pm.IsSoftDelete = softDeleteAttribute != null;

            // set read-only / editable
            // if property is identity, guid or sequential key then cannot be
            // editable and must be read-only
            if (pm.KeyType == KeyType.Identity || pm.KeyType == KeyType.Guid || pm.KeyType == KeyType.Sequential)
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
    }
}
