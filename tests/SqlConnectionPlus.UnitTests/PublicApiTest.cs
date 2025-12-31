using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using PublicApiGenerator;

namespace RentADeveloper.SqlConnectionPlus.UnitTests;

public class PublicApiTest : TestsBase
{
    [Fact]
    [Description("Verifies that the public API of SqlConnectionPlus has not been changed unnoticed.")]
    public Task PublicApiHasNotChanged()
    {
        var assembly = typeof(SqlConnectionExtensions).Assembly;

        var options = new ApiGeneratorOptions
        {
            ExcludeAttributes =
            [
                typeof(InternalsVisibleToAttribute).FullName!,
                typeof(TargetFrameworkAttribute).FullName!
            ],
            DenyNamespacePrefixes = []
        };

        var publicApi = assembly.GeneratePublicApi(options);

        return Verify(publicApi);
    }
}
