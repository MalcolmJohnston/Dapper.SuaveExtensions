using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.SuaveExtensions.DataContext
{
    /// <summary>
    /// Sql Server data context implementation.  Intended to be used with dependency injection and is preferred to using the
    /// extension methods directly.
    /// </summary>
    /// <seealso cref="Dapper.SuaveExtensions.DataContext.IDataContext" />
    public class SqlServerDataContext : IDataContext
    {
        private readonly string connectionString = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServerDataContext"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public SqlServerDataContext(string connectionString)
        {
            this.connectionString = connectionString;
        }

        /// <inheritdoc />
        public async Task<T> Create<T>(T entity)
        {
            using (SqlConnection conn = this.OpenConnection())
            {
                return await conn.Create<T>(entity);
            }
        }

        /// <inheritdoc />
        public async Task<T> Read<T>(object id)
        {
            using (SqlConnection conn = this.OpenConnection())
            {
                return await conn.Read<T>(id);
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<T>> ReadAll<T>()
        {
            using (SqlConnection conn = this.OpenConnection())
            {
                return await conn.ReadAll<T>();
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<T>> ReadList<T>(object whereConditions)
        {
            using (SqlConnection conn = this.OpenConnection())
            {
                return await conn.ReadList<T>(whereConditions);
            }
        }

        /// <inheritdoc />
        public async Task<T> Update<T>(object properties)
        {
            using (SqlConnection conn = this.OpenConnection())
            {
                return await conn.Update<T>(properties);
            }
        }

        /// <inheritdoc />
        public async Task Delete<T>(object id)
        {
            using (SqlConnection conn = this.OpenConnection())
            {
                await conn.Delete<T>(id);
            }
        }

        /// <inheritdoc />
        public async Task DeleteList<T>(object whereConditions)
        {
            using (SqlConnection conn = this.OpenConnection())
            {
                await conn.DeleteList<T>(whereConditions);
            }
        }

        private SqlConnection OpenConnection()
        {
            SqlConnection conn = new SqlConnection(this.connectionString);
            conn.Open();

            return conn;
        }
    }
}
