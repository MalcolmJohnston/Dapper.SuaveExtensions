// <copyright file="ISqlBuilder.cs" company="InsideTravel Technology Ltd">
// Copyright (c) InsideTravel Technology Ltd. All rights reserved.
// </copyright>

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

        string BuildSelectAll(TypeMap type);

        string BuildSelectById(TypeMap type, object id);

        string BuildSelectWhere(TypeMap type, object whereConditions);

        string BuildInsert(TypeMap type);

        string BuildUpdate(TypeMap type, object updateProperties);

        string BuildDeleteById(TypeMap type);

        string BuildDeleteWhere(TypeMap type, object whereConditions);

        string BuildGetNextId(TypeMap type);
    }
}
