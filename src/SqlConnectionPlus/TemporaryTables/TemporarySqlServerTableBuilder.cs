// Copyright (c) 2025 David Liebeherr
// Licensed under the MIT License. See LICENSE.md in the project root for more information.

using FastMember;
using LinkDotNet.StringBuilder;
using Microsoft.Data;
using RentADeveloper.SqlConnectionPlus.Entities;
using RentADeveloper.SqlConnectionPlus.Readers;
using RentADeveloper.SqlConnectionPlus.SqlServer;

namespace RentADeveloper.SqlConnectionPlus.TemporaryTables;

/// <summary>
/// Builds temporary SQL Server tables.
/// </summary>
internal static class TemporarySqlServerTableBuilder
{
    /// <summary>
    /// Builds a temporary table and populates it with the specified values.
    /// </summary>
    /// <param name="connection">The SQL connection to use to build the temporary table.</param>
    /// <param name="transaction">The SQL transaction within to build the temporary table.</param>
    /// <param name="name">The name of the temporary table to build.</param>
    /// <param name="values">The values to populate the temporary table with.</param>
    /// <param name="valuesType">The type of values in <paramref name="values" />.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>
    /// An instance of <see cref="TemporarySqlServerTableDisposer" /> that can be used to dispose the built table.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 <paramref name="connection" /> is <see langword="null" />.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <paramref name="name" /> is <see langword="null" />.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <paramref name="values" /> is <see langword="null" />.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <paramref name="valuesType" /> is <see langword="null" />.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="name" /> is empty or consists only of white-space characters.
    /// </exception>
    /// <remarks>
    /// If <paramref name="valuesType" /> is a scalar type
    /// (e.g. <see cref="String" />, <see cref="Int32" />, <see cref="DateTime" />, <see cref="Enum" /> and so on),
    /// a single-column temporary table will be built with a column named "Value" with a data type that matches the
    /// type <paramref name="valuesType" />.
    /// 
    /// If <paramref name="valuesType" /> is a complex type (e.g. a class or a record), a multi-column temporary table
    /// will be built.
    /// The temporary table will contain a column for each property (with a public getter) of the type
    /// <paramref name="valuesType" />.
    /// The name of each column will be the name of the corresponding property.
    /// The data type of each column will be the property type of the corresponding property.
    /// </remarks>
    internal static TemporarySqlServerTableDisposer BuildTemporaryTable(
        SqlConnection connection,
        SqlTransaction? transaction,
        String name,
        IEnumerable values,
        Type valuesType,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(values);
        ArgumentNullException.ThrowIfNull(valuesType);

        // For text columns, we must use the collation of the database the connection is currently connected to,
        // because the tempDB might have a different collation. Otherwise, we could run into collation conflict errors
        // when joining the temporary table with other tables.
        var databaseCollation = GetCurrentDatabaseCollation(connection, transaction);

        if (valuesType.IsBuiltInTypeOrNullableBuildInType() || valuesType.IsEnumOrNullableEnumType())
        {
            using var createCommand = Dependencies.SqlCommandFactory.CreateSqlCommand(
                connection,
                BuildCreateSingleColumnTemporaryTableSqlCode(
                    name,
                    // ReSharper disable once PossibleMultipleEnumeration
                    values,
                    valuesType,
                    databaseCollation,
                    SqlConnectionExtensions.EnumSerializationMode
                ),
                transaction
            );

            using var registerSqlCommandCancellation =
                DbCommandHelper.RegisterDbCommandCancellation(createCommand, cancellationToken);

            createCommand.ExecuteNonQuery();
        }
        else
        {
            using var createCommand = Dependencies.SqlCommandFactory.CreateSqlCommand(
                connection,
                BuildCreateMultiColumnTemporaryTableSqlCode(
                    name,
                    valuesType,
                    databaseCollation,
                    SqlConnectionExtensions.EnumSerializationMode
                ),
                transaction
            );

            using var registerSqlCommandCancellation =
                DbCommandHelper.RegisterDbCommandCancellation(createCommand, cancellationToken);

            createCommand.ExecuteNonQuery();
        }

        // ReSharper disable once PossibleMultipleEnumeration
        using var reader = CreateValuesDataReader(values, valuesType);

        using var sqlBulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.TableLock, transaction);

        sqlBulkCopy.BulkCopyTimeout = 0;
        sqlBulkCopy.BatchSize = 0;
        sqlBulkCopy.DestinationTableName = name;

