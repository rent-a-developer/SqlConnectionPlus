using System.ComponentModel.DataAnnotations.Schema;

namespace RentADeveloper.SqlConnectionPlus.UnitTests.TestData;

[Table("Entity")]
public record EntityWithTableAttribute : Entity;
