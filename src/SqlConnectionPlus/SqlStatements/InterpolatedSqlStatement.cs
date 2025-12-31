// Copyright (c) 2025 David Liebeherr
// Licensed under the MIT License. See LICENSE.md in the project root for more information.

using System.Diagnostics;
using System.Text;
using LinkDotNet.StringBuilder;

namespace RentADeveloper.SqlConnectionPlus.SqlStatements;

/// <summary>
/// Represents an SQL statement constructed using interpolated string syntax.
/// </summary>
/// <remarks>
/// This type enables passing values as parameters and sequences of values as temporary tables to SQL statements via
/// expressions inside interpolated strings.
/// Therefore, this type implements the C# interpolated string handler pattern
/// (see https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/tutorials/interpolated-string-handler).
/// </remarks>
[InterpolatedStringHandler]
[DebuggerTypeProxy(typeof(InterpolatedSqlStatementDebugView))]
// ReSharper disable once StructCanBeMadeReadOnly
public struct InterpolatedSqlStatement : IEquatable<InterpolatedSqlStatement>
{
    /// <summary>Initializes a new instance of this type.</summary>
    /// <param name="literalLength">The length of the interpolated string.</param>
    /// <param name="formattedCount">The number of expressions used in the interpolated string.</param>
    /// <remarks>
    /// This constructor is part of the interpolated string handler pattern.
    /// It is not intended to be called by user code.
    /// </remarks>
    public InterpolatedSqlStatement(Int32 literalLength, Int32 formattedCount)
    {
        this.codeBuilder = new(literalLength);
        this.parameters = new(formattedCount, StringComparer.OrdinalIgnoreCase);
        this.temporaryTables = [];
    }

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="code">The code of the SQL statement.</param>
    /// <param name="parameters">The parameters of the SQL statement.</param>
    /// <exception cref="ArgumentNullException"><paramref name="parameters" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="parameters" /> contains a duplicate parameter.</exception>
    /// <remarks>
    /// If a parameter value is an <see cref="Enum" />, it is serialized according to
    /// <see cref="SqlConnectionExtensions.EnumSerializationMode" />.
    /// </remarks>
    public InterpolatedSqlStatement(String? code, params (String Name, Object? Value)[] parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        this.codeBuilder = new(code?.Length ?? 0);
        this.parameters = new(parameters.Length, StringComparer.OrdinalIgnoreCase);
        this.temporaryTables = [];

        foreach (var parameter in parameters)
        {
            if (this.parameters.ContainsKey(parameter.Name))
            {
                throw new ArgumentException(
                    $"Duplicate parameter name '{parameter.Name}'. Make sure each parameter name is only " +
                    $"used once.",
                    nameof(parameters)
                );
            }

            if (parameter.Value is Enum enumValue)
            {
                this.parameters.Add(
                    parameter.Name,
                    EnumSerializer.SerializeEnum(enumValue, SqlConnectionExtensions.EnumSerializationMode)
                );
            }
            else
            {
                this.parameters.Add(parameter.Name, parameter.Value);
            }
        }

        if (!String.IsNullOrEmpty(code))
        {
            this.codeBuilder.Append(code);
        }
    }

    /// <summary>
    /// Appends the specified value to this instance.
    /// </summary>
    /// <typeparam name="T">The type of value to append.</typeparam>
    /// <param name="value">The value to append.</param>
    /// <param name="alignment">
    /// The minimum number of characters that should be written for <paramref name="value" />.
    /// A negative value indicates that the value should be left-aligned and the required minimum whitespace characters
    /// to add is the absolute value.
    /// </param>
    /// <param name="format">The string to use to format <paramref name="value" />.</param>
    /// <remarks>
    /// This method is part of the interpolated string handler pattern.
    /// It is not intended to be called by user code.
    /// </remarks>
    public void AppendFormatted<T>(T? value, Int32 alignment = 0, String? format = null)
    {
        switch (value)
        {
            case InterpolatedParameter interpolatedParameter:
                var parameterName = interpolatedParameter.InferredName;

                if (String.IsNullOrWhiteSpace(parameterName))
                {
                    parameterName = "@Parameter_" + (this.parameters.Count + 1).ToString(CultureInfo.InvariantCulture);
                }

                if (this.parameters.ContainsKey(parameterName))
                {
                    var suffix = 2;

                    var newParameterName = parameterName + suffix.ToString(CultureInfo.InvariantCulture);

                    while (this.parameters.ContainsKey(newParameterName))
                    {
                        suffix++;
                        newParameterName = parameterName + suffix.ToString(CultureInfo.InvariantCulture);
                    }

                    parameterName = newParameterName;
                }

                var parameterValue = interpolatedParameter.Value;

                if (parameterValue is Enum enumValue)
                {
                    parameterValue = EnumSerializer.SerializeEnum(
                        enumValue,
                        SqlConnectionExtensions.EnumSerializationMode
                    );
                }

                this.parameters.Add(parameterName, parameterValue);
                this.codeBuilder.Append(parameterName);
                break;

            case InterpolatedTemporaryTable interpolatedTemporaryTable:
                this.temporaryTables.Add(interpolatedTemporaryTable);
                this.codeBuilder.Append(interpolatedTemporaryTable.Name);
                break;

            default:
                var formattedValue =
                    value switch
                    {
                        String stringValue => stringValue,
                        IFormattable formattable => formattable.ToString(format, CultureInfo.InvariantCulture),
                        null => String.Empty,
                        _ => value.ToString() ?? String.Empty
                    };

                if (alignment != 0)
                {
                    var paddingWidth = Math.Abs(alignment);
                    var padding = paddingWidth - formattedValue.Length;

                    if (padding > 0)
                    {
                        if (alignment > 0)
                        {
                            // Right-align:
                            this.codeBuilder.Append(' ', padding);
                            this.codeBuilder.Append(formattedValue);
                        }
                        else
                        {
                            // Left-align:
                            this.codeBuilder.Append(formattedValue);
                            this.codeBuilder.Append(' ', padding);
                        }

                        break;
                    }
                }

                this.codeBuilder.Append(formattedValue);
                break;
        }
    }

