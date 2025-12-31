// Copyright (c) 2025 David Liebeherr
// Licensed under the MIT License. See LICENSE.md in the project root for more information.

using RentADeveloper.SqlConnectionPlus.SqlStatements;

namespace RentADeveloper.SqlConnectionPlus;

/// <summary>
/// Provides extension members for the type <see cref="SqlConnection" />.
/// </summary>
public static partial class SqlConnectionExtensions
{
    /// <summary>
    /// Wraps <paramref name="values" /> in an instance of <see cref="InterpolatedTemporaryTable" /> to indicate that
    /// this sequence of values should be passed as a temporary table to an SQL statement.
    /// 
    /// Use this method to pass a sequence of scalar values or complex objects in an interpolated string as a temporary
    /// table to an SQL statement.
    /// </summary>
    /// <typeparam name="T">The type of values in <paramref name="values" />.</typeparam>
    /// <param name="values">The sequence of scalar values or complex objects to pass as a temporary table.</param>
    /// <param name="valuesExpression">
    /// The expression from which <paramref name="valuesExpression" /> was obtained.
    /// Used to infer the name for the temporary table.
    /// This parameter is optional and is automatically provided by the compiler.
    /// </param>
    /// <returns>
    /// An instance of <see cref="InterpolatedTemporaryTable" /> indicating that <paramref name="values" /> should be
    /// passed as a temporary table to an SQL statement.
    /// </returns>
    /// <remarks>
    /// To use this method import <see cref="SqlConnectionExtensions" /> with a using directive with the static
    /// modifier:
    /// <code>
    /// using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;
    /// </code>
    /// You can pass a sequence of scalar values (e.g. <see cref="String" />, <see cref="Int32" />,
    /// <see cref="DateTime" />, <see cref="Enum" /> and so on) or a sequence of complex objects.
    /// 
    /// If a sequence of scalar values is passed, the temporary table will have a single column named "Value" with
    /// a data type that matches the type of the passed values.
    /// 
    /// Example:
    /// <code>
    /// <![CDATA[
    /// using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;
    /// 
    /// var retiredSupplierIds = suppliers.Where(a => a.IsRetired).Select(a => a.Id);
    /// 
    /// var retiredSupplierProductsReader = connection.ExecuteReader(
    ///    $"""
    ///     SELECT  *
    ///     FROM    Product
    ///     WHERE   SupplierId IN (
    ///                 SELECT  Value
    ///                 FROM    {TemporaryTable(retiredSupplierIds)}
    ///             )
    ///     """
    /// );
    /// ]]>
    /// </code>
    /// This will create a temporary table with a single column named "Value" and with a data type that matches the
    /// type of the passed values:
    /// <code>
    /// CREATE TABLE #RetiredSupplierIds_48d42afd5d824a27bd9352676ab6c198
    /// (
    ///     Value BIGINT
    /// )
    /// </code>
    /// If a sequence of complex objects is passed, the temporary table will have multiple columns.
    /// The temporary table will contain a column for each property (with a public getter) of the passed objects.
    /// The name of each column will be the name of the corresponding property.
    /// The data type of each column will be compatible with the property type of the corresponding property.
    /// 
    /// Example:
    /// <code>
    /// <![CDATA[
    /// using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;
    /// 
    /// class OrderItem
    /// {
    ///     public Int64 ProductId { get; set; }
    ///     public DateTime OrderDate { get; set; }
    /// }
    /// 
    /// var orderItems = GetOrderItems();
    /// var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
    /// 
    /// var productsOrderedInPastSixMonthsReader = connection.ExecuteReader(
    ///     $"""
    ///      SELECT     *
    ///      FROM       Product
    ///      WHERE      EXISTS (
    ///                     SELECT  1
    ///                     FROM    {TemporaryTable(orderItems)} TOrderItem
    ///                     WHERE   TOrderItem.ProductId = Product.Id AND
    ///                             TOrderItem.OrderDate >= {Parameter(sixMonthsAgo)}
    ///                 )
    ///      """
    /// );
    /// ]]>
    /// </code>
    /// This will create a temporary table with columns matching the properties of the passed objects:
    /// <code>
    /// CREATE TABLE #OrderItems_d6545835d97148ab93709efe9ba1f110
    /// (
    ///     ProductId BIGINT,
    ///     OrderDate DATETIME2
    /// )
    /// </code>
    /// The name of the temporary table will be inferred from the expression passed to <see cref="TemporaryTable{T}" />
    /// and suffixed with a new Guid to avoid naming conflicts (e.g. "#OrderItems_395c98f203514e81aa0098ec7f13e8a2").
    /// 
    /// If the name cannot be inferred from the expression the name "#Values" (also suffixed with a new Guid) will be
    /// used (e.g. "#Values_395c98f203514e81aa0098ec7f13e8a2").
    /// 
    /// If you pass enum values or objects containing enum properties, the enum values are serialized according to the
    /// setting <see cref="SqlConnectionExtensions.EnumSerializationMode" />.
    /// 
    /// When <see cref="SqlConnectionExtensions.EnumSerializationMode" /> is set to
    /// <see cref="EnumSerializationMode.Strings" />, the data type of the corresponding column in the temporary table
    /// will be <code>NVARCHAR(200)</code>.
    /// 
    /// When <see cref="SqlConnectionExtensions.EnumSerializationMode" /> is set to
    /// <see cref="EnumSerializationMode.Integers" />, the data type of the corresponding column in the temporary table
    /// will be <code>INT</code>.
    /// </remarks>
    public static InterpolatedTemporaryTable TemporaryTable<T>(
        IEnumerable<T> values,
        [CallerArgumentExpression(nameof(values))]
        String? valuesExpression = null
    )
    {
        String? temporaryTableName = null;

        if (!String.IsNullOrWhiteSpace(valuesExpression))
        {
            var nameFromCallerArgumentExpression = NameHelper.CreateNameFromCallerArgumentExpression(
                valuesExpression,
                // SQL Server temporary table names have a maximum length of 116; minus the GUID suffix is 84.
                84
            );

            if (!String.IsNullOrWhiteSpace(nameFromCallerArgumentExpression))
            {
                temporaryTableName = "#" + nameFromCallerArgumentExpression + "_" + Guid.NewGuid().ToString("N");
            }
        }

        if (String.IsNullOrWhiteSpace(temporaryTableName))
        {
            temporaryTableName = "#Values_" + Guid.NewGuid().ToString("N");
        }

        return new(temporaryTableName, values, typeof(T));
    }
}
