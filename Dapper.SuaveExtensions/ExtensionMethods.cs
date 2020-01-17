// <copyright file="SuaveExtensions.cs" company="InsideTravel Technology Ltd">
// Copyright (c) InsideTravel Technology Ltd. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using System.Threading.Tasks;

using Dapper.SuaveExtensions.Map;
using Dapper.SuaveExtensions.SqlBuilder;

namespace Dapper
{
    /// <summary>
    /// Static class providing Suave CRUD extension methods.
    /// </summary>
    public static class ExtensionMethods
    {
        private static readonly ISqlBuilder SqlBuilder = new TSqlBuilder();

        /// <summary>
        /// Inserts the specified entity.
        /// </summary>
        /// <typeparam name="T">The type to insert.</typeparam>
        /// <param name="conn">The connection.</param>
        /// <param name="entity">The entity.</param>
        /// <returns>The inserted entity.</returns>
        public static async Task<T> Create<T>(this IDbConnection conn, T entity)
        {
            TypeMap type = TypeMap.GetTypeMap<T>();

            // if we have a sequential key then set the value
            if (type.HasSequentialKey)
            {
                // read the next id from the database
                object id = await conn.ExecuteScalarAsync(SqlBuilder.BuildGetNextId(type), entity)
                    .ConfigureAwait(false);

                // set the sequential key on our entity
                type.SequentialKey.PropertyInfo.SetValue(
                    entity,
                    Convert.ChangeType(id, type.SequentialKey.PropertyInfo.PropertyType));
            }

            // now set any date stamp properties
            if (type.DateStampProperties.Any())
            {
                DateTime dateStamp = DateTime.Now;
                foreach (PropertyMap dateStampProperty in type.DateStampProperties)
                {
                    dateStampProperty.PropertyInfo.SetValue(entity, dateStamp);
                }
            }

            // and any soft delete properties
            if (type.HasSoftDelete)
            {
                type.SoftDeleteProperty.PropertyInfo.SetValue(
                    entity,
                    type.SoftDeleteProperty.InsertedValue);
            }

            // TODO: validate

            // execute the insert
            var row = (await conn.QueryAsync(SqlBuilder.BuildInsert(type), entity)
                .ConfigureAwait(false))
                .SingleOrDefault();

            // apply OUTPUT values to identity column
            if (type.HasIdentityKey)
            {
                if (row == null)
                {
                    throw new DataException("Expected row with Identity values, but no row returned.");
                }

                // convert to dictionary to iterate through results
                IDictionary<string, object> rowDictionary = (IDictionary<string, object>)row;

                // set the key value on the entity
                type.IdentityKey.PropertyInfo.SetValue(entity, rowDictionary[type.IdentityKey.Property]);
            }

            return entity;
        }

        /// <summary>
        /// Gets the instance of T from the database which has the specified identifier.
        /// </summary>
        /// <typeparam name="T">The type to retrieve from the database.</typeparam>
        /// <param name="connection">The connection.</param>
        /// <param name="id">The identifier.</param>
        /// <returns>An instance of <typeparamref name="T"/> or null if no object has this id.</returns>
        public static async Task<T> Read<T>(this IDbConnection connection, object id)
        {
            TypeMap type = TypeMap.GetTypeMap<T>();

            // validate that all key properties are passed
            id = type.ValidateKeyProperties(id);

            return (await connection.QueryAsync<T>(SqlBuilder.BuildSelectById(type, id), id)
                .ConfigureAwait(false))
                .SingleOrDefault();
        }

