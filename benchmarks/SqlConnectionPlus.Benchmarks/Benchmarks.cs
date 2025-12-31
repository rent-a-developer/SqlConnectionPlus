using System.Xml.Linq;
using AwesomeAssertions;
using BenchmarkDotNet.Attributes;
using FastMember;
using Microsoft.Data.SqlClient;
using RentADeveloper.SqlConnectionPlus.Entities;
using RentADeveloper.SqlConnectionPlus.IntegrationTests.TestHelpers;
using RentADeveloper.SqlConnectionPlus.Readers;
using RentADeveloper.SqlConnectionPlus.UnitTests.TestData;
using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;

// @formatter:off
// ReSharper disable InconsistentNaming

#pragma warning disable IDE0017
#pragma warning disable CA1822
#pragma warning disable IDE0305

namespace RentADeveloper.SqlConnectionPlus.Benchmarks;

[MemoryDiagnoser]
[Config(typeof(BenchmarksConfig))]
public class Benchmarks
{
    [GlobalSetup]
    public void Setup_Global()
    {
        TestDatabaseManager.ResetDatabase();

        // Warm up connection pool.
        for (var i = 0; i < 20; i++)
        {
            using var warmUpConnection = CreateConnection();
            warmUpConnection.ExecuteScalar<Int32>("SELECT 1");
        }

        using var connection = CreateConnection();

        connection.ExecuteNonQuery("CHECKPOINT");
        connection.ExecuteNonQuery("DBCC DROPCLEANBUFFERS");
        connection.ExecuteNonQuery("DBCC FREEPROCCACHE");
    }

    private void SetEntitiesInDb(Int32 numberOfEntities)
    {
        using var connection = CreateConnection();

        connection.ExecuteNonQuery("DELETE FROM Entity");

        this.entitiesInDb = Generate.Entities(numberOfEntities);
        connection.InsertEntities(this.entitiesInDb);
    }

    private static SqlConnection CreateConnection()
    {
        var connection = TestDatabaseManager.CreateConnection();
        connection.ExecuteNonQuery("SET TRANSACTION ISOLATION LEVEL READ COMMITTED");
        return connection;
    }

    private List<Entity> entitiesInDb = [];

    #region DeleteEntities
    private const String DeleteEntities_Category = "DeleteEntities";
    private const Int32 DeleteEntities_EntitiesPerOperation = 100;
    private const Int32 DeleteEntities_OperationsPerInvoke = 20;

    [IterationSetup(Targets = [nameof(DeleteEntities_Manually), nameof(DeleteEntities_SqlConnectionPlus)])]
    public void DeleteEntities_Setup() =>
        this.SetEntitiesInDb(DeleteEntities_OperationsPerInvoke * DeleteEntities_EntitiesPerOperation);

    [Benchmark(Baseline = true, OperationsPerInvoke = DeleteEntities_OperationsPerInvoke)]
    [BenchmarkCategory(DeleteEntities_Category)]
    public void DeleteEntities_Manually()
    {
        using var connection = CreateConnection();

        for (var i = 0; i < DeleteEntities_OperationsPerInvoke; i++)
        {
            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Entity WHERE Id = @Id";

            var idParameter = new SqlParameter();
            idParameter.ParameterName = "@Id";
            command.Parameters.Add(idParameter);

            var entities = this.entitiesInDb.Take(DeleteEntities_EntitiesPerOperation).ToList();

            entities
                .Should().HaveCount(DeleteEntities_EntitiesPerOperation);

            foreach (var entity in entities)
            {
                idParameter.Value = entity.Id;

                command.ExecuteNonQuery()
                    .Should().Be(1);

                this.entitiesInDb.Remove(entity);
            }
        }
    }

    [Benchmark(Baseline = false, OperationsPerInvoke = DeleteEntities_OperationsPerInvoke)]
    [BenchmarkCategory(DeleteEntities_Category)]
    public void DeleteEntities_SqlConnectionPlus()
    {
        using var connection = CreateConnection();

        for (var i = 0; i < DeleteEntities_OperationsPerInvoke; i++)
        {
            var entities = this.entitiesInDb.Take(DeleteEntities_EntitiesPerOperation).ToList();

            entities
                .Should().HaveCount(DeleteEntities_EntitiesPerOperation);

            connection.DeleteEntities(entities)
                .Should().Be(DeleteEntities_EntitiesPerOperation);

            foreach (var entity in entities)
            {
                this.entitiesInDb.Remove(entity);
            }
        }
    }
    #endregion

    #region DeleteEntity
    private const String DeleteEntity_Category = "DeleteEntity";
    private const Int32 DeleteEntity_OperationsPerInvoke = 1200;

    [IterationSetup(Targets = [nameof(DeleteEntity_Manually), nameof(DeleteEntity_SqlConnectionPlus)])]
    public void DeleteEntity_Setup() =>
        this.SetEntitiesInDb(DeleteEntity_OperationsPerInvoke);

    [Benchmark(Baseline = true, OperationsPerInvoke = DeleteEntity_OperationsPerInvoke)]
    [BenchmarkCategory(DeleteEntity_Category)]
    public void DeleteEntity_Manually()
    {
        using var connection = CreateConnection();

        for (var i = 0; i < DeleteEntity_OperationsPerInvoke; i++)
        {
            var entityToDelete = this.entitiesInDb.First();

            using var command = connection.CreateCommand();

            command.CommandText = "DELETE FROM Entity WHERE Id = @Id";
            command.Parameters.Add(new("@Id", entityToDelete.Id));

            command.ExecuteNonQuery()
                .Should().Be(1);

            this.entitiesInDb.Remove(entityToDelete);
        }
    }

    [Benchmark(Baseline = false, OperationsPerInvoke = DeleteEntity_OperationsPerInvoke)]
    [BenchmarkCategory(DeleteEntity_Category)]
    public void DeleteEntity_SqlConnectionPlus()
    {
        using var connection = CreateConnection();

        for (var i = 0; i < DeleteEntity_OperationsPerInvoke; i++)
        {
            var entityToDelete = this.entitiesInDb.First();

            connection.DeleteEntity(entityToDelete)
                .Should().Be(1);

            this.entitiesInDb.Remove(entityToDelete);
        }
    }
    #endregion

    #region ExecuteNonQuery
    private const String ExecuteNonQuery_Category = "ExecuteNonQuery";
    private const Int32 ExecuteNonQuery_OperationsPerInvoke = 1100;

    [IterationSetup(Targets = [nameof(ExecuteNonQuery_Manually), nameof(ExecuteNonQuery_SqlConnectionPlus)])]
    public void ExecuteNonQuery_Setup() =>
        this.SetEntitiesInDb(ExecuteNonQuery_OperationsPerInvoke);

    [Benchmark(Baseline = true, OperationsPerInvoke = ExecuteNonQuery_OperationsPerInvoke)]
    [BenchmarkCategory(ExecuteNonQuery_Category)]
    public void ExecuteNonQuery_Manually()
    {
        using var connection = CreateConnection();

        for (var i = 0; i < ExecuteNonQuery_OperationsPerInvoke; i++)
        {
            var entity = this.entitiesInDb.First();

            using var command = connection.CreateCommand();

            command.CommandText = "DELETE FROM Entity WHERE Id = @Id";
            command.Parameters.Add(new("@Id", entity.Id));

            command.ExecuteNonQuery()
                .Should().Be(1);

            this.entitiesInDb.Remove(entity);
        }
    }