        sqlBulkCopy.ColumnMappings.Clear();

        for (var fieldOrdinal = 0; fieldOrdinal < reader.FieldCount; fieldOrdinal++)
        {
            var fieldName = reader.GetName(fieldOrdinal);
            sqlBulkCopy.ColumnMappings.Add(fieldName, fieldName);
        }

        sqlBulkCopy.WriteToServer(reader);

        return new(
            () => DropTable(name, connection, transaction),
            () => DropTableAsync(name, connection, transaction)
        );
    }

    /// <summary>
    /// Asynchronously builds a temporary table and populates it with the specified values.
    /// </summary>
    /// <param name="connection">The SQL connection to use to build the temporary table.</param>
    /// <param name="transaction">The SQL transaction within to build the temporary table.</param>
    /// <param name="name">The name of the temporary table to build.</param>
    /// <param name="values">The values to populate the temporary table with.</param>
    /// <param name="valuesType">The type of values in <paramref name="values" />.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// <see cref="Task{TResult}.Result" /> will contain an instance of <see cref="TemporarySqlServerTableDisposer" />
    /// that can be used to dispose the built table.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 <paramref name="connection" /> is <see langword="null" />.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <paramref name="name" /> is <see langword="null" />.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <paramref name="values" /> is <see langword="null" />.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <paramref name="valuesType" /> is <see langword="null" />.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="name" /> is empty or consists only of white-space characters.
    /// </exception>
    /// <remarks>
    /// If <paramref name="valuesType" /> is a scalar type
    /// (e.g. <see cref="String" />, <see cref="Int32" />, <see cref="DateTime" />, <see cref="Enum" /> and so on),
    /// a single-column temporary table will be built with a column named "Value" with a data type that matches
    /// the type <paramref name="valuesType" />.
    /// 
    /// If <paramref name="valuesType" /> is a complex type (e.g. a class or a record), a multi-column temporary table
    /// will be built.
    /// The temporary table will contain a column for each property (with a public getter) of the type
    /// <paramref name="valuesType" />.
    /// The name of each column will be the name of the corresponding property.
    /// The data type of each column will be the property type of the corresponding property.
    /// </remarks>
    internal static async Task<TemporarySqlServerTableDisposer> BuildTemporaryTableAsync(
        SqlConnection connection,
        SqlTransaction? transaction,
        String name,
        IEnumerable values,
        Type valuesType,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(values);
        ArgumentNullException.ThrowIfNull(valuesType);

        // For text columns, we must use the collation of the database the connection is currently connected to,
        // because the tempDB might have a different collation. Otherwise, we could run into collation conflict errors
        // when joining the temporary table with other tables.
        var databaseCollation = await GetCurrentDatabaseCollationAsync(connection, transaction)
            .ConfigureAwait(false);

        if (valuesType.IsBuiltInTypeOrNullableBuildInType() || valuesType.IsEnumOrNullableEnumType())
        {
            using var createCommand = Dependencies.SqlCommandFactory.CreateSqlCommand(
                connection,
                BuildCreateSingleColumnTemporaryTableSqlCode(
                    name,
                    // ReSharper disable once PossibleMultipleEnumeration
                    values,
                    valuesType,
                    databaseCollation,
                    SqlConnectionExtensions.EnumSerializationMode
                ),
                transaction
            );

            using var registerSqlCommandCancellation =
                DbCommandHelper.RegisterDbCommandCancellation(createCommand, cancellationToken);

            await createCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
        else
        {
            using var createCommand = Dependencies.SqlCommandFactory.CreateSqlCommand(
                connection,
                BuildCreateMultiColumnTemporaryTableSqlCode(
                    name,
                    valuesType,
                    databaseCollation,
                    SqlConnectionExtensions.EnumSerializationMode
                ),
                transaction
            );

            using var registerSqlCommandCancellation =
                DbCommandHelper.RegisterDbCommandCancellation(createCommand, cancellationToken);

            await createCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        // ReSharper disable once PossibleMultipleEnumeration
        using var reader = CreateValuesDataReader(values, valuesType);

        using var sqlBulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.TableLock, transaction);

        sqlBulkCopy.BulkCopyTimeout = 0;
        sqlBulkCopy.BatchSize = 0;
        sqlBulkCopy.DestinationTableName = name;

        sqlBulkCopy.ColumnMappings.Clear();

        for (var fieldOrdinal = 0; fieldOrdinal < reader.FieldCount; fieldOrdinal++)
        {
            var fieldName = reader.GetName(fieldOrdinal);
            sqlBulkCopy.ColumnMappings.Add(fieldName, fieldName);
        }

        try
        {
            await sqlBulkCopy.WriteToServerAsync(reader, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationAbortedException)
        {
            throw new OperationCanceledException(cancellationToken);
        }

        return new(
            () => DropTable(name, connection, transaction),
            () => DropTableAsync(name, connection, transaction)
        );
    }


    /// <summary>
    /// Builds an SQL code to create a multi-column temporary table to be populated with objects of the type
    /// <paramref name="objectsType" />.
    /// </summary>
    /// <param name="tableName">The name of the temporary table to create.</param>
    /// <param name="objectsType">The type of objects the temporary table will be populated with.</param>
    /// <param name="collation">The collation to use for text columns.</param>
    /// <param name="enumSerializationMode">The mode to use to serialize <see cref="Enum" /> values.</param>
    /// <returns>The SQL code.</returns>
    private static String BuildCreateMultiColumnTemporaryTableSqlCode(
        String tableName,
        Type objectsType,
        String collation,
        EnumSerializationMode enumSerializationMode
    )
    {
        using var sqlBuilder = new ValueStringBuilder(stackalloc Char[500]);

        sqlBuilder.Append("CREATE TABLE [");
        sqlBuilder.Append(tableName);
        sqlBuilder.Append("] (");

        var properties = EntityHelper.GetEntityReadableProperties(objectsType);

        var prependComma = false;

        foreach (var property in properties)
        {
            if (prependComma)
            {
                sqlBuilder.Append(", ");
            }

            sqlBuilder.Append('[');
            sqlBuilder.Append(property.Name);
            sqlBuilder.Append("] ");

            var propertyType = property.PropertyType;

            sqlBuilder.Append(SqlServerTypes.GetSqlServerDataType(propertyType, enumSerializationMode));

            if (
                propertyType == typeof(String)
                ||
                (
                    propertyType.IsEnumOrNullableEnumType() &&
                    enumSerializationMode == EnumSerializationMode.Strings
                )
            )
            {
                sqlBuilder.Append(" COLLATE ");
                sqlBuilder.Append(collation);
            }

            prependComma = true;
        }

        sqlBuilder.Append(')');

        return sqlBuilder.ToString();
    }

    /// <summary>
    /// Builds an SQL code to create a single-column temporary table to be populated with values of the type
    /// <paramref name="valuesType" />.
    /// </summary>
    /// <param name="tableName">The name of the temporary table to create.</param>
    /// <param name="values">The values to populate the temporary table with.</param>
    /// <param name="valuesType">The type of values the temporary table will be populated with.</param>
    /// <param name="collation">The collation to use for text columns.</param>
    /// <param name="enumSerializationMode">The mode to use to serialize <see cref="Enum" /> values.</param>
    private static String BuildCreateSingleColumnTemporaryTableSqlCode(
        String tableName,
        IEnumerable values,
        Type valuesType,
        String collation,
        EnumSerializationMode enumSerializationMode
    )
    {
        using var sqlBuilder = new ValueStringBuilder(stackalloc Char[100]);

        sqlBuilder.Append("CREATE TABLE [");
        sqlBuilder.Append(tableName);
        sqlBuilder.Append("] ([Value] ");

        if (valuesType == typeof(String))
        {
            var maxLength = 0;

            foreach (String? value in values)
            {
                if (value?.Length > maxLength)
                {
                    maxLength = value.Length;
                }
            }

            switch (maxLength)
            {
                case 0:
                    sqlBuilder.Append("NVARCHAR(1)");
                    break;

                case <= 4000:
                    sqlBuilder.Append("NVARCHAR(");
                    sqlBuilder.Append(maxLength);
                    sqlBuilder.Append(')');
                    break;

                default:
                    sqlBuilder.Append("NVARCHAR(MAX)");
                    break;
            }
        }
        else
        {
            sqlBuilder.Append(SqlServerTypes.GetSqlServerDataType(valuesType, enumSerializationMode));
        }

        if (
            valuesType == typeof(String)
            ||
            (
                valuesType.IsEnumOrNullableEnumType() &&
                enumSerializationMode == EnumSerializationMode.Strings
            )
        )
        {
            sqlBuilder.Append(" COLLATE ");
            sqlBuilder.Append(collation);
        }

        sqlBuilder.Append(')');

        return sqlBuilder.ToString();
    }

    /// <summary>
    /// Creates an <see cref="IDataReader" /> that reads data from the specified sequence of values.
    /// </summary>
    /// <param name="values">The sequence containing the values to be read.</param>
    /// <param name="valuesType">The type of values in <paramref name="values" />.</param>
    /// <returns>An <see cref="IDataReader" /> that provides access to the data in <paramref name="values" />.</returns>
    private static IDataReader CreateValuesDataReader(IEnumerable values, Type valuesType)
    {
        if (valuesType.IsBuiltInTypeOrNullableBuildInType() || valuesType.IsEnumOrNullableEnumType())
        {
            return new EnumerableReader(values, valuesType, "Value");
        }

        return new ObjectReader(
            valuesType,
            values,
            EntityHelper.GetEntityReadablePropertyNames(valuesType)
        );
    }

    /// <summary>
    /// Drops the temporary table with the specified name.
    /// </summary>
    /// <param name="name">The name of the temporary table to drop.</param>
    /// <param name="connection">The connection to use to drop the table.</param>
    /// <param name="transaction">The transaction within to drop the table.</param>
    private static void DropTable(String name, SqlConnection connection, SqlTransaction? transaction)
    {
        using var command = Dependencies.SqlCommandFactory.CreateSqlCommand(
            connection,
            $"IF OBJECT_ID('tempdb..{name}', 'U') IS NOT NULL DROP TABLE [{name}]",
            transaction
        );

        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Asynchronously drops the temporary table with the specified name.
    /// </summary>
    /// <param name="name">The name of the temporary table to drop.</param>
    /// <param name="connection">The connection to use to drop the table.</param>
    /// <param name="transaction">The transaction within to drop the table.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static async ValueTask DropTableAsync(String name, SqlConnection connection, SqlTransaction? transaction)
    {
        using var command = Dependencies.SqlCommandFactory.CreateSqlCommand(
            connection,
            $"IF OBJECT_ID('tempdb..{name}', 'U') IS NOT NULL DROP TABLE [{name}]",
            transaction
        );

        await command.ExecuteNonQueryAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the collation of the database the specified connection is currently connected to.
    /// </summary>
    /// <param name="connection">The connection to the database of which to get the collation.</param>
    /// <param name="transaction">The SQL transaction within to perform the operation.</param>
    /// <returns>The collation of the database the specified connection is currently connected to.</returns>
    private static String GetCurrentDatabaseCollation(
        SqlConnection connection,
        SqlTransaction? transaction = null
    ) =>
        databaseCollationPerDatabase.GetOrAdd(
            connection.Database,
            static (_, args) =>
            {
                using var command = Dependencies.SqlCommandFactory.CreateSqlCommand(
                    args.connection,
                    GetCurrentDatabaseCollationQuery,
                    args.transaction
                );
                return (String)command.ExecuteScalar()!;
            },
            (connection, transaction)
        );

    /// <summary>
    /// Asynchronously gets the collation of the database the specified connection is currently connected to.
    /// </summary>
    /// <param name="connection">The connection to the database of which to get the collation.</param>
    /// <param name="transaction">The SQL transaction within to perform the operation.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// <see cref="ValueTask{TResult}.Result" /> will contain the collation of the database the specified connection is
    /// currently connected to.
    /// </returns>
    private static async ValueTask<String> GetCurrentDatabaseCollationAsync(
        SqlConnection connection,
        SqlTransaction? transaction = null
    )
    {
        if (databaseCollationPerDatabase.TryGetValue(connection.Database, out var collation))
        {
            return collation;
        }

        using var command = Dependencies.SqlCommandFactory.CreateSqlCommand(
            connection,
            GetCurrentDatabaseCollationQuery,
            transaction
        );

        collation = (String)(await command.ExecuteScalarAsync().ConfigureAwait(false))!;

        return databaseCollationPerDatabase.GetOrAdd(connection.Database, collation);
    }

    private const String GetCurrentDatabaseCollationQuery =
        "SELECT CONVERT (VARCHAR(256), DATABASEPROPERTYEX(DB_NAME(), 'collation'))";

    private static readonly ConcurrentDictionary<String, String> databaseCollationPerDatabase = [];
}
