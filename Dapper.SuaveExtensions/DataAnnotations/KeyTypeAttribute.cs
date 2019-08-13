using System;

namespace Dapper.SuaveExtensions.DataAnnotations
{
    /// <summary>
    /// Key Type Attribute.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class KeyTypeAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyTypeAttribute"/> class.
        /// </summary>
        /// <param name="keyType">Type of the key.</param>
        public KeyTypeAttribute(KeyType keyType)
        {
            this.KeyType = keyType;
        }

        /// <summary>
        /// Gets the type of the key.
        /// </summary>
        /// <value>
        /// The type of the key.
        /// </value>
        public KeyType KeyType { get; private set; }
    }
}