    [Benchmark(Baseline = false, OperationsPerInvoke = ExecuteNonQuery_OperationsPerInvoke)]
    [BenchmarkCategory(ExecuteNonQuery_Category)]
    public void ExecuteNonQuery_SqlConnectionPlus()
    {
        using var connection = CreateConnection();

        for (var i = 0; i < ExecuteNonQuery_OperationsPerInvoke; i++)
        {
            var entity = this.entitiesInDb.First();

            connection.ExecuteNonQuery($"DELETE FROM Entity WHERE Id = {Parameter(entity.Id)}")
                .Should().Be(1);

            this.entitiesInDb.Remove(entity);
        }
    }
    #endregion

    #region ExecuteReader
    private const String ExecuteReader_Category = "ExecuteReader";
    private const Int32 ExecuteReader_OperationsPerInvoke = 700;
    private const Int32 ExecuteReader_EntitiesPerOperation = 100;

    [GlobalSetup(Targets = [nameof(ExecuteReader_Manually), nameof(ExecuteReader_SqlConnectionPlus)])]
    public void ExecuteReader_Setup() =>
        this.SetEntitiesInDb(ExecuteReader_EntitiesPerOperation);

    [Benchmark(Baseline = true, OperationsPerInvoke = ExecuteReader_OperationsPerInvoke)]
    [BenchmarkCategory(ExecuteReader_Category)]
    public List<Entity> ExecuteReader_Manually()
    {
        using var connection = CreateConnection();

        var entities = new List<Entity>();

        for (var i = 0; i < ExecuteReader_OperationsPerInvoke; i++)
        {
            entities.Clear();

            using var command = connection.CreateCommand();
            command.CommandText = $"""
                                  SELECT
                                    TOP ({ExecuteReader_EntitiesPerOperation})
                                    [Id],
                                    [BooleanValue],
                                    [ByteValue],
                                    [CharValue],
                                    [DateTimeOffsetValue],
                                    [DateTimeValue],
                                    [DecimalValue],
                                    [DoubleValue],
                                    [EnumValue],
                                    [GuidValue],
                                    [Int16Value],
                                    [Int32Value],
                                    [Int64Value],
                                    [SingleValue],
                                    [StringValue],
                                    [TimeSpanValue]
                                  FROM
                                    Entity
                                  """;

            using var dataReader = command.ExecuteReader();

            while (dataReader.Read())
            {
                var charBuffer = new Char[1];

                entities.Add(new()
                {
                    Id = dataReader.GetInt64(0),
                    BooleanValue = dataReader.GetBoolean(1),
                    ByteValue = dataReader.GetByte(2),
                    CharValue = dataReader.GetChars(3, 0, charBuffer, 0, 1) == 1 ? charBuffer[0] : throw new(),
                    DateTimeOffsetValue = dataReader.GetDateTimeOffset(4),
                    DateTimeValue = dataReader.GetDateTime(5),
                    DecimalValue = dataReader.GetDecimal(6),
                    DoubleValue = dataReader.GetDouble(7),
                    EnumValue = Enum.Parse<TestEnum>(dataReader.GetString(8)),
                    GuidValue = dataReader.GetGuid(9),
                    Int16Value = dataReader.GetInt16(10),
                    Int32Value = dataReader.GetInt32(11),
                    Int64Value = dataReader.GetInt64(12),
                    SingleValue = dataReader.GetFloat(13),
                    StringValue = dataReader.GetString(14),
                    TimeSpanValue = dataReader.GetTimeSpan(15)
                });
            }

            entities
                .Should().HaveCount(ExecuteReader_EntitiesPerOperation);
        }

        return entities;
    }

    [Benchmark(Baseline = false, OperationsPerInvoke = ExecuteReader_OperationsPerInvoke)]
    [BenchmarkCategory(ExecuteReader_Category)]
    public List<Entity> ExecuteReader_SqlConnectionPlus()
    {
        using var connection = CreateConnection();

        var entities = new List<Entity>();

        for (var i = 0; i < ExecuteReader_OperationsPerInvoke; i++)
        {
            entities.Clear();

            using var dataReader = connection.ExecuteReader(
                $"""
                 SELECT
                     TOP ({ExecuteReader_EntitiesPerOperation})
                     [Id],
                     [BooleanValue],
                     [ByteValue],
                     [CharValue],
                     [DateTimeOffsetValue],
                     [DateTimeValue],
                     [DecimalValue],
                     [DoubleValue],
                     [EnumValue],
                     [GuidValue],
                     [Int16Value],
                     [Int32Value],
                     [Int64Value],
                     [SingleValue],
                     [StringValue],
                     [TimeSpanValue]
                 FROM
                     Entity
                 """
            );

            while (dataReader.Read())
            {
                var charBuffer = new Char[1];

                entities.Add(new()
                {
                    Id = dataReader.GetInt64(0),
                    BooleanValue = dataReader.GetBoolean(1),
                    ByteValue = dataReader.GetByte(2),
                    CharValue = dataReader.GetChars(3, 0, charBuffer, 0, 1) == 1 ? charBuffer[0] : throw new(),
                    DateTimeOffsetValue = (DateTimeOffset)dataReader.GetValue(4),
                    DateTimeValue = dataReader.GetDateTime(5),
                    DecimalValue = dataReader.GetDecimal(6),
                    DoubleValue = dataReader.GetDouble(7),
                    EnumValue = Enum.Parse<TestEnum>(dataReader.GetString(8)),
                    GuidValue = dataReader.GetGuid(9),
                    Int16Value = dataReader.GetInt16(10),
                    Int32Value = dataReader.GetInt32(11),
                    Int64Value = dataReader.GetInt64(12),
                    SingleValue = dataReader.GetFloat(13),
                    StringValue = dataReader.GetString(14),
                    TimeSpanValue = (TimeSpan)dataReader.GetValue(15)
                });
            }

            entities
                .Should().HaveCount(ExecuteReader_EntitiesPerOperation);
        }

        return entities;
    }
    #endregion

    #region ExecuteScalar
    private const String ExecuteScalar_Category = "ExecuteScalar";
    private const Int32 ExecuteScalar_OperationsPerInvoke = 5000;

    [GlobalSetup(Targets = [nameof(ExecuteScalar_Manually), nameof(ExecuteScalar_SqlConnectionPlus)])]
    public void ExecuteScalar_Setup() =>
        this.SetEntitiesInDb(ExecuteScalar_OperationsPerInvoke);

    [Benchmark(Baseline = true, OperationsPerInvoke = ExecuteScalar_OperationsPerInvoke)]
    [BenchmarkCategory(ExecuteScalar_Category)]
    public String ExecuteScalar_Manually()
    {
        using var connection = CreateConnection();

        String result = null!;

        for (var i = 0; i < ExecuteScalar_OperationsPerInvoke; i++)
        {
            var entity = this.entitiesInDb[i];

            using var command = connection.CreateCommand();

            command.CommandText = "SELECT StringValue FROM Entity WHERE Id = @Id";
            command.Parameters.Add(new("@Id", entity.Id));

            result = (String)command.ExecuteScalar()!;

            result
                .Should().Be(entity.StringValue);
        }

        return result;
    }

