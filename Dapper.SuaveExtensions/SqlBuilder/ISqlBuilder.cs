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
        /// Gets the string representation of a ascending qualifier on an ORDER BY clause.
        /// </summary>
        string OrderByAscending { get; }

        /// <summary>
        /// Gets the string representation of a descending qualifier on an ORDER BY clause.
        /// </summary>
        string OrderByDescending { get; }

        /// <summary>
        /// Build a select statement that counts the returned rows when the where conditions are applied.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="whereConditions">The where conditions.</param>
        /// <returns>The select statement that counts the number of rows.</returns>
        string BuildSelectCount(TypeMap type, object whereConditions);

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
        /// Builds a select statement with a dynamic where clause, dynamic sort order collection and paging arguments.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="whereConditions">The where conditions.</param>
        /// <param name="sortOrders">The sort orders (may be null).</param>
        /// <param name="firstRow">The first row to return (e.g. 1).</param>
        /// <param name="lastRow">The last row to return (e.g. 10).</param>
        /// <returns>The select where statement for the given page and number of objects.</returns>
        string BuildSelectWhere(TypeMap type, object whereConditions, object sortOrders, int firstRow, int lastRow);

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
