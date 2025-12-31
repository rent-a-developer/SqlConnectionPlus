// Copyright (c) 2025 David Liebeherr
// Licensed under the MIT License. See LICENSE.md in the project root for more information.

using System.Linq.Expressions;

namespace RentADeveloper.SqlConnectionPlus.Extensions;

/// <summary>
/// Provides extension members for the type <see cref="Type" />.
/// </summary>
internal static class TypeExtensions
{
    /// <summary>
    /// Creates a new instance of <see cref="List{T}" /> where T is <paramref name="elementType" />.
    /// </summary>
    /// <param name="elementType">The element type of the <see cref="List{T}" /> to create.</param>
    /// <returns>
    /// A new instance of <see cref="List{T}" /> with the specified element type <paramref name="elementType" />
    /// cast to <see cref="IList" />.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="elementType" /> is <see langword="null" />.</exception>
    internal static IList CreateListForType(this Type elementType)
    {
        ArgumentNullException.ThrowIfNull(elementType);

        return listCreatorPerElementType.GetOrAdd(
            elementType,
            static elementType2 =>
            {
                var listType = typeof(List<>).MakeGenericType(elementType2);
                var constructor = listType.GetConstructor(Type.EmptyTypes)!;

                return Expression
                    .Lambda<Func<IList>>(
                        Expression.Convert(
                            Expression.New(constructor),
                            typeof(IList)
                        )
                    )
                    .Compile();
            }
        )();
    }

    /// <summary>
    /// Determines whether this type is a built-in .NET type
    /// (e.g. <see cref="Boolean" />, <see cref="String" />, <see cref="Decimal" />, ...).
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns>
    /// <see langword="true" /> if this type is a built-in .NET type; otherwise, <see langword="false" />.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="type" /> is <see langword="null" />.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Boolean IsBuiltInTypeOrNullableBuildInType(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        return builtInTypes.Contains(Nullable.GetUnderlyingType(type) ?? type);
    }

    /// <summary>
    /// Determines whether this type is the type <see cref="Char" /> or <see cref="Nullable{Char}" />.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns>
    /// <see langword="true" /> if this type is the type <see cref="Char" /> or <see cref="Nullable{Char}" />;
    /// otherwise, <see langword="false" />.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="type" /> is <see langword="null" />.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Boolean IsCharOrNullableCharType(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        return type == typeof(Char) || type == typeof(Char?);
    }

    /// <summary>
    /// Determines whether this type is an <see cref="Enum" /> type or a nullable <see cref="Enum" /> type.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns>
    /// <see langword="true" /> if this type is an enum type or a nullable enum type; otherwise,
    /// <see langword="false" />.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="type" /> is <see langword="null" />.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Boolean IsEnumOrNullableEnumType(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        return type.IsEnum || Nullable.GetUnderlyingType(type) is { IsEnum: true };
    }

    /// <summary>
    /// Determines whether this type is a reference type or a nullable type (<see cref="Nullable{T}" />).
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns>
    /// <see langword="true" /> if this type is a reference type or a nullable type; otherwise,
    /// <see langword="false" />.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="type" /> is <see langword="null" />.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Boolean IsReferenceTypeOrNullableType(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        return !type.IsValueType || Nullable.GetUnderlyingType(type) is not null;
    }

    /// <summary>
    /// Determines whether this type is a <see cref="ValueTuple" /> type with up to 7 fields (e.g.
    /// <see cref="ValueTuple{T1}" />, <see cref="ValueTuple{T1, T2}" />, <see cref="ValueTuple{T1, T2, T3}" />, ...).
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns>
    /// <see langword="true" /> if this type is a <see cref="ValueTuple" /> type with up to 7 fields;
    /// otherwise, <see langword="false" />.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="type" /> is <see langword="null" />.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Boolean IsValueTupleTypeWithUpTo7Fields(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        return type.IsGenericType && valueTupleTypes.Contains(type.GetGenericTypeDefinition());
    }

    private static readonly HashSet<Type> builtInTypes =
    [
        typeof(Boolean),
        typeof(Byte),
        typeof(SByte),
        typeof(Char),
        typeof(Decimal),
        typeof(Double),
        typeof(Single),
        typeof(Int16),
        typeof(UInt16),
        typeof(Int32),
        typeof(UInt32),
        typeof(Int64),
        typeof(UInt64),
        typeof(IntPtr),
        typeof(UIntPtr),
        typeof(String),
        typeof(DateTime),
        typeof(DateOnly),
        typeof(DateTimeOffset),
        typeof(TimeSpan),
        typeof(TimeOnly),
        typeof(Guid)
    ];

    private static readonly ConcurrentDictionary<Type, Func<IList>> listCreatorPerElementType = [];

    private static readonly HashSet<Type> valueTupleTypes =
    [
        typeof(ValueTuple<>),
        typeof(ValueTuple<,>),
        typeof(ValueTuple<,,>),
        typeof(ValueTuple<,,,>),
        typeof(ValueTuple<,,,,>),
        typeof(ValueTuple<,,,,,>),
        typeof(ValueTuple<,,,,,,>)
    ];
}
