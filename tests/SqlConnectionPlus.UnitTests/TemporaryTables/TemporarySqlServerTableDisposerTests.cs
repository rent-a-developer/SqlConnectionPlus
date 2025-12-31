using RentADeveloper.SqlConnectionPlus.TemporaryTables;

namespace RentADeveloper.SqlConnectionPlus.UnitTests.TemporaryTables;

public class TemporarySqlServerTableDisposerTests : TestsBase
{
    [Fact]
    public void Dispose_ShouldCallDropFunction()
    {
        var dropTableFunction = Substitute.For<Action>();
        var dropTableAsyncFunction = Substitute.For<Func<ValueTask>>();

        var disposer = new TemporarySqlServerTableDisposer(dropTableFunction, dropTableAsyncFunction);
        disposer.Dispose();

        dropTableFunction.Received(1).Invoke();
    }

    [Fact]
    public async Task DisposeAsync_ShouldCallAsyncDropFunction()
    {
        var dropTableFunction = Substitute.For<Action>();
        var dropTableAsyncFunction = Substitute.For<Func<ValueTask>>();

        var disposer = new TemporarySqlServerTableDisposer(dropTableFunction, dropTableAsyncFunction);

        await disposer.DisposeAsync();

        await dropTableAsyncFunction.Received(1).Invoke();
    }

    [Fact]
    public void VerifyNullArgumentGuards()
    {
        var dropTableFunction = () => { };
        var dropTableAsyncFunction = () => ValueTask.CompletedTask;

        ArgumentNullGuardVerifier.Verify(() =>
            new TemporarySqlServerTableDisposer(dropTableFunction, dropTableAsyncFunction));
    }
}
