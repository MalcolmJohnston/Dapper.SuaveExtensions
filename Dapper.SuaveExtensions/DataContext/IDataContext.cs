using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dapper.SuaveExtensions.DataContext
{
    /// <summary>
    /// Data Context Interface.
    /// Intended to be used with Dependency Injection libraries and to simplify testing.
    /// </summary>
    public interface IDataContext
    {
        /// <summary>
        /// Creates the specified entity.
        /// </summary>
        /// <typeparam name="T">Type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <returns>The created entity.</returns>
        Task<T> Create<T>(T entity);

        /// <summary>
        /// Reads the entity for the given type and identifier.
        /// </summary>
        /// <typeparam name="T">Type of the entity.</typeparam>
        /// <param name="id">The identifier.</param>
        /// <returns>The entity.</returns>
        Task<T> Read<T>(object id);

        /// <summary>
        /// Reads all entities of the specified type.
        /// </summary>
        /// <typeparam name="T">Type of the entity.</typeparam>
        /// <returns>All entities of this type.</returns>
        Task<IEnumerable<T>> ReadAll<T>();

        /// <summary>
        /// Reads all entities with properties that match the where condition.
        /// </summary>
        /// <typeparam name="T">Type of the entity.</typeparam>
        /// <param name="whereConditions">The properties to filter on.</param>
        /// <returns>All entities matching the where condition.</returns>
        Task<IEnumerable<T>> ReadList<T>(object whereConditions);

        /// <summary>
        /// Reads the entities in pages with optional where conditions and sort orders.
        /// </summary>
        /// <typeparam name="T">Type of the entity.</typeparam>
        /// <param name="whereConditions">The properties to filter on.</param>
        /// <param name="sortOrders">The properties to sort on and associated sort order.</param>
        /// <param name="pageSize">The number of items on each page.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <returns>The paged list representing the result of the query.</returns>
        Task<PagedList<T>> ReadList<T>(object whereConditions, object sortOrders, int pageSize, int pageNumber);

        /// <summary>
        /// Updates the specified properties on the entity.
        /// Key properties must be included in the properties object.
        /// </summary>
        /// <typeparam name="T">Type of the entity.</typeparam>
        /// <param name="properties">The properties.</param>
        /// <returns>The updated entity.</returns>
        Task<T> Update<T>(object properties);

        /// <summary>
        /// Deletes an entity by identifier.
        /// </summary>
        /// <typeparam name="T">Type of the entity.</typeparam>
        /// <param name="id">The identifier.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous delete operation.</returns>
        Task Delete<T>(object id);

        /// <summary>
        /// Deletes a list of entities that match the where condition.
        /// </summary>
        /// <typeparam name="T">Type of the entity.</typeparam>
        /// <param name="whereConditions">The where conditions.</param>
        /// <returns>The <see cref="Task"/> representing the asynchronous delete operation.</returns>
        Task DeleteList<T>(object whereConditions);
    }
}
