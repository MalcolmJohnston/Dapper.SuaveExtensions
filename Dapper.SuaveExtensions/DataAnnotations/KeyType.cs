namespace Dapper.SuaveExtensions.DataAnnotations
{
    /// <summary>
    /// Enumeration of supported primary key types for SQL generation.
    /// </summary>
    public enum KeyType
    {
        /// <summary>
        /// The column is not a key
        /// </summary>
        NotAKey,

        /// <summary>
        /// The column is an identity key calculated by the database
        /// </summary>
        Identity,

        /// <summary>
        /// The key is a guid.
        /// </summary>
        Guid,

        /// <summary>
        /// The key must be assinged in code.
        /// </summary>
        Assigned,

        /// <summary>
        /// The key is sequential
        /// This works like an identity column but the value is calculated
        /// (e.g. SELECT TOP (ID) + 1 FROM Table)
        /// </summary>
        Sequential,
    }
}
