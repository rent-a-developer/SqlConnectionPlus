// Copyright (c) 2025 David Liebeherr
// Licensed under the MIT License. See LICENSE.md in the project root for more information.

namespace RentADeveloper.SqlConnectionPlus.SqlStatements;

/// <summary>
/// A parameter created from an interpolated value to be passed to an SQL statement.
/// </summary>
/// <param name="InferredName">
/// The name for the parameter inferred from the expression from which the parameter value was obtained.
/// This is <see langword="null" /> if no name could be inferred.
/// </param>
/// <param name="Value">The value of the parameter.</param>
public readonly record struct InterpolatedParameter(String? InferredName, Object? Value);