    [Benchmark(Baseline = false, OperationsPerInvoke = ExecuteScalar_OperationsPerInvoke)]
    [BenchmarkCategory(ExecuteScalar_Category)]
    public String ExecuteScalar_SqlConnectionPlus()
    {
        using var connection = CreateConnection();

        String result = null!;

        for (var i = 0; i < ExecuteScalar_OperationsPerInvoke; i++)
        {
            var entity = this.entitiesInDb[i];

            result = connection.ExecuteScalar<String>(
                $"SELECT StringValue FROM Entity WHERE Id = {Parameter(entity.Id)}"
            );

            result
                .Should().Be(entity.StringValue);
        }

        return result;
    }
    #endregion

    #region ExecuteXmlReader
    private const String ExecuteXmlReader_Category = "ExecuteXmlReader";
    private const Int32 ExecuteXmlReader_OperationsPerInvoke = 250;
    private const Int32 ExecuteXmlReader_EntitiesPerOperation = 100;

    [GlobalSetup(Targets = [nameof(ExecuteXmlReader_Manually), nameof(ExecuteXmlReader_SqlConnectionPlus)])]
    public void ExecuteXmlReader_Setup() =>
        this.SetEntitiesInDb(ExecuteXmlReader_EntitiesPerOperation);

    [Benchmark(Baseline = true, OperationsPerInvoke = ExecuteXmlReader_OperationsPerInvoke)]
    [BenchmarkCategory(ExecuteXmlReader_Category)]
    public List<XElement> ExecuteXmlReader_Manually()
    {
        using var connection = CreateConnection();

        var result = new List<XElement>();

        for (var i = 0; i < ExecuteXmlReader_OperationsPerInvoke; i++)
        {
            using var command = connection.CreateCommand();
            command.CommandText = """
                                  SELECT    *
                                  FROM      Entity
                                  FOR       XML AUTO, ROOT('Entities')
                                  """;

            using var xmlReader = command.ExecuteXmlReader();

            result = XDocument.Load(xmlReader).Root!.Elements().ToList();

            result
                .Should().HaveCount(ExecuteXmlReader_EntitiesPerOperation);
        }

        return result;
    }

    [Benchmark(Baseline = false, OperationsPerInvoke = ExecuteXmlReader_OperationsPerInvoke)]
    [BenchmarkCategory(ExecuteXmlReader_Category)]
    public List<XElement> ExecuteXmlReader_SqlConnectionPlus()
    {
        using var connection = CreateConnection();

        var result = new List<XElement>();

        for (var i = 0; i < ExecuteXmlReader_OperationsPerInvoke; i++)
        {
            result.Clear();

            using var xmlReader = connection.ExecuteXmlReader(
                $"""
                SELECT    *
                FROM      Entity
                FOR       XML AUTO, ROOT('Entities')
                """
            );

            result = XDocument.Load(xmlReader).Root!.Elements().ToList();

            result
                .Should().HaveCount(ExecuteXmlReader_EntitiesPerOperation);
        }

        return result;
    }
    #endregion

    #region Exists
    private const String Exists_Category = "Exists";
    private const Int32 Exists_OperationsPerInvoke = 5000;

    [GlobalSetup(Targets = [nameof(Exists_Manually), nameof(Exists_SqlConnectionPlus)])]
    public void Exists_Setup() =>
        this.SetEntitiesInDb(Exists_OperationsPerInvoke);

    [Benchmark(Baseline = true, OperationsPerInvoke = Exists_OperationsPerInvoke)]
    [BenchmarkCategory(Exists_Category)]
    public Boolean Exists_Manually()
    {
        using var connection = CreateConnection();

        var result = false;

        for (var i = 0; i < Exists_OperationsPerInvoke; i++)
        {
            var entityId = this.entitiesInDb[i].Id;

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1 FROM Entity WHERE Id = @Id";
            command.Parameters.Add(new("@Id", entityId));

            using var dataReader = command.ExecuteReader();

            result = dataReader.HasRows;

            result
                .Should().BeTrue();
        }

        return result;
    }

    [Benchmark(Baseline = false, OperationsPerInvoke = Exists_OperationsPerInvoke)]
    [BenchmarkCategory(Exists_Category)]
    public Boolean Exists_SqlConnectionPlus()
    {
        using var connection = CreateConnection();

        var result = false;

        for (var i = 0; i < Exists_OperationsPerInvoke; i++)
        {
            var entityId = this.entitiesInDb[i].Id;

            result = connection.Exists($"SELECT 1 FROM Entity WHERE Id = {Parameter(entityId)}");

            result
                .Should().BeTrue();
        }

        return result;
    }
    #endregion

    #region InsertEntities
    private const String InsertEntities_Category = "InsertEntities";
    private const Int32 InsertEntities_OperationsPerInvoke = 20;
    private const Int32 InsertEntities_EntitiesPerOperation = 100;

    [GlobalSetup(Targets = [nameof(InsertEntities_Manually), nameof(InsertEntities_SqlConnectionPlus)])]
    public void InsertEntities_Setup() =>
        this.SetEntitiesInDb(0);

    [Benchmark(Baseline = true, OperationsPerInvoke = InsertEntities_OperationsPerInvoke)]
    [BenchmarkCategory(InsertEntities_Category)]
    public void InsertEntities_Manually()
    {
        using var connection = CreateConnection();

        for (var i = 0; i < InsertEntities_OperationsPerInvoke; i++)
        {
            var entities = Generate.Entities(InsertEntities_EntitiesPerOperation);

            using var command = connection.CreateCommand();
            command.CommandText = """
                                  INSERT INTO [Entity]
                                  (
                                    [Id],
                                    [BooleanValue],
                                    [ByteValue],
                                    [CharValue],
                                    [DateTimeOffsetValue],
                                    [DateTimeValue],
                                    [DecimalValue],
                                    [DoubleValue],
                                    [EnumValue],
                                    [GuidValue],
                                    [Int16Value],
                                    [Int32Value],
                                    [Int64Value],
                                    [SingleValue],
                                    [StringValue],
                                    [TimeSpanValue]
                                  )
                                  VALUES
                                  (
                                    @Id,
                                    @BooleanValue,
                                    @ByteValue,
                                    @CharValue,
                                    @DateTimeOffsetValue,
                                    @DateTimeValue,
                                    @DecimalValue,
                                    @DoubleValue,
                                    @EnumValue,
                                    @GuidValue,
                                    @Int16Value,
                                    @Int32Value,
                                    @Int64Value,
                                    @SingleValue,
                                    @StringValue,
                                    @TimeSpanValue
                                  )
                                  """;

            var idParameter = new SqlParameter();
            idParameter.ParameterName = "@Id";

            var booleanValueParameter = new SqlParameter();
            booleanValueParameter.ParameterName = "@BooleanValue";

            var byteValueParameter = new SqlParameter();
            byteValueParameter.ParameterName = "@ByteValue";

            var charValueParameter = new SqlParameter();
            charValueParameter.ParameterName = "@CharValue";

            var dateTimeOffsetValueParameter = new SqlParameter();
            dateTimeOffsetValueParameter.ParameterName = "@DateTimeOffsetValue";

            var dateTimeValueParameter = new SqlParameter();
            dateTimeValueParameter.ParameterName = "@DateTimeValue";

            var decimalValueParameter = new SqlParameter();
            decimalValueParameter.ParameterName = "@DecimalValue";

            var doubleValueParameter = new SqlParameter();
            doubleValueParameter.ParameterName = "@DoubleValue";

            var enumValueParameter = new SqlParameter();
            enumValueParameter.ParameterName = "@EnumValue";

            var guidValueParameter = new SqlParameter();
            guidValueParameter.ParameterName = "@GuidValue";

            var int16ValueParameter = new SqlParameter();
            int16ValueParameter.ParameterName = "@Int16Value";

            var int32ValueParameter = new SqlParameter();
            int32ValueParameter.ParameterName = "@Int32Value";

            var int64ValueParameter = new SqlParameter();
            int64ValueParameter.ParameterName = "@Int64Value";

            var singleValueParameter = new SqlParameter();
            singleValueParameter.ParameterName = "@SingleValue";

            var stringValueParameter = new SqlParameter();
            stringValueParameter.ParameterName = "@StringValue";

            var timeSpanValueParameter = new SqlParameter();
            timeSpanValueParameter.ParameterName = "@TimeSpanValue";

            command.Parameters.Add(idParameter);
            command.Parameters.Add(booleanValueParameter);
            command.Parameters.Add(byteValueParameter);
            command.Parameters.Add(charValueParameter);
            command.Parameters.Add(dateTimeOffsetValueParameter);
            command.Parameters.Add(dateTimeValueParameter);
            command.Parameters.Add(decimalValueParameter);
            command.Parameters.Add(doubleValueParameter);
            command.Parameters.Add(enumValueParameter);
            command.Parameters.Add(guidValueParameter);
            command.Parameters.Add(int16ValueParameter);
            command.Parameters.Add(int32ValueParameter);
            command.Parameters.Add(int64ValueParameter);
            command.Parameters.Add(singleValueParameter);
            command.Parameters.Add(stringValueParameter);
            command.Parameters.Add(timeSpanValueParameter);

            foreach (var entity in entities)
            {
                idParameter.Value = entity.Id;
                booleanValueParameter.Value = entity.BooleanValue;
                byteValueParameter.Value = entity.ByteValue;
                charValueParameter.Value = entity.CharValue;
                dateTimeOffsetValueParameter.Value = entity.DateTimeOffsetValue;
                dateTimeValueParameter.Value = entity.DateTimeValue;
                decimalValueParameter.Value = entity.DecimalValue;
                doubleValueParameter.Value = entity.DoubleValue;
                enumValueParameter.Value = entity.EnumValue.ToString();
                guidValueParameter.Value = entity.GuidValue;
                int16ValueParameter.Value = entity.Int16Value;
                int32ValueParameter.Value = entity.Int32Value;
                int64ValueParameter.Value = entity.Int64Value;
                singleValueParameter.Value = entity.SingleValue;
                stringValueParameter.Value = entity.StringValue;
                timeSpanValueParameter.Value = entity.TimeSpanValue;

                command.ExecuteNonQuery()
                    .Should().Be(1);
            }
        }
    }

