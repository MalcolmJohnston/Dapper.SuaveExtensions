using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Dapper.SuaveExtensions.DataContext
{
    /// <summary>
    /// Class containing extension methods that provide LINQ behaviour with dynamic fields.
    /// </summary>
    public static class DynamicLINQ
    {
        /// <summary>
        /// Sorts the elements of a sequence in ascending order according to a key.
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <param name="source">A sequence of values to order.</param>
        /// <param name="orderByProperty">The name of the key property.</param>
        /// <returns>An <see cref="IOrderedQueryable{T}" /> whose elements are sorted according to a key.</returns>
        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string orderByProperty)
        {
            var type = typeof(T);
            var property = type.GetProperty(orderByProperty);
            var parameter = Expression.Parameter(type, "p");
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var orderByExpression = Expression.Lambda(propertyAccess, parameter);
            var resultExpression = Expression.Call(
                typeof(Queryable),
                "OrderBy",
                new Type[] { type, property.PropertyType },
                source.Expression,
                Expression.Quote(orderByExpression));

            return (IOrderedQueryable<T>)source.Provider.CreateQuery<T>(resultExpression);
        }

        /// <summary>
        /// Sorts the elements of a sequence in descending order according to a key.
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <param name="source">A sequence of values to order.</param>
        /// <param name="orderByProperty">The name of the key property.</param>
        /// <returns>An <see cref="IOrderedQueryable{T}" /> whose elements are sorted according to a key.</returns>
        public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, string orderByProperty)
        {
            var type = typeof(T);
            var property = type.GetProperty(orderByProperty);
            var parameter = Expression.Parameter(type, "p");
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var orderByExpression = Expression.Lambda(propertyAccess, parameter);
            var resultExpression = Expression.Call(
                typeof(Queryable),
                "OrderByDescending",
                new Type[] { type, property.PropertyType },
                source.Expression,
                Expression.Quote(orderByExpression));

            return (IOrderedQueryable<T>)source.Provider.CreateQuery<T>(resultExpression);
        }
    }
}
