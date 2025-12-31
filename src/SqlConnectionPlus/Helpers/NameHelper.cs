// Copyright (c) 2025 David Liebeherr
// Licensed under the MIT License. See LICENSE.md in the project root for more information.

namespace RentADeveloper.SqlConnectionPlus.Helpers;

/// <summary>
/// Provides helper functions for name inference.
/// </summary>
internal static class NameHelper
{
    /// <summary>
    /// Creates a name suitable for a parameter or temporary table from the specified expression obtained through
    /// <see cref="CallerArgumentExpressionAttribute" /> and truncates the name to the specified maximum length.
    /// </summary>
    /// <param name="expression">The expression from which to create a name.</param>
    /// <param name="maximumLength">The maximum length of the name to return.</param>
    /// <returns>The name created from <paramref name="expression" />.</returns>
    internal static String CreateNameFromCallerArgumentExpression(ReadOnlySpan<Char> expression, Int32 maximumLength)
    {
        if (expression.StartsWith("this.", StringComparison.OrdinalIgnoreCase))
        {
            expression = expression[5..];
        }

        if (expression.StartsWith("new", StringComparison.OrdinalIgnoreCase))
        {
            expression = expression[3..];
        }

        if (expression.StartsWith("Get", StringComparison.OrdinalIgnoreCase))
        {
            expression = expression[3..];
        }

        var buffer = expression.Length <= 512 ? stackalloc Char[expression.Length] : new Char[expression.Length];
        var count = 0;

        foreach (var character in expression)
        {
            if (count >= maximumLength)
            {
                break;
            }

            if (character is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9' or '_')
            {
                buffer[count++] = character;
            }
        }

        if (count > 0 && Char.IsLower(buffer[0]))
        {
            buffer[0] = Char.ToUpper(buffer[0], CultureInfo.InvariantCulture);
        }

        return new(buffer[..count]);
    }
}
