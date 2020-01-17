using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using Dapper.SuaveExtensions.DataAnnotations;
using Dapper.SuaveExtensions.Map;

namespace Dapper.SuaveExtensions.SqlBuilder
{
    /// <summary>
    /// TSqlDialect implementation for SQL Server.
    /// </summary>
    public class TSqlBuilder : ISqlBuilder
    {
        private const string OrderByAsc = "ASC";
        private const string OrderByDesc = "DESC";

        private readonly ConcurrentDictionary<string, string> staticSqlStatementCache = new ConcurrentDictionary<string, string>();

        /// <inheritdoc />
        public string EncapsulationFormat
        {
            get { return "[{0}]"; }
        }

        /// <inheritdoc />
        public string OrderByAscending
        {
            get { return OrderByAsc; }
        }

        /// <inheritdoc />
        public string OrderByDescending
        {
            get { return OrderByDesc; }
        }

        /// <inheritdoc />
        public string BuildSelectCount(TypeMap type, object whereConditions)
        {
            if (type == null)
            {
                throw new ArgumentException("Please provide a non-null TypeMap.");
            }

            string cacheKey = $"{type.Type.FullName}_Select_Count";
            if (!this.staticSqlStatementCache.ContainsKey(cacheKey))
            {
                this.staticSqlStatementCache[cacheKey] = $"SELECT COUNT(*) FROM {type.TableIdentifier}";
            }

            IDictionary<string, object> whereConditionsDict = type.CoalesceToDictionary(whereConditions);
            if (whereConditionsDict.Any())
            {
                return $"{this.staticSqlStatementCache[cacheKey]} {BuildWhere(type, whereConditions)}";
            }

            return this.staticSqlStatementCache[cacheKey];
        }

        /// <inheritdoc />
        public string BuildSelectAll(TypeMap type)
        {
            if (type == null)
            {
                throw new ArgumentException("Please provide a non-null TypeMap.");
            }

            string cacheKey = $"{type.Type.FullName}_Select";
            if (!this.staticSqlStatementCache.ContainsKey(cacheKey))
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("SELECT ");
                sb.Append(string.Join(", ", type.SelectProperties.Select(x => x.ColumnSelect)));
                sb.Append(" FROM ");
                sb.Append(type.TableIdentifier);

                this.staticSqlStatementCache[cacheKey] = sb.ToString();
            }

            return this.staticSqlStatementCache[cacheKey];
        }

        /// <inheritdoc />
        public string BuildSelectById(TypeMap type, object keyProperties)
        {
            if (type == null)
            {
                throw new ArgumentException("Please provide a non-null TypeMap.");
            }

            string cacheKey = $"{type.Type.FullName}_SelectById";
            if (!this.staticSqlStatementCache.ContainsKey(cacheKey))
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(this.BuildSelectAll(type));
                sb.Append(" ");
                sb.Append(this.BuildWhereIdEquals(type));

                this.staticSqlStatementCache[cacheKey] = sb.ToString();
            }