    [Benchmark(Baseline = false, OperationsPerInvoke = InsertEntities_OperationsPerInvoke)]
    [BenchmarkCategory(InsertEntities_Category)]
    public void InsertEntities_SqlConnectionPlus()
    {
        using var connection = CreateConnection();

        for (var i = 0; i < InsertEntities_OperationsPerInvoke; i++)
        {
            var entitiesToInsert = Generate.Entities(InsertEntities_EntitiesPerOperation);

            connection.InsertEntities(entitiesToInsert)
                .Should().Be(InsertEntities_EntitiesPerOperation);
        }
    }
    #endregion

    #region InsertEntity
    private const String InsertEntity_Category = "InsertEntity";
    private const Int32 InsertEntity_OperationsPerInvoke = 700;

    [GlobalSetup(Targets = [nameof(InsertEntity_Manually), nameof(InsertEntity_SqlConnectionPlus)])]
    public void InsertEntity_Setup() =>
        this.SetEntitiesInDb(0);

    [Benchmark(Baseline = true, OperationsPerInvoke = InsertEntity_OperationsPerInvoke)]
    [BenchmarkCategory(InsertEntity_Category)]
    public void InsertEntity_Manually()
    {
        using var connection = CreateConnection();

        for (var i = 0; i < InsertEntity_OperationsPerInvoke; i++)
        {
            var entity = Generate.Entity();

            using var command = connection.CreateCommand();
            command.CommandText = """
                                  INSERT INTO [Entity]
                                  (
                                    [Id],
                                    [BooleanValue],
                                    [ByteValue],
                                    [CharValue],
                                    [DateTimeOffsetValue],
                                    [DateTimeValue],
                                    [DecimalValue],
                                    [DoubleValue],
                                    [EnumValue],
                                    [GuidValue],
                                    [Int16Value],
                                    [Int32Value],
                                    [Int64Value],
                                    [SingleValue],
                                    [StringValue],
                                    [TimeSpanValue]
                                  )
                                  VALUES
                                  (
                                    @Id,
                                    @BooleanValue,
                                    @ByteValue,
                                    @CharValue,
                                    @DateTimeOffsetValue,
                                    @DateTimeValue,
                                    @DecimalValue,
                                    @DoubleValue,
                                    @EnumValue,
                                    @GuidValue,
                                    @Int16Value,
                                    @Int32Value,
                                    @Int64Value,
                                    @SingleValue,
                                    @StringValue,
                                    @TimeSpanValue
                                  )
                                  """;
            command.Parameters.Add(new("@Id", entity.Id));
            command.Parameters.Add(new("@BooleanValue", entity.BooleanValue));
            command.Parameters.Add(new("@ByteValue", entity.ByteValue));
            command.Parameters.Add(new("@CharValue", entity.CharValue));
            command.Parameters.Add(new("@DateTimeOffsetValue", entity.DateTimeOffsetValue));
            command.Parameters.Add(new("@DateTimeValue", entity.DateTimeValue));
            command.Parameters.Add(new("@DecimalValue", entity.DecimalValue));
            command.Parameters.Add(new("@DoubleValue", entity.DoubleValue));
            command.Parameters.Add(new("@EnumValue", entity.EnumValue.ToString()));
            command.Parameters.Add(new("@GuidValue", entity.GuidValue));
            command.Parameters.Add(new("@Int16Value", entity.Int16Value));
            command.Parameters.Add(new("@Int32Value", entity.Int32Value));
            command.Parameters.Add(new("@Int64Value", entity.Int64Value));
            command.Parameters.Add(new("@SingleValue", entity.SingleValue));
            command.Parameters.Add(new("@StringValue", entity.StringValue));
            command.Parameters.Add(new("@TimeSpanValue", entity.TimeSpanValue));

            command.ExecuteNonQuery();
        }
    }

    [Benchmark(Baseline = false, OperationsPerInvoke = InsertEntity_OperationsPerInvoke)]
    [BenchmarkCategory(InsertEntity_Category)]
    public void InsertEntity_SqlConnectionPlus()
    {
        using var connection = CreateConnection();

        for (var i = 0; i < InsertEntity_OperationsPerInvoke; i++)
        {
            var entity = Generate.Entity();

            connection.InsertEntity(entity);
        }
    }
    #endregion

    #region Parameter
    private const String Parameter_Category = "Parameter";
    private const Int32 Parameter_OperationsPerInvoke = 2500;

    [GlobalSetup(Targets = [nameof(Parameter_Manually), nameof(Parameter_SqlConnectionPlus)])]
    public void Parameter_Setup() =>
        this.SetEntitiesInDb(0);