        /// <summary>
        /// Gets all instances of T from the database.
        /// </summary>
        /// <typeparam name="T">The type to retrieve from the database.</typeparam>
        /// <param name="connection">The connection.</param>
        /// <returns>Enumerable collection of <typeparamref name="T"/>.</returns>
        public static async Task<IEnumerable<T>> ReadAll<T>(this IDbConnection connection)
        {
            TypeMap type = TypeMap.GetTypeMap<T>();

            return await connection.QueryAsync<T>(SqlBuilder.BuildSelectAll(type))
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Gets a list T from the database where T matches the properties of the whereConditions object.
        /// </summary>
        /// <typeparam name="T">The type to retrieve from the database.</typeparam>
        /// <param name="connection">The connection.</param>
        /// <param name="whereConditions">The where conditions.</param>
        /// <returns>Enumerable collection of <typeparamref name="T"/>.</returns>
        public static async Task<IEnumerable<T>> ReadList<T>(this IDbConnection connection, object whereConditions)
        {
            TypeMap type = TypeMap.GetTypeMap<T>();

            // validate all properties passed
            type.ValidateWhereProperties(type.CoalesceToDictionary(whereConditions));

            return await connection.QueryAsync<T>(
                SqlBuilder.BuildSelectWhere(type, whereConditions),
                whereConditions).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets a list T from the database where T matches the properties of the whereConditions object.
        /// </summary>
        /// <typeparam name="T">The type to retrieve from the database.</typeparam>
        /// <param name="connection">The connection.</param>
        /// <param name="whereConditions">The where conditions.</param>
        /// <param name="sortOrders">The sort orders.</param>
        /// <param name="pageSize">The number of objects to return.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <returns>Enumerable collection of <typeparamref name="T"/>.</returns>
        public static async Task<PagedList<T>> ReadList<T>(
            this IDbConnection connection,
            object whereConditions,
            object sortOrders,
            int pageSize,
            int pageNumber)
        {
            TypeMap type = TypeMap.GetTypeMap<T>();

            // create the paging variables
            int firstRow = ((pageNumber - 1) * pageSize) + 1;
            int lastRow = firstRow + (pageSize - 1);

            // read the count
            int total = await connection.ExecuteScalarAsync<int>(SqlBuilder.BuildSelectCount(type, whereConditions), whereConditions);

            // read the rows
            IEnumerable<T> results = await connection.QueryAsync<T>(
                SqlBuilder.BuildSelectWhere(type, whereConditions, sortOrders, firstRow, lastRow),
                whereConditions);

            return new PagedList<T>()
            {
                Rows = results,
                HasNext = lastRow < total,
                HasPrevious = firstRow > 1,
                TotalPages = (total / pageSize) + ((total % pageSize) > 0 ? 1 : 0),
                TotalRows = total
            };
        }

        /// <summary>
        /// Updates the specified properties of an entity.
        /// </summary>
        /// <typeparam name="T">The type to update.</typeparam>
        /// <param name="conn">The connection.</param>
        /// <param name="properties">The properties to update (must include all key properties).</param>
        /// <returns>The updated entity.</returns>
        public static async Task<T> Update<T>(this IDbConnection conn, object properties)
        {
            TypeMap type = TypeMap.GetTypeMap<T>();

            // coalesce key property
            IDictionary<string, object> id = type.CoalesceKeyToDictionary(properties);

            // execute the insert
            await conn.ExecuteAsync(SqlBuilder.BuildUpdate(type, properties), properties)
                .ConfigureAwait(false);

            // return
            return (await conn.QueryAsync<T>(SqlBuilder.BuildSelectById(type, id), id)
                .ConfigureAwait(false))
                .SingleOrDefault();
        }

        /// <summary>
        /// Deletes the specified entity type by identifier.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="connection">The connection.</param>
        /// <param name="id">The identifier.</param>
        /// <returns><see cref="Task"/> representing the deletion operation.</returns>
        public static async Task Delete<T>(this IDbConnection connection, object id)
        {
            TypeMap type = TypeMap.GetTypeMap<T>();

            // validate the key properties
            id = type.ValidateKeyProperties(id);

            // delete
            await connection.QueryAsync<T>(SqlBuilder.BuildDeleteById(type), id)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes a list of the specified entity type by providing where conditions.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="connection">The connection.</param>
        /// <param name="whereConditions">The where conditions.</param>
        /// <returns><see cref="Task"/> representing the deletion operation.</returns>
        public static async Task DeleteList<T>(this IDbConnection connection, object whereConditions)
        {
            TypeMap type = TypeMap.GetTypeMap<T>();

            // validate the key properties
            type.ValidateWhereProperties(whereConditions);

            // delete
            await connection.QueryAsync<T>(
                SqlBuilder.BuildDeleteWhere(type, whereConditions),
                whereConditions).ConfigureAwait(false);
        }
    }
}
