// Copyright (c) 2025 David Liebeherr
// Licensed under the MIT License. See LICENSE.md in the project root for more information.

using System.Diagnostics.CodeAnalysis;

namespace RentADeveloper.SqlConnectionPlus.Converters;

/// <summary>
/// Converts values to different types.
/// </summary>
internal static class ValueConverter
{
    /// <summary>
    /// Converts <paramref name="value" /> to the type <typeparamref name="TTarget" />.
    /// </summary>
    /// <typeparam name="TTarget">The type to convert <paramref name="value" /> to.</typeparam>
    /// <param name="value">The value to convert to the type <typeparamref name="TTarget" />.</param>
    /// <returns><paramref name="value" /> converted to the type <typeparamref name="TTarget" />.</returns>
    /// <exception cref="InvalidCastException">
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 <paramref name="value" /> is <see langword="null" /> or a <see cref="DBNull" /> value, but
    ///                 the type <typeparamref name="TTarget" /> is non-nullable.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <paramref name="value" /> could not be converted to the type <typeparamref name="TTarget" />,
    ///                 because that conversion is not supported.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <typeparamref name="TTarget" /> is <see cref="Char" /> or <see cref="Nullable{Char}" /> and
    /// <paramref name="value" /> is a string that has a length other than 1.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static TTarget? ConvertValueToType<TTarget>(Object? value)
    {
        var targetType = typeof(TTarget);

        switch (value)
        {
            case null or DBNull when default(TTarget) is null:
                return default;

            case null or DBNull when default(TTarget) is not null:
                ThrowCouldNotConvertNullToNonNullableTargetTypeException(value, targetType);
                return default;

            case TTarget alreadyTargetTypeValue:
                return alreadyTargetTypeValue;

            case String stringValue when targetType.IsCharOrNullableCharType():
                if (stringValue.Length != 1)
                {
                    ThrowCouldNotConvertNonSingleCharStringToChar(stringValue, targetType);
                }

                return (TTarget)(Object)stringValue[0];

            default:
                // Unwrap Nullable<T> types:
                var effectiveTargetType = Nullable.GetUnderlyingType(targetType) ?? targetType;

                try
                {
                    if (effectiveTargetType.IsEnum)
                    {
                        return EnumConverter.ConvertValueToEnumMember<TTarget>(value);
                    }

                    return (TTarget?)Convert.ChangeType(value, effectiveTargetType, CultureInfo.InvariantCulture);
                }
                catch (Exception exception) when (
                    exception is ArgumentException or InvalidCastException or FormatException or OverflowException
                )
                {
                    ThrowCouldNotConvertValueToTargetTypeException(value, targetType, exception);
                    return default;
                }
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    [DoesNotReturn]
    private static void ThrowCouldNotConvertNonSingleCharStringToChar(String stringValue, Type targetType) =>
        throw new InvalidCastException(
            $"Could not convert the string '{stringValue}' to the target type {targetType}. The string must be " +
            $"exactly one character long."
        );

    [MethodImpl(MethodImplOptions.NoInlining)]
    [DoesNotReturn]
    private static void ThrowCouldNotConvertNullToNonNullableTargetTypeException(Object? value, Type targetType) =>
        throw new InvalidCastException(
            $"Could not convert the value {value.ToDebugString()} to the target type {targetType}, because the " +
            $"target type is non-nullable."
        );

    [MethodImpl(MethodImplOptions.NoInlining)]
    [DoesNotReturn]
    private static void ThrowCouldNotConvertValueToTargetTypeException(
        Object? value,
        Type targetType,
        Exception innerException
    ) =>
        throw new InvalidCastException(
            $"Could not convert the value {value.ToDebugString()} to the target type {targetType}. " +
            $"See inner exception for details.",
            innerException
        );
}
