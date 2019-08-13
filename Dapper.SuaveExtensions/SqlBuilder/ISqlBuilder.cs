using Dapper.SuaveExtensions.Map;

namespace Dapper.SuaveExtensions.SqlBuilder
{
    /// <summary>
    /// Sql Builder interface is intented to be implemented for multiple database
    /// engines.
    /// </summary>
    public interface ISqlBuilder
    {
        /// <summary>
        /// Gets the format string for encapsulating identifiers.
        /// For example in SQL Server identifers are wrapped in square brackets i.e. "[{0}]".
        /// </summary>
        string EncapsulationFormat { get; }

        /// <summary>
        /// Builds the select statement.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The select all statement.</returns>
        string BuildSelectAll(TypeMap type);

        /// <summary>
        /// Builds a select statement with a where identifier equals clause.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="id">The identifier.</param>
        /// <returns>The select where identifier equals statement.</returns>
        string BuildSelectById(TypeMap type, object id);

        /// <summary>
        /// Builds a select statement with a dynamic where clause.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="whereConditions">The where conditions.</param>
        /// <returns>The select where statement.</returns>
        string BuildSelectWhere(TypeMap type, object whereConditions);

        /// <summary>
        /// Builds the insert statement.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The insert statement.</returns>
        string BuildInsert(TypeMap type);

        /// <summary>
        /// Builds a dynamic update statement.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="updateProperties">The update properties.</param>
        /// <returns>The update statement.</returns>
        string BuildUpdate(TypeMap type, object updateProperties);

        /// <summary>
        /// Builds a delete where id equals statement.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The delete where id equals statement.</returns>
        string BuildDeleteById(TypeMap type);

        /// <summary>
        /// Builds a dynamic delete where statement.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="whereConditions">The where conditions.</param>
        /// <returns>The delete where statement.</returns>
        string BuildDeleteWhere(TypeMap type, object whereConditions);

        /// <summary>
        /// Builds the get next identifier.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The get next identifier select statement.</returns>
        string BuildGetNextId(TypeMap type);
    }
}
