using RentADeveloper.SqlConnectionPlus.Readers;

namespace RentADeveloper.SqlConnectionPlus.IntegrationTests.Readers;

public class DisposeSignalingDataReaderDecoratorTests : DatabaseTestsBase
{
    [Fact]
    public void Read_OperationCancelledViaCancellationToken_ShouldThrowOperationCanceledException()
    {
        using var command = this.Connection.CreateCommand();
        command.CommandText = "SELECT 1; WAITFOR DELAY '00:00:01'; SELECT 1;";

        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        ThreadPool.QueueUserWorkItem(_ =>
            {
                Thread.Sleep(100);
                cancellationTokenSource.Cancel();
                // ReSharper disable once AccessToDisposedClosure
                command.Cancel();
            }
        );

        using var innerReader = command.ExecuteReader();

        using var reader = new DisposeSignalingDataReaderDecorator(innerReader, cancellationToken);

        // Read the value from before the delay:
        reader.Read()
            .Should().BeTrue();

        // ReSharper disable once AccessToDisposedClosure
        Invoking(() => reader.Read())
            .Should().Throw<OperationCanceledException>()
            .Where(a => a.CancellationToken == cancellationToken);
    }

    [Fact]
    public async Task ReadAsync_OperationCancelledViaCancellationToken_ShouldThrowOperationCanceledException()
    {
        using var command = this.Connection.CreateCommand();
        command.CommandText = "SELECT 1; WAITFOR DELAY '00:00:10'; SELECT 1;";

        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        ThreadPool.QueueUserWorkItem(_ =>
            {
                Thread.Sleep(100);
                cancellationTokenSource.Cancel();
                // ReSharper disable once AccessToDisposedClosure
                command.Cancel();
            }
        );

        await using var innerReader = await command.ExecuteReaderAsync(TestContext.Current.CancellationToken);

        await using var reader = new DisposeSignalingDataReaderDecorator(innerReader, cancellationToken);

        // Read the value from before the delay:
        (await reader.ReadAsync(TestContext.Current.CancellationToken))
            .Should().BeTrue();

        // ReSharper disable once AccessToDisposedClosure
        await Invoking(() => reader.ReadAsync(TestContext.Current.CancellationToken))
            .Should().ThrowAsync<OperationCanceledException>()
            .Where(a => a.CancellationToken == cancellationToken);
    }
}
