namespace RentADeveloper.SqlConnectionPlus.UnitTests;

/// <summary>
/// Base class for tests.
/// </summary>
public class TestsBase
{
    public TestsBase()
    {
        // Ensure consistent culture for tests.
        Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = new("en-US");

        // Reset all settings to defaults before each test.
        SqlConnectionExtensions.EnumSerializationMode = EnumSerializationMode.Strings;
    }
}