    [Benchmark(Baseline = true, OperationsPerInvoke = Parameter_OperationsPerInvoke)]
    [BenchmarkCategory(Parameter_Category)]
    public Object Parameter_Manually()
    {
        using var connection = CreateConnection();

        var result = new List<Object>();

        for (var i = 0; i < Parameter_OperationsPerInvoke; i++)
        {
            result.Clear();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT @P1, @P2, @P3, @P4, @P5";
            command.Parameters.Add(new("@P1", 1));
            command.Parameters.Add(new("@P2", "Test"));
            command.Parameters.Add(new("@P3", DateTime.UtcNow));
            command.Parameters.Add(new("@P4", Guid.NewGuid()));
            command.Parameters.Add(new("@P5", true));

            using var dataReader = command.ExecuteReader();

            dataReader.Read();
            result.Add(dataReader.GetInt32(0));
            result.Add(dataReader.GetString(1));
            result.Add(dataReader.GetDateTime(2));
            result.Add(dataReader.GetGuid(3));
            result.Add(dataReader.GetBoolean(4));

            result
                .Should().HaveCount(5);
        }

        return result;
    }

    [Benchmark(Baseline = false, OperationsPerInvoke = Parameter_OperationsPerInvoke)]
    [BenchmarkCategory(Parameter_Category)]
    public Object Parameter_SqlConnectionPlus()
    {
        using var connection = CreateConnection();

        var result = new List<Object>();

        for (var i = 0; i < Parameter_OperationsPerInvoke; i++)
        {
            result.Clear();

            using var dataReader = connection.ExecuteReader(
                $"""
                 SELECT {Parameter(1)},
                        {Parameter("Test")},
                        {Parameter(DateTime.UtcNow)},
                        {Parameter(Guid.NewGuid())},
                        {Parameter(true)}
                 """);

            dataReader.Read();
            result.Add(dataReader.GetInt32(0));
            result.Add(dataReader.GetString(1));
            result.Add(dataReader.GetDateTime(2));
            result.Add(dataReader.GetGuid(3));
            result.Add(dataReader.GetBoolean(4));

            result
                .Should().HaveCount(5);
        }

        return result;
    }
    #endregion

    #region QueryEntities
    private const String QueryEntities_Category = "QueryEntities";
    private const Int32 QueryEntities_OperationsPerInvoke = 600;
    private const Int32 QueryEntities_EntitiesPerOperation = 100;

    [GlobalSetup(Targets = [nameof(QueryEntities_Manually), nameof(QueryEntities_SqlConnectionPlus)])]
    public void QueryEntities_Setup() =>
        this.SetEntitiesInDb(QueryEntities_EntitiesPerOperation);

    [Benchmark(Baseline = true, OperationsPerInvoke = QueryEntities_OperationsPerInvoke)]
    [BenchmarkCategory(QueryEntities_Category)]
    public List<Entity> QueryEntities_Manually()
    {
        using var connection = CreateConnection();

        var entities = new List<Entity>();

        for (var i = 0; i < QueryEntities_OperationsPerInvoke; i++)
        {
            entities.Clear();

            using var dataReader = connection.ExecuteReader(
                $"""
                 SELECT
                     TOP ({QueryEntities_EntitiesPerOperation})
                     [Id],
                     [BooleanValue],
                     [ByteValue],
                     [CharValue],
                     [DateTimeOffsetValue],
                     [DateTimeValue],
                     [DecimalValue],
                     [DoubleValue],
                     [EnumValue],
                     [GuidValue],
                     [Int16Value],
                     [Int32Value],
                     [Int64Value],
                     [SingleValue],
                     [StringValue],
                     [TimeSpanValue]
                 FROM
                     Entity
                 """
            );

            while (dataReader.Read())
            {
                var charBuffer = new Char[1];

                entities.Add(new()
                {
                    Id = dataReader.GetInt64(0),
                    BooleanValue = dataReader.GetBoolean(1),
                    ByteValue = dataReader.GetByte(2),
                    CharValue = dataReader.GetChars(3, 0, charBuffer, 0, 1) == 1 ? charBuffer[0] : throw new(),
                    DateTimeOffsetValue = (DateTimeOffset)dataReader.GetValue(4),
                    DateTimeValue = dataReader.GetDateTime(5),
                    DecimalValue = dataReader.GetDecimal(6),
                    DoubleValue = dataReader.GetDouble(7),
                    EnumValue = Enum.Parse<TestEnum>(dataReader.GetString(8)),
                    GuidValue = dataReader.GetGuid(9),
                    Int16Value = dataReader.GetInt16(10),
                    Int32Value = dataReader.GetInt32(11),
                    Int64Value = dataReader.GetInt64(12),
                    SingleValue = dataReader.GetFloat(13),
                    StringValue = dataReader.GetString(14),
                    TimeSpanValue = (TimeSpan)dataReader.GetValue(15)
                });
            }

            entities
                .Should().HaveCount(QueryEntities_EntitiesPerOperation);
        }

        return entities;
    }

    [Benchmark(Baseline = false, OperationsPerInvoke = QueryEntities_OperationsPerInvoke)]
    [BenchmarkCategory(QueryEntities_Category)]
    public List<Entity> QueryEntities_SqlConnectionPlus()
    {
        using var connection = CreateConnection();

        List<Entity> entities = [];

        for (var i = 0; i < QueryEntities_OperationsPerInvoke; i++)
        {
            entities = connection
                .QueryEntities<Entity>($"SELECT TOP ({QueryEntities_EntitiesPerOperation}) * FROM Entity")
                .ToList();

            entities
                .Should().HaveCount(QueryEntities_EntitiesPerOperation);
        }

        return entities;
    }
    #endregion

    #region QueryScalars
    private const String QueryScalars_Category = "QueryScalars";
    private const Int32 QueryScalars_OperationsPerInvoke = 1500;
    private const Int32 QueryScalars_EntitiesPerOperation = 100;

    [GlobalSetup(Targets = [nameof(QueryScalars_Manually), nameof(QueryScalars_SqlConnectionPlus)])]
    public void QueryScalars_Setup() =>
        this.SetEntitiesInDb(QueryScalars_EntitiesPerOperation);

    [Benchmark(Baseline = true, OperationsPerInvoke = QueryScalars_OperationsPerInvoke)]
    [BenchmarkCategory(QueryScalars_Category)]
    public List<Int64> QueryScalars_Manually()
    {
        using var connection = CreateConnection();

        var data = new List<Int64>();

        for (var i = 0; i < QueryScalars_OperationsPerInvoke; i++)
        {
            data.Clear();

            using var command = connection.CreateCommand();
            command.CommandText = $"SELECT TOP ({QueryScalars_EntitiesPerOperation}) Id FROM Entity";

            using var dataReader = command.ExecuteReader();

            while (dataReader.Read())
            {
                var id = dataReader.GetInt64(0);

                data.Add(id);
            }

            data
                .Should().HaveCount(QueryScalars_EntitiesPerOperation);
        }

        return data;
    }

    [Benchmark(Baseline = false, OperationsPerInvoke = QueryScalars_OperationsPerInvoke)]
    [BenchmarkCategory(QueryScalars_Category)]
    public List<Int64> QueryScalars_SqlConnectionPlus()
    {
        using var connection = CreateConnection();

        List<Int64> data = [];

        for (var i = 0; i < QueryScalars_OperationsPerInvoke; i++)
        {
            data = connection
                .QueryScalars<Int64>($"SELECT TOP ({QueryScalars_EntitiesPerOperation}) Id FROM Entity")
                .ToList();

            data
                .Should().HaveCount(QueryScalars_EntitiesPerOperation);
        }

        return data;
    }
    #endregion

    #region QueryTuples
    private const String QueryTuples_Category = "QueryTuples";
    private const Int32 QueryTuples_OperationsPerInvoke = 900;
    private const Int32 QueryTuples_EntitiesPerOperation = 100;

