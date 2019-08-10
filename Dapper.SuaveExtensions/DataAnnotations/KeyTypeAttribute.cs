using System;

namespace Dapper.SuaveExtensions.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class KeyTypeAttribute : Attribute
    {
        public KeyTypeAttribute(KeyType keyType)
        {
            this.KeyType = keyType;
        }

        public KeyType KeyType { get; private set; } 
    }
}