    /// <summary>
    /// Appends the specified literal value to this instance.
    /// </summary>
    /// <param name="value">The literal value to append to this instance.</param>
    /// <remarks>
    /// This method is part of the interpolated string handler pattern.
    /// It is not intended to be called by user code.
    /// </remarks>
    public void AppendLiteral(String? value) =>
        this.codeBuilder.Append(value ?? String.Empty);

    /// <inheritdoc />
    public Boolean Equals(InterpolatedSqlStatement other) =>
        EqualityComparer<String>.Default.Equals(this.codeBuilder.ToString(), other.codeBuilder.ToString()) &&
        // ReSharper disable once UsageOfDefaultStructEquality
        this.parameters.SequenceEqual(other.parameters) &&
        this.temporaryTables.SequenceEqual(other.temporaryTables);

    /// <inheritdoc />
    public override Boolean Equals(Object? obj) =>
        obj is InterpolatedSqlStatement other && this.Equals(other);

    /// <inheritdoc />
    public override Int32 GetHashCode()
    {
        var hashCode = new HashCode();

        hashCode.Add(this.codeBuilder.ToString());

        foreach (var parameter in this.parameters)
        {
            hashCode.Add(parameter.Key);
            hashCode.Add(parameter.Value);
        }

        foreach (var temporaryTable in this.temporaryTables)
        {
            hashCode.Add(temporaryTable);
        }

        return hashCode.ToHashCode();
    }

    /// <inheritdoc />
    public override String ToString()
    {
        using var stringBuilder = new ValueStringBuilder(stackalloc Char[500]);

        stringBuilder.AppendLine("SQL Statement");
        stringBuilder.AppendLine("");

        stringBuilder.AppendLine("Statement Code");
        stringBuilder.AppendLine("--------------");
        stringBuilder.AppendLine(this.codeBuilder.ToString());
        stringBuilder.AppendLine("--------------");

        stringBuilder.AppendLine();
        stringBuilder.AppendLine("Statement Parameters");
        stringBuilder.AppendLine("--------------------");

        foreach (var (name, value) in this.parameters)
        {
            stringBuilder.Append(name);
            stringBuilder.Append(" = ");
            stringBuilder.AppendLine(value.ToDebugString());
        }

        stringBuilder.AppendLine();
        stringBuilder.AppendLine("Statement Temporary Tables");
        stringBuilder.AppendLine("--------------------------");

        foreach (var temporaryTable in this.temporaryTables)
        {
            stringBuilder.AppendLine(temporaryTable.Name);
            stringBuilder.AppendLine(new String('-', temporaryTable.Name.Length + 1));

            foreach (var value in temporaryTable.Values)
            {
                stringBuilder.AppendLine(value.ToDebugString());
            }

            stringBuilder.AppendLine();
        }

        return stringBuilder.ToString();
    }

    /// <summary>
    /// Creates a new instance of <see cref="InterpolatedSqlStatement" /> from the specified string.
    /// </summary>
    /// <param name="value">
    /// The string from which to create an instance of <see cref="InterpolatedSqlStatement" />.
    /// </param>
    public static InterpolatedSqlStatement FromString(String value) =>
        new(value);

    /// <summary>
    /// Determines whether the two specified instances of <see cref="InterpolatedSqlStatement" /> are equal.
    /// </summary>
    /// <param name="left">The first instance to compare.</param>
    /// <param name="right">The second instance to compare.</param>
    /// <returns>
    /// <see langword="true" /> if the two specified instances of <see cref="InterpolatedSqlStatement" /> are
    /// equal; otherwise, <see langword="false" />.
    /// </returns>
    public static Boolean operator ==(InterpolatedSqlStatement left, InterpolatedSqlStatement right) =>
        left.Equals(right);

    /// <summary>
    /// Implicitly converts a string to an instance of <see cref="InterpolatedSqlStatement" />.
    /// </summary>
    /// <param name="value">The string to convert to an instance of <see cref="InterpolatedSqlStatement" />.</param>
    public static implicit operator InterpolatedSqlStatement(String value) =>
        new(value);

    /// <summary>
    /// Determines whether the two specified instances of <see cref="InterpolatedSqlStatement" /> are unequal.
    /// </summary>
    /// <param name="left">The first instance to compare.</param>
    /// <param name="right">The second instance to compare.</param>
    /// <returns>
    /// <see langword="true" /> if the two the specified instances of <see cref="InterpolatedSqlStatement" /> are
    /// unequal; otherwise, <see langword="false" />.
    /// </returns>
    public static Boolean operator !=(InterpolatedSqlStatement left, InterpolatedSqlStatement right) =>
        !(left == right);

    /// <summary>
    /// The code of this SQL statement.
    /// </summary>
    internal String Code => this.codeBuilder.ToString();

    /// <summary>
    /// The parameters of this SQL statement.
    /// The keys are the parameter names, and the values are the parameter values.
    /// </summary>
    internal IReadOnlyDictionary<String, Object?> Parameters => this.parameters;

    /// <summary>
    /// The temporary tables used in this SQL statement.
    /// </summary>
    internal IReadOnlyList<InterpolatedTemporaryTable> TemporaryTables => this.temporaryTables;

    private readonly StringBuilder codeBuilder;
    private readonly Dictionary<String, Object?> parameters;
    private readonly List<InterpolatedTemporaryTable> temporaryTables;
}