    [GlobalSetup(Targets = [nameof(QueryTuples_Manually), nameof(QueryTuples_SqlConnectionPlus)])]
    public void QueryTuples_Setup() =>
        this.SetEntitiesInDb(QueryTuples_EntitiesPerOperation);

    [Benchmark(Baseline = true, OperationsPerInvoke = QueryTuples_OperationsPerInvoke)]
    [BenchmarkCategory(QueryTuples_Category)]
    public List<(Int64 Id, DateTime DateTimeValue, TestEnum EnumValue, String StringValue)> QueryTuples_Manually()
    {
        using var connection = CreateConnection();

        var tuples = new List<(Int64 Id, DateTime DateTimeValue, TestEnum EnumValue, String StringValue)>();

        for (var i = 0; i < QueryTuples_OperationsPerInvoke; i++)
        {
            tuples.Clear();

            using var command = connection.CreateCommand();
            command.CommandText = $"""
                                   SELECT   TOP ({QueryTuples_EntitiesPerOperation})
                                            Id, DateTimeValue, EnumValue, StringValue
                                   FROM     Entity
                                   """;

            using var dataReader = command.ExecuteReader();

            while (dataReader.Read())
            {
                tuples.Add(
                    (
                        dataReader.GetInt64(0),
                        dataReader.GetDateTime(1),
                        Enum.Parse<TestEnum>(dataReader.GetString(2)),
                        dataReader.GetString(3)
                    )
                );
            }

            tuples
                .Should().HaveCount(QueryTuples_EntitiesPerOperation);
        }

        return tuples;
    }

    [Benchmark(Baseline = false, OperationsPerInvoke = QueryTuples_OperationsPerInvoke)]
    [BenchmarkCategory(QueryTuples_Category)]
    public List<(Int64 Id, DateTime DateTimeValue, TestEnum EnumValue, String StringValue)>
        QueryTuples_SqlConnectionPlus()
    {
        using var connection = CreateConnection();

        List<(Int64 Id, DateTime DateTimeValue, TestEnum EnumValue, String StringValue)> tuples = [];

        for (var i = 0; i < QueryTuples_OperationsPerInvoke; i++)
        {
            tuples = connection
                .QueryTuples<(Int64 Id, DateTime DateTimeValue, TestEnum EnumValue, String StringValue)>(
                    $"""
                     SELECT   TOP ({QueryTuples_EntitiesPerOperation})
                             Id, DateTimeValue, EnumValue, StringValue
                     FROM     Entity
                     """
                )
                .ToList();

            tuples
                .Should().HaveCount(QueryTuples_EntitiesPerOperation);
        }

        return tuples;
    }
    #endregion

    #region TemporaryTable_ComplexObjects
    private const String TemporaryTable_ComplexObjects_Category = "TemporaryTable_ComplexObjects";
    private const Int32 TemporaryTable_ComplexObjects_OperationsPerInvoke = 25;
    private const Int32 TemporaryTable_ComplexObjects_EntitiesPerOperation = 100;

    [GlobalSetup(Targets = [
        nameof(TemporaryTable_ComplexObjects_Manually),
        nameof(TemporaryTable_ComplexObjects_SqlConnectionPlus)
    ])]
    public void TemporaryTable_ComplexObjects_Setup() =>
        this.SetEntitiesInDb(0);

    [Benchmark(Baseline = true, OperationsPerInvoke = TemporaryTable_ComplexObjects_OperationsPerInvoke)]
    [BenchmarkCategory(TemporaryTable_ComplexObjects_Category)]
    public List<Entity> TemporaryTable_ComplexObjects_Manually()
    {
        using var connection = CreateConnection();

        var entities = Generate.Entities(TemporaryTable_ComplexObjects_EntitiesPerOperation);

        var result = new List<Entity>();

        for (var i = 0; i < TemporaryTable_ComplexObjects_OperationsPerInvoke; i++)
        {
            result.Clear();

            using var entitiesReader = new ObjectReader(
                typeof(Entity),
                entities,
                EntityHelper.GetEntityReadablePropertyNames(typeof(Entity))
            );

            using var getCollationCommand = connection.CreateCommand();
            getCollationCommand.CommandText =
                "SELECT CONVERT (VARCHAR(256), DATABASEPROPERTYEX(DB_NAME(), 'collation'))";
            var databaseCollation = (String)getCollationCommand.ExecuteScalar()!;

            using var createTableCommand = connection.CreateCommand();
            createTableCommand.CommandText =
                $"""
                 CREATE TABLE [#Entities] (
                    [BooleanValue] BIT,
                    [ByteValue] TINYINT,
                    [CharValue] CHAR(1),
                    [DateTimeOffsetValue] DATETIMEOFFSET,
                    [DateTimeValue] DATETIME2,
                    [DecimalValue] DECIMAL(28, 10),
                    [DoubleValue] FLOAT,
                    [EnumValue] NVARCHAR(200) COLLATE {databaseCollation},
                    [GuidValue] UNIQUEIDENTIFIER,
                    [Id] BIGINT,
                    [Int16Value] SMALLINT,
                    [Int32Value] INT,
                    [Int64Value] BIGINT,
                    [SingleValue] REAL,
                    [StringValue] NVARCHAR(MAX) COLLATE {databaseCollation},
                    [TimeSpanValue] TIME
                 )
                 """;
            createTableCommand.ExecuteNonQuery();

            using (var bulkCopy = new SqlBulkCopy(connection))
            {
                bulkCopy.DestinationTableName = "#Entities";
                bulkCopy.WriteToServer(entitiesReader);
            }

            using var selectCommand = connection.CreateCommand();
            selectCommand.CommandText =
                """
                SELECT
                    [Id],
                    [BooleanValue],
                    [ByteValue],
                    [CharValue],
                    [DateTimeOffsetValue],
                    [DateTimeValue],
                    [DecimalValue],
                    [DoubleValue],
                    [EnumValue],
                    [GuidValue],
                    [Int16Value],
                    [Int32Value],
                    [Int64Value],
                    [SingleValue],
                    [StringValue],
                    [TimeSpanValue]
                FROM
                    #Entities
                """;

            using var dataReader = selectCommand.ExecuteReader();

            while (dataReader.Read())
            {
                var charBuffer = new Char[1];

                result.Add(new()
                {
                    Id = dataReader.GetInt64(0),
                    BooleanValue = dataReader.GetBoolean(1),
                    ByteValue = dataReader.GetByte(2),
                    CharValue = dataReader.GetChars(3, 0, charBuffer, 0, 1) == 1 ? charBuffer[0] : throw new(),
                    DateTimeOffsetValue = (DateTimeOffset)dataReader.GetValue(4),
                    DateTimeValue = dataReader.GetDateTime(5),
                    DecimalValue = dataReader.GetDecimal(6),
                    DoubleValue = dataReader.GetDouble(7),
                    EnumValue = Enum.Parse<TestEnum>(dataReader.GetString(8)),
                    GuidValue = dataReader.GetGuid(9),
                    Int16Value = dataReader.GetInt16(10),
                    Int32Value = dataReader.GetInt32(11),
                    Int64Value = dataReader.GetInt64(12),
                    SingleValue = dataReader.GetFloat(13),
                    StringValue = dataReader.GetString(14),
                    TimeSpanValue = (TimeSpan)dataReader.GetValue(15)
                });
            }

            result
                .Should().HaveCount(TemporaryTable_ComplexObjects_EntitiesPerOperation);

            using var dropTableCommand = connection.CreateCommand();
            dropTableCommand.CommandText = "DROP TABLE #Entities";
            dropTableCommand.ExecuteNonQuery();
        }

        return result;
    }

