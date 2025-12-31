using AwesomeAssertions.Specialized;

namespace RentADeveloper.SqlConnectionPlus.IntegrationTests.TestHelpers;

/// <summary>
/// Provides extension methods for asserting that a delegate throws an <see cref="SqlException" /> that is caused by a
/// timeout.
/// </summary>
public static class SqlExceptionAssertionExtensions
{
    /// <summary>
    /// Asserts that a delegate throws an <see cref="SqlException" /> that is caused by a timeout.
    /// </summary>
    /// <param name="assertions">The assertion object for the delegate to be tested.</param>
    /// <returns>An object that can be used to assert further on the thrown <see cref="SqlException" />.</returns>
    public static ExceptionAssertions<SqlException> ThrowTimeoutSqlException(this ActionAssertions assertions) =>
        assertions
            .Throw<SqlException>()
            .Where(e => e.Number == -2)
            .Where(e => e.Class == 11)
            .Where(e => e.State == 0);

    /// <summary>
    /// Asserts that a delegate throws an <see cref="SqlException" /> that is caused by a timeout.
    /// </summary>
    /// <typeparam name="TResult">The type of the value returned by the delegate.</typeparam>
    /// <param name="assertions">The assertion object for the delegate to be tested.</param>
    /// <returns>An object that can be used to assert further on the thrown <see cref="SqlException" />.</returns>
    public static ExceptionAssertions<SqlException> ThrowTimeoutSqlException<TResult>(
        this FunctionAssertions<TResult> assertions
    ) =>
        assertions
            .Throw<SqlException>()
            .Where(e => e.Number == -2)
            .Where(e => e.Class == 11)
            .Where(e => e.State == 0);

    /// <summary>
    /// Asserts that an asynchronous delegate throws an <see cref="SqlException" /> that is caused by a timeout.
    /// </summary>
    /// <param name="assertions">The assertion object for the asynchronous delegate to be tested.</param>
    /// <returns>An object that can be used to assert further on the thrown <see cref="SqlException" />.</returns>
    public static async Task<ExceptionAssertions<SqlException>> ThrowTimeoutSqlExceptionAsync(
        this NonGenericAsyncFunctionAssertions assertions
    )
    {
        var exceptionAssertions = await assertions.ThrowAsync<SqlException>();

        return exceptionAssertions
            .Where(e => e.Number == -2)
            .Where(e => e.Class == 11)
            .Where(e => e.State == 0);
    }

    /// <summary>
    /// Asserts that a delegate throws an <see cref="SqlException" /> that is caused by a timeout.
    /// </summary>
    /// <typeparam name="TResult">The type of the value returned by the delegate.</typeparam>
    /// <param name="assertions">The assertion object for the asynchronous delegate to be tested.</param>
    /// <returns>An object that can be used to assert further on the thrown <see cref="SqlException" />.</returns>
    public static Task<ExceptionAssertions<SqlException>> ThrowTimeoutSqlExceptionAsync<TResult>(
        this GenericAsyncFunctionAssertions<TResult> assertions
    ) =>
        assertions.ThrowAsync<SqlException>()
            .Where(e => e.Number == -2)
            .Where(e => e.Class == 11)
            .Where(e => e.State == 0);
}
