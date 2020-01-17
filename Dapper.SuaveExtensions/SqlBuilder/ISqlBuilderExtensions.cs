namespace Dapper.SuaveExtensions.SqlBuilder
{
    /// <summary>
    /// Extension methods for the ISqlBuilder interface.
    /// </summary>
    public static class ISqlBuilderExtensions
    {
        /// <summary>
        /// Gets the string representation of the Sort Order enumeration for a given Sql Builder.
        /// </summary>
        /// <param name="sqlBuilder">The Sql Builder.</param>
        /// <param name="sortOrder">The Sort Order.</param>
        /// <returns>The sort order for an ORDER BY clause for the given Sql Builder.</returns>
        public static string GetSortOrder(this ISqlBuilder sqlBuilder, SortOrder sortOrder)
        {
            return sortOrder == SortOrder.Ascending ? sqlBuilder.OrderByAscending : sqlBuilder.OrderByDescending;
        }
    }
}
