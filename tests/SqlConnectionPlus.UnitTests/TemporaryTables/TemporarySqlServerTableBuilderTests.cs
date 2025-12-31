using RentADeveloper.SqlConnectionPlus.TemporaryTables;
using RentADeveloper.SqlConnectionPlus.UnitTests.TestData;

namespace RentADeveloper.SqlConnectionPlus.UnitTests.TemporaryTables;

public class TemporarySqlServerTableBuilderTests : TestsBase
{
    [Fact]
    public void BuildTemporaryTable_NameIsNullOrEmptyOrWhitespace_ShouldThrow()
    {
        Invoking(() =>
                TemporarySqlServerTableBuilder.BuildTemporaryTable(new(), null, "", new List<Int32> { 1 },
                    typeof(Int32))
            )
            .Should().Throw<ArgumentException>();

        Invoking(() =>
                TemporarySqlServerTableBuilder.BuildTemporaryTable(new(), null, " ", new List<Int32> { 1 },
                    typeof(Int32))
            )
            .Should().Throw<ArgumentException>();
    }

    [Fact]
    public async Task BuildTemporaryTableAsync_NameIsNullOrEmptyOrWhitespace_ShouldThrow()
    {
        await Invoking(() =>
                TemporarySqlServerTableBuilder.BuildTemporaryTableAsync(new(), null, "",
                    new List<Int32> { 1 }, typeof(Int32))
            )
            .Should().ThrowAsync<ArgumentException>();

        await Invoking(() =>
                TemporarySqlServerTableBuilder.BuildTemporaryTableAsync(new(), null, " ",
                    new List<Int32> { 1 }, typeof(Int32))
            )
            .Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public void VerifyNullArgumentGuards()
    {
        var entityIds = Generate.EntityIds(Generate.SmallNumber());

        ArgumentNullGuardVerifier.Verify(() =>
            TemporarySqlServerTableBuilder.BuildTemporaryTable(new(), null, "#Values", entityIds, typeof(Int64),
                CancellationToken.None)
        );

        ArgumentNullGuardVerifier.Verify(() =>
            TemporarySqlServerTableBuilder.BuildTemporaryTableAsync(new(), null, "#Values", entityIds, typeof(Int64),
                CancellationToken.None)
        );
    }
}
