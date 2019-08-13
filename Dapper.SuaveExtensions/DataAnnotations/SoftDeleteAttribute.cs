using System;

namespace Dapper.SuaveExtensions.DataAnnotations
{
    /// <summary>
    /// The Soft Delete attribute is used to denote a property on a POCO that is mapped to a column which indicates deletion of
    /// a row in the database.
    /// For example;
    ///     If the Property 'RecordStatus' has the Soft Delete attribute with an inserted value of 1 and a deleted value of 0
    ///     Then the column mapped to RecordStatus would be set to 1 on insert
    ///     and updated to 0 on deletion, rather than having the row physically deleted.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class SoftDeleteAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SoftDeleteAttribute"/> class.
        /// </summary>
        /// <param name="valueOnInsert">The value on insert.</param>
        /// <param name="valueOnDelete">The value on delete.</param>
        public SoftDeleteAttribute(object valueOnInsert, object valueOnDelete)
        {
            this.ValueOnInsert = valueOnInsert;
            this.ValueOnDelete = valueOnDelete;
        }

        /// <summary>
        /// Gets the value on insert.
        /// </summary>
        /// <value>
        /// The value on insert.
        /// </value>
        public object ValueOnInsert { get; private set; }

        /// <summary>
        /// Gets the value on delete.
        /// </summary>
        /// <value>
        /// The value on delete.
        /// </value>
        public object ValueOnDelete { get; private set; }
    }
}