            return this.staticSqlStatementCache[cacheKey];
        }

        /// <inheritdoc />
        public string BuildSelectWhere(TypeMap type, object whereConditions)
        {
            if (type == null)
            {
                throw new ArgumentException("Please provide a non-null TypeMap.");
            }

            // validate where condition properties
            type.ValidateWhereProperties(whereConditions);

            StringBuilder sb = new StringBuilder(this.BuildSelectAll(type));
            sb.Append(" ");
            sb.Append(BuildWhere(type, whereConditions));

            return sb.ToString();
        }

        /// <inheritdoc />
        public string BuildSelectWhere(TypeMap type, object whereConditions, object sortOrders, int firstRow, int lastRow)
        {
            if (type == null)
            {
                throw new ArgumentException("Please provide a non-null TypeMap.");
            }

            // build the WHERE clause (if any specified)
            IDictionary<string, object> whereConditionsDict = type.CoalesceToDictionary(whereConditions);
            string where = whereConditionsDict.Any() ? BuildWhere(type, whereConditions) : string.Empty;

            // build the ORDER BY clause (always required and will default to primary key columns ascending, if none specified)
            string orderBy = this.BuildOrderBy(type, sortOrders);

            // build paging sql
            return $@"SELECT {string.Join(", ", type.SelectProperties.Select(x => x.ColumnSelect))}
                        FROM (SELECT ROW_NUMBER() OVER ({orderBy}) AS RowNo, *
                                FROM {type.TableIdentifier}
                                {where}) tmp
                        WHERE tmp.RowNo BETWEEN {firstRow} AND {lastRow};";
        }

        /// <inheritdoc />
        public string BuildDeleteById(TypeMap type)
        {
            if (type == null)
            {
                throw new ArgumentException("Please provide a non-null TypeMap.");
            }

            string cacheKey = $"{type.Type.FullName}_DeleteById";

            // create cached delete by id statement if required
            if (!this.staticSqlStatementCache.ContainsKey(cacheKey))
            {
                StringBuilder sb = new StringBuilder(this.BuildDelete(type));
                sb.Append($" {this.BuildWhereIdEquals(type)}");

                this.staticSqlStatementCache[cacheKey] = sb.ToString();
            }

            return this.staticSqlStatementCache[cacheKey];
        }

        /// <inheritdoc />
        public string BuildDeleteWhere(TypeMap type, object whereConditions)
        {
            if (type == null)
            {
                throw new ArgumentException("Please provide a non-null TypeMap.");
            }

            StringBuilder sb = new StringBuilder(this.BuildDelete(type));
            sb.Append($" {BuildWhere(type, whereConditions)}");

            return sb.ToString();
        }

        /// <inheritdoc />
        public string BuildInsert(TypeMap type)
        {
            if (type == null)
            {
                throw new ArgumentException("Please provide a non-null TypeMap.");
            }

            string cacheKey = $"{type.Type.FullName}_Insert";

            if (!this.staticSqlStatementCache.ContainsKey(cacheKey))
            {
                // setup the insert with table name
                StringBuilder sb = new StringBuilder($"INSERT INTO {type.TableIdentifier} (");

                // add the columns we are inserting
                sb.Append(string.Join(", ", type.InsertableProperties.Select(x => x.Column)));
                sb.Append(")");

                // add identity column outputs
                if (type.HasIdentityKey)
                {
                    sb.Append($" OUTPUT inserted.{type.IdentityKey.ColumnSelect}");
                }

                // add parameterised values
                sb.Append(" VALUES (");
                sb.Append(string.Join(", ", type.InsertableProperties.Select(x => $"@{x.Property}")));
                sb.Append(")");

                this.staticSqlStatementCache[cacheKey] = sb.ToString();
            }

            return this.staticSqlStatementCache[cacheKey];
        }

        /// <inheritdoc />
        public string BuildUpdate(TypeMap type, object properties)
        {
            if (type == null)
            {
                throw new ArgumentException("Please provide a non-null TypeMap.");
            }

            if (properties == null)
            {
                throw new ArgumentException("Please provide one or more properties to update.");
            }

            string cacheKey = $"{type.Type.FullName}_Update";

            // build list of columns to update
            List<PropertyMap> updateMaps = new List<PropertyMap>();
            PropertyInfo[] propertyInfos = properties.GetType().GetProperties();

            // loop through our updateable properties and add them as required
            foreach (PropertyInfo pi in propertyInfos)
            {
                PropertyMap propertyMap = type.UpdateableProperties
                    .Where(x => x.Property == pi.Name).SingleOrDefault();

                if (propertyMap != null)
                {
                    updateMaps.Add(propertyMap);
                }
            }

            // check we have properties to update
            if (updateMaps.Count == 0)
            {
                throw new ArgumentException("Please provide one or more updateable properties.");
            }

            // build the update sql
            StringBuilder sb = new StringBuilder($"UPDATE {type.TableIdentifier} SET ");

            // add all update properties to SET clause
            for (int i = 0; i < updateMaps.Count; i++)
            {
                sb.Append($"{updateMaps[i].Column} = @{updateMaps[i].Property}, ");
            }

            // deal with date stamp properties
            int dateStampCount = type.DateStampProperties.Where(x => !x.IsReadOnly).Count();
            if (dateStampCount > 0)
            {
                // add any Date Stamp properties to the SET clause
                foreach (PropertyMap pm in type.DateStampProperties.Where(x => !x.IsReadOnly))
                {
                    sb.Append($"{pm.Column} = GETDATE(), ");
                }
            }

            // remove trailing separator (either from update maps or date stamps)
            sb.Remove(sb.Length - 2, 2);

            // add where clause
            sb.Append($" {this.BuildWhereIdEquals(type)}");

            return sb.ToString();
        }

        /// <inheritdoc />
        public string BuildGetNextId(TypeMap type)
        {
            if (type == null)
            {
                throw new ArgumentException("Please provide a non-null TypeMap.");
            }

            if (!type.HasSequentialKey)
            {
                throw new ArgumentException("Cannot generate get next id sql for type that does not have a sequential key.");
            }

            string cacheKey = $"{type.Type.FullName}_GetNextId";

            if (!this.staticSqlStatementCache.ContainsKey(cacheKey))
            {
                StringBuilder sb = new StringBuilder("SELECT ISNULL(MAX(");
                sb.Append(type.SequentialKey.Column);
                sb.Append($"), 0) + 1 FROM {type.TableIdentifier} ");

                if (type.HasManualKeys)
                {
                    sb.Append(BuildWhere(type.AssignedKeys.ToList()));
                }

                this.staticSqlStatementCache[cacheKey] = sb.ToString();
            }

            return this.staticSqlStatementCache[cacheKey];
        }

        /// <summary>
        /// Builds the where clause from an anonymous object.
        /// </summary>
        /// <param name="type">The type map.</param>
        /// <param name="whereConditions">An anonymous object containing the where conditions.</param>
        /// <returns>The WHERE clause string.</returns>
        /// <exception cref="ArgumentException">Argument exception throw if property in where conditions doesn't exist.</exception>
        private static string BuildWhere(TypeMap type, object whereConditions)
        {
            if (whereConditions == null)
            {
                throw new ArgumentException("Please specify some conditions to search by.");
            }

            // build a list of properties for the WHERE clause
            // this will error if no properties are found that match our Type
            IList<PropertyMap> properties = type.ValidateWhereProperties(whereConditions);

            // return the WHERE clause
            return BuildWhere(properties);
        }

        private static string BuildWhere(IList<PropertyMap> properties)
        {
            if (properties == null || properties.Count == 0)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder("WHERE ");
            for (int i = 0; i < properties.Count; i++)
            {
                sb.Append(properties[i].Column);
                sb.Append(" = @");
                sb.Append(properties[i].Property);

                // exclude AND on last property
                if ((i + 1) < properties.Count)
                {
                    sb.Append(" AND ");
                }
            }

            return sb.ToString();
        }

        private string BuildWhereIdEquals(TypeMap type)
        {
            string cacheKey = $"{type.Type.FullName}_WhereIdEquals";

            if (!this.staticSqlStatementCache.ContainsKey(cacheKey))
            {
                this.staticSqlStatementCache[cacheKey] = BuildWhere(type.AllKeys);
            }

            return this.staticSqlStatementCache[cacheKey];
        }

        private string BuildDelete(TypeMap type)
        {
            string cacheKey = $"{type.Type.FullName}_Delete";

            // create cached delete statement if required
            if (!this.staticSqlStatementCache.ContainsKey(cacheKey))
            {
                this.staticSqlStatementCache[cacheKey] = $"DELETE FROM {type.TableIdentifier}";
            }

            return this.staticSqlStatementCache[cacheKey];
        }

        private string BuildOrderBy(TypeMap type, object sortOrders)
        {
            // coalesce the dictionary
            IDictionary<string, SortOrder> sortOrderDict = type.CoalesceSortOrderDictionary(sortOrders);

            // validate / return
            StringBuilder orderBySb = new StringBuilder("ORDER BY ");
            for (int i = 0; i < sortOrderDict.Count; i++)
            {
                string propertyName = sortOrderDict.Keys.ElementAt(i);
                SortOrder order = sortOrderDict[propertyName];

                if (type.AllProperties.TryGetValue(propertyName, out PropertyMap pm) == false)
                {
                    throw new ArgumentException($"Failed to find property {propertyName} on {type.Type.Name}");
                }

                orderBySb.Append($"{pm.Column} {this.GetSortOrder(order)}");

                if (i != sortOrderDict.Count - 1)
                {
                    orderBySb.Append(", ");
                }
            }

            return orderBySb.ToString();
        }
    }
}
