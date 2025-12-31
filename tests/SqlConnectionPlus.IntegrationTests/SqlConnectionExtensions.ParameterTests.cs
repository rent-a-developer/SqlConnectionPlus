namespace RentADeveloper.SqlConnectionPlus.IntegrationTests;

public class SqlConnectionExtensions_ParameterTests : DatabaseTestsBase
{
    [Fact]
    public void Parameter_MultipleParameters_ShouldPassValuesAsParameters()
    {
        const Int64 int64 = 123L;
        var guid = Guid.NewGuid();
        var dateTime = new DateTime(2025, 12, 31, 23, 59, 59);

        this.Connection
            .QueryTuples<(Int64, Guid, DateTime)>(
                $"SELECT {Parameter(int64)}, {Parameter(guid)}, {Parameter(dateTime)}",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().BeEquivalentTo([(int64, guid, dateTime)]);
    }

    [Fact]
    public void Parameter_ShouldPassValueAsParameter()
    {
        const Int64 int64 = 123L;
        this.Connection
            .ExecuteScalar<Int64>(
                $"SELECT {Parameter(int64)}",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().Be(int64);

        var guid = Guid.NewGuid();
        this.Connection
            .ExecuteScalar<Guid>(
                $"SELECT {Parameter(guid)}",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().Be(guid);

        var dateTime = new DateTime(2025, 12, 31, 23, 59, 59);
        this.Connection
            .ExecuteScalar<DateTime>(
                $"SELECT {Parameter(dateTime)}",
                cancellationToken: TestContext.Current.CancellationToken
            )
            .Should().Be(dateTime);
    }
}