    [Benchmark(Baseline = false, OperationsPerInvoke = TemporaryTable_ComplexObjects_OperationsPerInvoke)]
    [BenchmarkCategory(TemporaryTable_ComplexObjects_Category)]
    public List<Entity> TemporaryTable_ComplexObjects_SqlConnectionPlus()
    {
        using var connection = CreateConnection();

        var entities = Generate.Entities(TemporaryTable_ComplexObjects_EntitiesPerOperation);

        List<Entity> result = [];

        for (var i = 0; i < TemporaryTable_ComplexObjects_OperationsPerInvoke; i++)
        {
            result = connection.QueryEntities<Entity>($"SELECT * FROM {TemporaryTable(entities)}").ToList();

            result
                .Should().HaveCount(TemporaryTable_ComplexObjects_EntitiesPerOperation);
        }

        return result;
    }
    #endregion

    #region TemporaryTable_ScalarValues
    private const String TemporaryTable_ScalarValues_Category = "TemporaryTable_ScalarValues";
    private const Int32 TemporaryTable_ScalarValues_OperationsPerInvoke = 30;
    private const Int32 TemporaryTable_ScalarValues_ValuesPerOperation = 5000;

    [GlobalSetup(Targets = [
        nameof(TemporaryTable_ScalarValues_Manually),
        nameof(TemporaryTable_ScalarValues_SqlConnectionPlus)
    ])]
    public void TemporaryTable_ScalarValues_Setup() =>
        this.SetEntitiesInDb(0);

    [Benchmark(Baseline = true, OperationsPerInvoke = TemporaryTable_ScalarValues_OperationsPerInvoke)]
    [BenchmarkCategory(TemporaryTable_ScalarValues_Category)]
    public List<String> TemporaryTable_ScalarValues_Manually()
    {
        using var connection = CreateConnection();

        var scalarValues = Enumerable
            .Range(0, TemporaryTable_ScalarValues_ValuesPerOperation)
            .Select(a => a.ToString())
            .ToList();

        var result = new List<String>();

        for (var i = 0; i < TemporaryTable_ScalarValues_OperationsPerInvoke; i++)
        {
            result.Clear();

            using var valuesReader = new EnumerableReader(scalarValues, typeof(String), "Value");

            using var getCollationCommand = connection.CreateCommand();
            getCollationCommand.CommandText =
                "SELECT CONVERT (VARCHAR(256), DATABASEPROPERTYEX(DB_NAME(), 'collation'))";
            var databaseCollation = (String)getCollationCommand.ExecuteScalar()!;

            using var createTableCommand = connection.CreateCommand();
            createTableCommand.CommandText = $"CREATE TABLE #Values (Value NVARCHAR(4) COLLATE {databaseCollation})";
            createTableCommand.ExecuteNonQuery();

            using (var bulkCopy = new SqlBulkCopy(connection))
            {
                bulkCopy.DestinationTableName = "#Values";
                bulkCopy.WriteToServer(valuesReader);
            }

            using var selectCommand = connection.CreateCommand();
            selectCommand.CommandText = "SELECT Value FROM #Values";

            using var dataReader = selectCommand.ExecuteReader();

            while (dataReader.Read())
            {
                result.Add(dataReader.GetString(0));
            }

            result
                .Should().HaveCount(TemporaryTable_ScalarValues_ValuesPerOperation);

            using var dropTableCommand = connection.CreateCommand();
            dropTableCommand.CommandText = "DROP TABLE #Values";
            dropTableCommand.ExecuteNonQuery();
        }

        return result;
    }

    [Benchmark(Baseline = false, OperationsPerInvoke = TemporaryTable_ScalarValues_OperationsPerInvoke)]
    [BenchmarkCategory(TemporaryTable_ScalarValues_Category)]
    public List<String> TemporaryTable_ScalarValues_SqlConnectionPlus()
    {
        using var connection = CreateConnection();

        var scalarValues = Enumerable
            .Range(0, TemporaryTable_ScalarValues_ValuesPerOperation)
            .Select(a => a.ToString())
            .ToList();

        List<String> result = [];

        for (var i = 0; i < TemporaryTable_ScalarValues_OperationsPerInvoke; i++)
        {
            result = connection.QueryScalars<String>($"SELECT Value FROM {TemporaryTable(scalarValues)}").ToList();

            result
                .Should().HaveCount(TemporaryTable_ScalarValues_ValuesPerOperation);
        }

        return result;
    }
    #endregion

    #region UpdateEntities
    private const String UpdateEntities_Category = "UpdateEntities";
    private const Int32 UpdateEntities_OperationsPerInvoke = 10;
    private const Int32 UpdateEntities_EntitiesPerOperation = 100;

    [GlobalSetup(Targets = [
        nameof(UpdateEntities_Manually),
        nameof(UpdateEntities_SqlConnectionPlus)
    ])]
    public void UpdateEntities_Setup() =>
        this.SetEntitiesInDb(UpdateEntities_EntitiesPerOperation);

    [Benchmark(Baseline = true, OperationsPerInvoke = UpdateEntities_OperationsPerInvoke)]
    [BenchmarkCategory(UpdateEntities_Category)]
    public void UpdateEntities_Manually()
    {
        using var connection = CreateConnection();

        for (var i = 0; i < UpdateEntities_OperationsPerInvoke; i++)
        {
            var updatedEntities = Generate.Updates(this.entitiesInDb);

            using var command = connection.CreateCommand();
            command.CommandText = """
                                  UPDATE    [Entity]
                                  SET       [BooleanValue] = @BooleanValue,
                                            [ByteValue] = @ByteValue,
                                            [CharValue] = @CharValue,
                                            [DateTimeOffsetValue] = @DateTimeOffsetValue,
                                            [DateTimeValue] = @DateTimeValue,
                                            [DecimalValue] = @DecimalValue,
                                            [DoubleValue] = @DoubleValue,
                                            [EnumValue] = @EnumValue,
                                            [GuidValue] = @GuidValue,
                                            [Int16Value] = @Int16Value,
                                            [Int32Value] = @Int32Value,
                                            [Int64Value] = @Int64Value,
                                            [SingleValue] = @SingleValue,
                                            [StringValue] = @StringValue,
                                            [TimeSpanValue] = @TimeSpanValue
                                  WHERE     [Id] = @Id
                                  """;

            var idParameter = new SqlParameter();
            idParameter.ParameterName = "@Id";

            var booleanValueParameter = new SqlParameter();
            booleanValueParameter.ParameterName = "@BooleanValue";

            var byteValueParameter = new SqlParameter();
            byteValueParameter.ParameterName = "@ByteValue";

            var charValueParameter = new SqlParameter();
            charValueParameter.ParameterName = "@CharValue";

            var dateTimeOffsetValueParameter = new SqlParameter();
            dateTimeOffsetValueParameter.ParameterName = "@DateTimeOffsetValue";

            var dateTimeValueParameter = new SqlParameter();
            dateTimeValueParameter.ParameterName = "@DateTimeValue";

            var decimalValueParameter = new SqlParameter();
            decimalValueParameter.ParameterName = "@DecimalValue";

            var doubleValueParameter = new SqlParameter();
            doubleValueParameter.ParameterName = "@DoubleValue";

            var enumValueParameter = new SqlParameter();
            enumValueParameter.ParameterName = "@EnumValue";

            var guidValueParameter = new SqlParameter();
            guidValueParameter.ParameterName = "@GuidValue";

            var int16ValueParameter = new SqlParameter();
            int16ValueParameter.ParameterName = "@Int16Value";

            var int32ValueParameter = new SqlParameter();
            int32ValueParameter.ParameterName = "@Int32Value";

            var int64ValueParameter = new SqlParameter();
            int64ValueParameter.ParameterName = "@Int64Value";

            var singleValueParameter = new SqlParameter();
            singleValueParameter.ParameterName = "@SingleValue";

            var stringValueParameter = new SqlParameter();
            stringValueParameter.ParameterName = "@StringValue";

            var timeSpanValueParameter = new SqlParameter();
            timeSpanValueParameter.ParameterName = "@TimeSpanValue";

            command.Parameters.Add(idParameter);
            command.Parameters.Add(booleanValueParameter);
            command.Parameters.Add(byteValueParameter);
            command.Parameters.Add(charValueParameter);
            command.Parameters.Add(dateTimeOffsetValueParameter);
            command.Parameters.Add(dateTimeValueParameter);
            command.Parameters.Add(decimalValueParameter);
            command.Parameters.Add(doubleValueParameter);
            command.Parameters.Add(enumValueParameter);
            command.Parameters.Add(guidValueParameter);
            command.Parameters.Add(int16ValueParameter);
            command.Parameters.Add(int32ValueParameter);
            command.Parameters.Add(int64ValueParameter);
            command.Parameters.Add(singleValueParameter);
            command.Parameters.Add(stringValueParameter);
            command.Parameters.Add(timeSpanValueParameter);

            foreach (var updatedEntity in updatedEntities)
            {
                idParameter.Value = updatedEntity.Id;
                booleanValueParameter.Value = updatedEntity.BooleanValue;
                byteValueParameter.Value = updatedEntity.ByteValue;
                charValueParameter.Value = updatedEntity.CharValue;
                dateTimeOffsetValueParameter.Value = updatedEntity.DateTimeOffsetValue;
                dateTimeValueParameter.Value = updatedEntity.DateTimeValue;
                decimalValueParameter.Value = updatedEntity.DecimalValue;
                doubleValueParameter.Value = updatedEntity.DoubleValue;
                enumValueParameter.Value = updatedEntity.EnumValue.ToString();
                guidValueParameter.Value = updatedEntity.GuidValue;
                int16ValueParameter.Value = updatedEntity.Int16Value;
                int32ValueParameter.Value = updatedEntity.Int32Value;
                int64ValueParameter.Value = updatedEntity.Int64Value;
                singleValueParameter.Value = updatedEntity.SingleValue;
                stringValueParameter.Value = updatedEntity.StringValue;
                timeSpanValueParameter.Value = updatedEntity.TimeSpanValue;

                command.ExecuteNonQuery()
                    .Should().Be(1);
            }
        }
    }

    [Benchmark(Baseline = false, OperationsPerInvoke = UpdateEntities_OperationsPerInvoke)]
    [BenchmarkCategory(UpdateEntities_Category)]
    public void UpdateEntities_SqlConnectionPlus()
    {
        using var connection = CreateConnection();

        for (var i = 0; i < UpdateEntities_OperationsPerInvoke; i++)
        {
            var updatesEntities = Generate.Updates(this.entitiesInDb);

            connection.UpdateEntities(updatesEntities)
                .Should().Be(UpdateEntities_EntitiesPerOperation);
        }
    }
    #endregion

    #region UpdateEntity
    private const String UpdateEntity_Category = "UpdateEntity";
    private const Int32 UpdateEntity_OperationsPerInvoke = 700;

    [GlobalSetup(Targets = [
        nameof(UpdateEntity_Manually),
        nameof(UpdateEntity_SqlConnectionPlus)
    ])]
    public void UpdateEntity_Setup() =>
        this.SetEntitiesInDb(UpdateEntity_OperationsPerInvoke);

    [Benchmark(Baseline = true, OperationsPerInvoke = UpdateEntity_OperationsPerInvoke)]
    [BenchmarkCategory(UpdateEntity_Category)]
    public void UpdateEntity_Manually()
    {
        using var connection = CreateConnection();

        for (var i = 0; i < UpdateEntity_OperationsPerInvoke; i++)
        {
            var entity = this.entitiesInDb[i];

            var updatedEntity = Generate.Update(entity);

            using var command = connection.CreateCommand();
            command.CommandText = """
                                  UPDATE    [Entity]
                                  SET       [BooleanValue] = @BooleanValue,
                                            [ByteValue] = @ByteValue,
                                            [CharValue] = @CharValue,
                                            [DateTimeOffsetValue] = @DateTimeOffsetValue,
                                            [DateTimeValue] = @DateTimeValue,
                                            [DecimalValue] = @DecimalValue,
                                            [DoubleValue] = @DoubleValue,
                                            [EnumValue] = @EnumValue,
                                            [GuidValue] = @GuidValue,
                                            [Int16Value] = @Int16Value,
                                            [Int32Value] = @Int32Value,
                                            [Int64Value] = @Int64Value,
                                            [SingleValue] = @SingleValue,
                                            [StringValue] = @StringValue,
                                            [TimeSpanValue] = @TimeSpanValue
                                  WHERE     [Id] = @Id
                                  """;
            command.Parameters.Add(new("@Id", updatedEntity.Id));
            command.Parameters.Add(new("@BooleanValue", updatedEntity.BooleanValue));
            command.Parameters.Add(new("@ByteValue", updatedEntity.ByteValue));
            command.Parameters.Add(new("@CharValue", updatedEntity.CharValue));
            command.Parameters.Add(new("@DateTimeOffsetValue", updatedEntity.DateTimeOffsetValue));
            command.Parameters.Add(new("@DateTimeValue", updatedEntity.DateTimeValue));
            command.Parameters.Add(new("@DecimalValue", updatedEntity.DecimalValue));
            command.Parameters.Add(new("@DoubleValue", updatedEntity.DoubleValue));
            command.Parameters.Add(new("@EnumValue", updatedEntity.EnumValue.ToString()));
            command.Parameters.Add(new("@GuidValue", updatedEntity.GuidValue));
            command.Parameters.Add(new("@Int16Value", updatedEntity.Int16Value));
            command.Parameters.Add(new("@Int32Value", updatedEntity.Int32Value));
            command.Parameters.Add(new("@Int64Value", updatedEntity.Int64Value));
            command.Parameters.Add(new("@SingleValue", updatedEntity.SingleValue));
            command.Parameters.Add(new("@StringValue", updatedEntity.StringValue));
            command.Parameters.Add(new("@TimeSpanValue", updatedEntity.TimeSpanValue));

            command.ExecuteNonQuery()
                .Should().Be(1);
        }
    }

    [Benchmark(Baseline = false, OperationsPerInvoke = UpdateEntity_OperationsPerInvoke)]
    [BenchmarkCategory(UpdateEntity_Category)]
    public void UpdateEntity_SqlConnectionPlus()
    {
        using var connection = CreateConnection();

        for (var i = 0; i < UpdateEntity_OperationsPerInvoke; i++)
        {
            var entity = this.entitiesInDb[i];

            var updatedEntity = Generate.Update(entity);

            connection.UpdateEntity(updatedEntity)
                .Should().Be(1);
        }
    }
    #endregion
}
