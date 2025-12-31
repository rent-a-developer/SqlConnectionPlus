[![NuGet Version](https://img.shields.io/nuget/v/RentADeveloper.SqlConnectionPlus)](https://www.nuget.org/packages/RentADeveloper.SqlConnectionPlus/)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=rent-a-developer_SqlConnectionPlus&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=rent-a-developer_SqlConnectionPlus)
[![license](https://img.shields.io/badge/License-MIT-purple.svg)](LICENSE.md)
![semver](https://img.shields.io/badge/semver-1.0.0-blue)

# ![image icon](https://raw.githubusercontent.com/rent-a-developer/SqlConnectionPlus/main/icon32.png) SqlConnectionPlus
A focused set of extension methods for
[Microsoft.Data.SqlClient.SqlConnection](https://learn.microsoft.com/dotnet/api/microsoft.data.sqlclient.sqlconnection)
that reduce boilerplate code, boost productivity, and make working with SQL Server in C# more enjoyable.

Highlights:
- Parameterized interpolated-string support
- On-the-fly temporary tables from in-memory collections
- Entity mapping helpers (insert, update, delete, query)
- Designed to be used in synchronous and asynchronous code paths

## Table of contents
- [ SqlConnectionPlus](#-sqlconnectionplus)
  - [Table of contents](#table-of-contents)
  - [Installation](#installation)
  - [Quick start](#quick-start)
  - [Examples](#examples)
    - [Parameters via interpolated strings](#parameters-via-interpolated-strings)
    - [On-the-fly temporary tables via interpolated strings](#on-the-fly-temporary-tables-via-interpolated-strings)
    - [Enum support](#enum-support)
  - [API summary](#api-summary)
    - [Attributes](#attributes)
      - [`System.ComponentModel.DataAnnotations.KeyAttribute`](#systemcomponentmodeldataannotationskeyattribute)
      - [`System.ComponentModel.DataAnnotations.Schema.TableAttribute`](#systemcomponentmodeldataannotationsschematableattribute)
      - [`System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute`](#systemcomponentmodeldataannotationsschemanotmappedattribute)
    - [Configuration](#configuration)
      - [EnumSerializationMode](#enumserializationmode)
    - [General-purpose methods](#general-purpose-methods)
      - [ExecuteNonQuery / ExecuteNonQueryAsync](#executenonquery--executenonqueryasync)
      - [ExecuteReader / ExecuteReaderAsync](#executereader--executereaderasync)
      - [ExecuteScalar / ExecuteScalarAsync](#executescalar--executescalarasync)
      - [ExecuteXmlReader / ExecuteXmlReaderAsync](#executexmlreader--executexmlreaderasync)
      - [Exists / ExistsAsync](#exists--existsasync)
      - [QueryEntities / QueryEntitiesAsync](#queryentities--queryentitiesasync)
      - [QueryScalars / QueryScalarsAsync](#queryscalars--queryscalarsasync)
      - [QueryTuples / QueryTuplesAsync](#querytuples--querytuplesasync)
    - [Entity manipulation methods](#entity-manipulation-methods)
      - [InsertEntities / InsertEntitiesAsync](#insertentities--insertentitiesasync)
      - [InsertEntity / InsertEntityAsync](#insertentity--insertentityasync)
      - [UpdateEntities / UpdateEntitiesAsync](#updateentities--updateentitiesasync)
      - [UpdateEntity / UpdateEntityAsync](#updateentity--updateentityasync)
      - [DeleteEntities / DeleteEntitiesAsync](#deleteentities--deleteentitiesasync)
      - [DeleteEntity / DeleteEntityAsync](#deleteentity--deleteentityasync)
    - [Special helpers](#special-helpers)
      - [Parameter(value)](#parametervalue)
      - [TemporaryTable(values)](#temporarytablevalues)
  - [Benchmarks](#benchmarks)
    - [Running the benchmarks](#running-the-benchmarks)
  - [Running the unit tests](#running-the-unit-tests)
  - [Running the integration tests](#running-the-integration-tests)
  - [Contributing](#contributing)
  - [License](#license)
  - [Documentation](#documentation)
  - [Change Log](#change-log)
  - [Contributors](#contributors)

## Installation
First, [install NuGet](http://docs.nuget.org/docs/start-here/installing-nuget).

Then install the [NuGet package](https://www.nuget.org/packages/RentADeveloper.SqlConnectionPlus/) from the package
manager console:
~~~shell
PM> Install-Package RentADeveloper.SqlConnectionPlus
~~~

## Quick start
Import the library and the static helpers:

~~~csharp
using Microsoft.Data.SqlClient;
using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;
~~~

Open or reuse a `SqlConnection` and use the extension methods:

~~~csharp
using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;

class Product
{
    [Key]
    public Int64 Id { get; set; }
    public Int32 UnitsInStock { get; set; }
}

class OrderItem
{
    [Key]
    public Int64 Id { get; set; }
    public Int64 ProductId { get; set; }
    public DateTime OrderDate { get; set; }
}

var lowStockThreshold = configuration.Thresholds.LowStock;

var lowStockProductsReader = connection.ExecuteReader(
   $"""
    SELECT  *
    FROM    Product
    WHERE   UnitsInStock < {Parameter(lowStockThreshold)}
    """
);

...

var orderItems = GetOrderItems();
var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);

var productsOrderedInPastSixMonthsReader = connection.ExecuteReader(
    $"""
     SELECT     *
     FROM       Product
     WHERE      EXISTS (
                    SELECT  1
                    FROM    {TemporaryTable(orderItems)} TOrderItem
                    WHERE   TOrderItem.ProductId = Product.Id AND
                            TOrderItem.OrderDate >= {Parameter(sixMonthsAgo)}
                )
     """
);
~~~

## Examples

### Parameters via interpolated strings
All extension methods accept interpolated strings where parameter values are captured via
[Parameter(value)](#parametervalue):

~~~csharp
using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;

var lowStockThreshold = configuration.Thresholds.LowStock;

var lowStockProductInfos = connection.QueryTuples<(Int64 ProductId, Int32 UnitsInStock)>(
   $"""
    SELECT  Id, UnitsInStock
    FROM    Product
    WHERE   UnitsInStock < {Parameter(lowStockThreshold)}
    """
);
~~~

This prevents SQL injection and keeps the SQL readable.

### On-the-fly temporary tables via interpolated strings
Create a temporary table on the fly from an `IEnumerable<T>` and use it in statements via
[TemporaryTable(values)](#temporarytablevalues):

~~~csharp
using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;

var retiredSupplierIds = suppliers.Where(a => a.IsRetired).Select(a => a.Id);

var retiredSupplierProducts = connection.QueryEntities<Product>(
   $"""
    SELECT  *
    FROM    Product
    WHERE   SupplierId IN (
                SELECT  Value
                FROM    {TemporaryTable(retiredSupplierIds)}
            )
    """
);
~~~

Complex objects are also supported - the library creates a temporary table with appropriate columns and types:

~~~csharp
using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;

class OrderItem
{
    public Int64 ProductId { get; set; }
    public DateTime OrderDate { get; set; }
}

var orderItems = GetOrderItems();
var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);

var productsOrderedInPastSixMonths = connection.QueryEntities<Product>(
    $"""
     SELECT     *
     FROM       Product
     WHERE      EXISTS (
                    SELECT  1
                    FROM    {TemporaryTable(orderItems)} TOrderItem
                    WHERE   TOrderItem.ProductId = Product.Id AND
                            TOrderItem.OrderDate >= {Parameter(sixMonthsAgo)}
                )
     """
);
~~~

### Enum support
Enum values are either mapped to their string representation or to integers when sent to the database:

When `SqlConnectionExtensions.EnumSerializationMode` is set to `EnumSerializationMode.String`, enums are stored as
strings:

~~~sql
CREATE TABLE Users
(
    Id BIGINT,
    UserName NVARCHAR(255),
    Role NVARCHAR(200)
)
~~~

~~~csharp
using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;

enum UserRole
{ 
  Admin = 1,
  User = 2,
  Guest = 3 
}

class User
{
    [Key]
    public Int64 Id { get; set; }
    public String UserName { get; set; }
    public UserRole Role { get; set; }
}

var user = new User
{
    Id = 1,
    UserName = "adminuser",
    Role = UserRole.User
};

connection.InsertEntity(user); // Column "Role" will contain the string "User".
~~~

When `SqlConnectionExtensions.EnumSerializationMode` is set to `EnumSerializationMode.Integer`, enums are stored as
integers:

~~~sql
CREATE TABLE Users
(
    Id BIGINT,
    UserName NVARCHAR(255),
    Role INT
)
~~~

~~~csharp
using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;

enum UserRole
{ 
  Admin = 1,
  User = 2,
  Guest = 3
}

class User
{
    [Key]
    public Int64 Id { get; set; }
    public String UserName { get; set; }
    public UserRole Role { get; set; }
}

var user = new User
{
    Id = 1,
    UserName = "adminuser",
    Role = UserRole.User
};

connection.InsertEntity(user); // Column "Role" will contain the integer 2.
~~~

When reading data from the database, this library automatically maps string and integer values back to the
corresponding enum values.

## API summary

Attributes:
- [`System.ComponentModel.DataAnnotations.KeyAttribute`](#systemcomponentmodeldataannotationskeyattribute) 
Specify the key property of an entity type
- [`System.ComponentModel.DataAnnotations.Schema.TableAttribute`](#systemcomponentmodeldataannotationsschematableattribute) 
Specify the name of the table where entities of an entity type are stored
- [`System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute`](#systemcomponentmodeldataannotationsschemanotmappedattribute)
Specify that a property of an entity type is not mapped to a column in the database

Configuration:
- [EnumSerializationMode](#enumserializationmode) - Configure how enum values are serialized when sent to the database

General-purpose methods:
- [ExecuteNonQuery / ExecuteNonQueryAsync](#executenonquery--executenonqueryasync) - Execute a non-query and return 
number of affected rows
- [ExecuteReader / ExecuteReaderAsync](#executereader--executereaderasync) - Execute a query and return `DbDataReader` 
to read the results
- [ExecuteScalar / ExecuteScalarAsync](#executescalar--executescalarasync) - Read a single value
- [ExecuteXmlReader / ExecuteXmlReaderAsync](#executexmlreader--executexmlreaderasync) - Execute query and return 
`XmlReader` to read the results as XML
- [Exists / ExistsAsync](#exists--existsasync) - Check for existence of rows
- [QueryEntities / QueryEntitiesAsync](#queryentities--queryentitiesasync) - Map result set to entities
- [QueryScalars / QueryScalarsAsync](#queryscalars--queryscalarsasync) - Read first-column values
- [QueryTuples / QueryTuplesAsync](#querytuples--querytuplesasync) - Read column values as `ValueTuple`s.

Entity manipulation methods:
- [InsertEntities / InsertEntitiesAsync](#insertentities--insertentitiesasync) - Insert a sequence of new entities
- [InsertEntity / InsertEntityAsync](#insertentity--insertentityasync) - Insert a new entity
- [UpdateEntities / UpdateEntitiesAsync](#updateentities--updateentitiesasync) - Update existing entities by keys
- [UpdateEntity / UpdateEntityAsync](#updateentity--updateentityasync) - Update an existing entity by key
- [DeleteEntities / DeleteEntitiesAsync](#deleteentities--deleteentitiesasync) - Delete existing entities by keys
- [DeleteEntity / DeleteEntityAsync](#deleteentity--deleteentityasync) - Delete an existing entity by key

Special helpers:
- [Parameter(value)](#parametervalue) - Create a parameter for an SQL statement from an interpolated value
- [TemporaryTable(values)](#temporarytablevalues) - Create a temporary table from a sequence of values and reference 
it inside an SQL statement

### Attributes
This library uses the following attributes:

#### `System.ComponentModel.DataAnnotations.KeyAttribute`
Use this attribute to specify the property of an entity type by which entities of that type are identified 
(usually the primary key):
~~~csharp
class Product
{
  [Key]
  public Int64 Id { get; set; }
}
~~~

This attribute must be used for entities passed to the following methods:
- [InsertEntities / InsertEntitiesAsync](#insertentities--insertentitiesasync)
- [InsertEntity / InsertEntityAsync](#insertentity--insertentityasync)
- [UpdateEntities / UpdateEntitiesAsync](#updateentities--updateentitiesasync)
- [UpdateEntity / UpdateEntityAsync](#updateentity--updateentityasync)
- [DeleteEntities / DeleteEntitiesAsync](#deleteentities--deleteentitiesasync)
- [DeleteEntity / DeleteEntityAsync](#deleteentity--deleteentityasync)
 
#### `System.ComponentModel.DataAnnotations.Schema.TableAttribute`
Use this attribute to specify the name of the table where entities of an entity type are stored in the database:
~~~csharp
[Table("Products")]
public class Product { ... }
~~~
If you don't specify the table name using this attribute, the singular name of the entity type 
(not including its namespace) is used as the table name.
 
#### `System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute`
Use this attribute to specify that a property of an entity type should be ignored and not mapped to a column:
~~~csharp
public class OrderItem
{
    [NotMapped]
    public Decimal TotalPrice => this.UnitPrice * this.Quantity;
}
~~~
Properties marked with this attribute are ignored by the following methods:
- [InsertEntities / InsertEntitiesAsync](#insertentities--insertentitiesasync)
- [InsertEntity / InsertEntityAsync](#insertentity--insertentityasync)
- [UpdateEntities / UpdateEntitiesAsync](#updateentities--updateentitiesasync)
- [UpdateEntity / UpdateEntityAsync](#updateentity--updateentityasync)
- [TemporaryTable(values)](#temporarytablevalues)

### Configuration
#### EnumSerializationMode
Use `SqlConnectionExtensions.EnumSerializationMode` to configure how enum values are serialized when they are sent to a 
database.  
The default value is `EnumSerializationMode.Strings`, which serializes enum values as their string representation.

~~~csharp
using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;

enum UserRole
{ 
  Admin = 1,
  User = 2,
  Guest = 3 
}

class User
{
    [Key]
    public Int64 Id { get; set; }
    public String UserName { get; set; }
    public UserRole Role { get; set; }
}

var user = new User
{
    Id = 1,
    UserName = "adminuser",
    Role = UserRole.User
};

SqlConnectionExtensions.EnumSerializationMode = EnumSerializationMode.Strings;
connection.InsertEntity(user); // Column "Role" will contain the string "User".

SqlConnectionExtensions.EnumSerializationMode = EnumSerializationMode.Integers;
connection.InsertEntity(user); // Column "Role" will contain the integer 2.
~~~

When `SqlConnectionExtensions.EnumSerializationMode` is set to `EnumSerializationMode.Strings`, enum values are
serialized as strings.  
When `SqlConnectionExtensions.EnumSerializationMode` is set to `EnumSerializationMode.Integers`, enum values are
serialized as integers.

### General-purpose methods

#### ExecuteNonQuery / ExecuteNonQueryAsync
Executes an SQL statement and returns the number of rows affected by the statement.

~~~csharp
using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;

if (supplier.IsRetired)
{
    var numberOfDeletedProducts = connection.ExecuteNonQuery(
       $"""
        DELETE FROM Product
        WHERE       SupplierId = {Parameter(supplier.Id)}
        """
    );
}
~~~

#### ExecuteReader / ExecuteReaderAsync
Executes an SQL statement and returns a `DbDataReader` to read the results.

~~~csharp
using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;

var lowStockThreshold = configuration.Thresholds.LowStock;

var lowStockProductsReader = connection.ExecuteReader(
   $"""
    SELECT  *
    FROM    Product
    WHERE   UnitsInStock < {Parameter(lowStockThreshold)}
    """
);
~~~

#### ExecuteScalar / ExecuteScalarAsync
Executes an SQL statement and returns the value of the first column of the first row in the result set converted to 
the specified type.
~~~csharp
using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;

var lowStockThreshold = configuration.Thresholds.LowStock;

var numberOfLowStockProducts = connection.ExecuteScalar<Int32>(
   $"""
    SELECT  COUNT(*)
    FROM    Product
    WHERE   UnitsInStock < {Parameter(lowStockThreshold)}
    """
);
~~~

#### ExecuteXmlReader / ExecuteXmlReaderAsync
Executes an SQL statement and returns an `XmlReader` to read the results as XML.
~~~csharp
using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;

var lowStockThreshold = configuration.Thresholds.LowStock;

var lowStockProductsXmlReader = connection.ExecuteXmlReader(
   $"""
    SELECT  *
    FROM    Product
    WHERE   UnitsInStock < {Parameter(lowStockThreshold)}
    FOR     XML AUTO
    """
);
~~~

#### Exists / ExistsAsync
Checks if any rows exist that match the specified SQL statement.
~~~csharp
using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;

var lowStockThreshold = configuration.Thresholds.LowStock;

var existLowStockProducts = connection.Exists(
   $"""
    SELECT  1
    FROM    Product
    WHERE   UnitsInStock < {Parameter(lowStockThreshold)}
    """
);
~~~

#### QueryEntities / QueryEntitiesAsync
Executes an SQL statement and maps the result set to a sequence of entities of the specified type.
~~~csharp
using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;

var lowStockThreshold = configuration.Thresholds.LowStock;

var lowStockProducts = connection.QueryEntities<Product>(
   $"""
    SELECT  *
    FROM    Product
    WHERE   UnitsInStock < {Parameter(lowStockThreshold)}
    """
);
~~~

#### QueryScalars / QueryScalarsAsync
Executes an SQL statement and returns the values of the first column of the result as a sequence of values converted 
to the specified type.
~~~csharp
using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;

class OrderItem
{
    public Int64 ProductId { get; set; }
    public DateTime OrderDate { get; set; }
}

var orderItems = GetOrderItems();
var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);

var idsOfProductsOrderedInPastSixMonths = connection.QueryScalars<Int64>(
    $"""
     SELECT     Id
     FROM       Product
     WHERE      EXISTS (
                    SELECT  1
                    FROM    {TemporaryTable(orderItems)} TOrderItem
                    WHERE   TOrderItem.ProductId = Product.Id AND
                            TOrderItem.OrderDate >= {Parameter(sixMonthsAgo)}
                )
     """
);
~~~

#### QueryTuples / QueryTuplesAsync
Executes an SQL statement and returns the values of the columns of the result as a sequence of
[ValueTuples](https://learn.microsoft.com/dotnet/api/system.valuetuple).
~~~csharp
using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;

class OrderItem
{
    public Int64 ProductId { get; set; }
    public DateTime OrderDate { get; set; }
}

var orderItems = GetOrderItems();
var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);

var productsOrderedInPastSixMonthsInfos = connection.QueryTuples<(Int64 ProductId, Int32 UnitsInStock)>(
    $"""
     SELECT     Id, UnitsInStock
     FROM       Product
     WHERE      EXISTS (
                    SELECT  1
                    FROM    {TemporaryTable(orderItems)} TOrderItem
                    WHERE   TOrderItem.ProductId = Product.Id AND
                            TOrderItem.OrderDate >= {Parameter(sixMonthsAgo)}
                )
     """
);
~~~

### Entity manipulation methods

#### InsertEntities / InsertEntitiesAsync
Inserts a sequence of new entities into an SQL Server table.
~~~csharp
using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;

class Product
{
    [Key]
    public Int64 Id { get; set; }
    public Int64 SupplierId { get; set; }
    public String Name { get; set; }
    public Decimal UnitPrice { get; set; }
    public Int32 UnitsInStock { get; set; }
}

var newProducts = GetNewProducts();

connection.InsertEntities(newProducts);
~~~

#### InsertEntity / InsertEntityAsync
Inserts a new entity into an SQL Server table.
~~~csharp
using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;

class Product
{
    [Key]
    public Int64 Id { get; set; }
    public Int64 SupplierId { get; set; }
    public String Name { get; set; }
    public Decimal UnitPrice { get; set; }
    public Int32 UnitsInStock { get; set; }
}

var newProduct = GetNewProduct();

connection.InsertEntity(newProduct);
~~~

#### UpdateEntities / UpdateEntitiesAsync
Updates existing entities in an SQL Server table based on their keys.
~~~csharp
using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;

enum UserState
{
	Active,
	Inactive,
	Suspended
}
			
class User
{
    [Key]
    public Int64 Id { get; set; }
    public DateTime LastLoginDate { get; set; }
    public UserState State { get; set; }
}

var usersWithoutLoginInPastYear = connection.QueryEntities<User>(
    """
    SELECT  *
    FROM    Users
    WHERE   LastLoginDate < DATEADD(YEAR, -1, GETUTCDATE())
    """
).ToList();

foreach (var user in usersWithoutLoginInPastYear)
{
    user.State = UserState.Inactive;
}

connection.UpdateEntities(usersWithoutLoginInPastYear);
~~~

#### UpdateEntity / UpdateEntityAsync
Updates an existing entity in an SQL Server table based on its key.
~~~csharp
using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;

enum UserState
{
	Active,
	Inactive,
	Suspended
}
			
class User
{
    [Key]
    public Int64 Id { get; set; }
    public DateTime LastLoginDate { get; set; }
    public UserState State { get; set; }
}

if (user.LastLoginDate < DateTime.UtcNow.AddYears(-1))
{
    user.State = UserState.Inactive;
    connection.UpdateEntity(user);
}
~~~

#### DeleteEntities / DeleteEntitiesAsync
Deletes a sequence of entities from an SQL Server table based on their keys.
~~~csharp
using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;

class Product
{
    [Key]
    public Int64 Id { get; set; }
    public Boolean IsDiscontinued { get; set; }
}

connection.DeleteEntities(products.Where(a => a.IsDiscontinued));
~~~

#### DeleteEntity / DeleteEntityAsync
Deletes an entity from an SQL Server table based on its key.
~~~csharp
using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;

class Product
{
    [Key]
    public Int64 Id { get; set; }
    public Boolean IsDiscontinued { get; set; }
}

if (product.IsDiscontinued)
{
    connection.DeleteEntity(product);
}
~~~

### Special helpers

#### Parameter(value)
Use `Parameter(value)` to pass a value in an interpolated string as a parameter to an SQL Server statement.

To use this method, first import `RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions` with a using directive
with the static modifier:
~~~csharp
using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;
~~~

Example:
~~~csharp
using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;

var lowStockThreshold = configuration.Thresholds.LowStock;

var lowStockProductsReader = connection.ExecuteReader(
   $"""
    SELECT  *
    FROM    Product
    WHERE   UnitsInStock < {Parameter(lowStockThreshold)}
    """
);
~~~
This will add a parameter with the name `@LowStockThreshold` and the value of the variable `lowStockThreshold` to the
 SQL statement.

The name of the parameter will be inferred from the expression passed to `Parameter(value)`.
If the name cannot be inferred from the expression a generic name like `@Parameter_1`, `@Parameter_2`, and so on will
be used.

The expression `{Parameter(value)}` will be replaced with the name of the parameter (e.g. `@LowStockThreshold`) in 
the SQL statement.

If you pass an enum value as a parameter, the enum value is serialized either as a string or as an integer according
to the setting `RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions.EnumSerializationMode`.

#### TemporaryTable(values)
Use `TemporaryTable(values)` to pass a sequence of scalar values or complex objects in an interpolated string as a
temporary table to an SQL statement.

To use this method, first import `RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions` with a using directive 
with the static modifier:
~~~csharp
using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;
~~~

You can pass a sequence of scalar values (e.g. `String`, `Int32`, `DateTime`, enums and so on) or a sequence of
complex objects.

If a sequence of scalar values is passed, the temporary table will have a single column named `Value` with a data
type that matches the type of the passed values.

Example:
~~~csharp
using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;

var retiredSupplierIds = suppliers.Where(a => a.IsRetired).Select(a => a.Id);

var retiredSupplierProductsReader = connection.ExecuteReader(
   $"""
    SELECT  *
    FROM    Product
    WHERE   SupplierId IN (
                SELECT  Value
                FROM    {TemporaryTable(retiredSupplierIds)}
            )
    """
);
~~~
This will create a temporary table with a single column named `Value` and with a data type that matches the type of
the passed values:
~~~sql
CREATE TABLE #RetiredSupplierIds_48d42afd5d824a27bd9352676ab6c198
(
    Value BIGINT
)
~~~

If a sequence of complex objects is passed, the temporary table will have multiple columns.
The temporary table will contain a column for each public property of the passed objects.
The name of each column will be the name of the corresponding property.
The data type of each column will be the property type of the corresponding property.

Example:
~~~csharp
using static RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions;

class OrderItem
{
    public Int64 ProductId { get; set; }
    public DateTime OrderDate { get; set; }
}

var orderItems = GetOrderItems();
var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);

var productsOrderedInPastSixMonthsReader = connection.ExecuteReader(
    $"""
     SELECT     *
     FROM       Product
     WHERE      EXISTS (
                    SELECT  1
                    FROM    {TemporaryTable(orderItems)} TOrderItem
                    WHERE   TOrderItem.ProductId = Product.Id AND
                            TOrderItem.OrderDate >= {Parameter(sixMonthsAgo)}
                )
     """
);
~~~
This will create a temporary table with columns matching the properties of the passed objects:
~~~sql
CREATE TABLE #OrderItems_d6545835d97148ab93709efe9ba1f110
(
    ProductId BIGINT,
    OrderDate DATETIME2
)
~~~

The name of the temporary table will be inferred from the expression passed to `TemporaryTable(values)` and suffixed
with a new Guid to avoid naming conflicts (e.g. `#OrderItems_395c98f203514e81aa0098ec7f13e8a2`).
If the name cannot be inferred from the expression the name `#Values` (also suffixed with a new Guid) will be used
(e.g. `#Values_395c98f203514e81aa0098ec7f13e8a2`).

The expression `{TemporaryTable(values)}` will be replaced with the name of the temporary table 
(e.g. `#OrderItems_395c98f203514e81aa0098ec7f13e8a2`) in the SQL statement.

If you pass enum values as a temporary table, the enum values are serialized either as strings or as integers
according to the setting `RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions.EnumSerializationMode`.

If you pass objects containing enum properties as a temporary table, the enum values are serialized either as strings
or as integers according to the setting
`RentADeveloper.SqlConnectionPlus.SqlConnectionExtensions.EnumSerializationMode`.

When `SqlConnectionExtensions.EnumSerializationMode` is set to `EnumSerializationMode.Strings`, the data type of the
corresponding column in the temporary table will be `NVARCHAR(200)`.

When `SqlConnectionExtensions.EnumSerializationMode` is set to `EnumSerializationMode.Integers`, the data type of the
corresponding column in the temporary table will be `INT`.

## Benchmarks
SqlConnectionPlus is designed to have a minimal performance and allocation overhead compared to using 
`SqlCommand` manually.  

```
BenchmarkDotNet v0.15.8, Windows 11 (10.0.26100.7462/24H2/2024Update/HudsonValley)
12th Gen Intel Core i9-12900K 3.19GHz, 1 CPU, 24 logical and 16 physical cores
.NET SDK 10.0.101
  [Host]     : .NET 8.0.22 (8.0.22, 8.0.2225.52707), X64 RyuJIT x86-64-v3
  Job-ADQEJE : .NET 8.0.22 (8.0.22, 8.0.2225.52707), X64 RyuJIT x86-64-v3

MinIterationTime=100ms  OutlierMode=DontRemove  Server=True  
InvocationCount=1  MaxIterationCount=20  UnrollFactor=1  
WarmupCount=10  
```

| Method                                          | Mean         | Error        | StdDev       | Median       | P90          | P95          | Ratio        | RatioSD | Allocated | Alloc Ratio |
|------------------------------------------------ |-------------:|-------------:|-------------:|-------------:|-------------:|-------------:|-------------:|--------:|----------:|------------:|
| **DeleteEntities_Manually**                         | **16,906.22 μs** |   **806.575 μs** |   **928.853 μs** | **16,622.62 μs** | **18,271.09 μs** | **18,413.06 μs** |     **baseline** |        **** | **101.71 KB** |            **** |
| DeleteEntities_SqlConnectionPlus                |  7,689.13 μs | 1,303.497 μs | 1,501.109 μs |  7,396.23 μs | 10,175.80 μs | 10,259.00 μs | 2.27x faster |   0.40x |   20.4 KB |  4.99x less |
|                                                 |              |              |              |              |              |              |              |         |           |             |
| **DeleteEntity_Manually**                           |    **139.55 μs** |    **11.709 μs** |    **13.484 μs** |    **136.37 μs** |    **146.17 μs** |    **158.65 μs** |     **baseline** |        **** |   **2.11 KB** |            **** |
| DeleteEntity_SqlConnectionPlus                  |    144.32 μs |     4.516 μs |     5.201 μs |    143.38 μs |    149.07 μs |    150.24 μs | 1.04x slower |   0.09x |   2.11 KB |  1.00x more |
|                                                 |              |              |              |              |              |              |              |         |           |             |
| **ExecuteNonQuery_Manually**                        |    **181.82 μs** |     **6.700 μs** |     **7.715 μs** |    **180.21 μs** |    **190.34 μs** |    **200.92 μs** |     **baseline** |        **** |   **2.11 KB** |            **** |
| ExecuteNonQuery_SqlConnectionPlus               |    147.33 μs |    34.629 μs |    39.879 μs |    138.12 μs |    174.63 μs |    216.30 μs | 1.30x faster |   0.28x |   2.93 KB |  1.39x more |
|                                                 |              |              |              |              |              |              |              |         |           |             |
| **ExecuteReader_Manually**                          |    **172.77 μs** |     **9.833 μs** |    **11.323 μs** |    **168.20 μs** |    **197.12 μs** |    **197.29 μs** |     **baseline** |        **** |  **44.08 KB** |            **** |
| ExecuteReader_SqlConnectionPlus                 |    188.65 μs |    30.072 μs |    34.631 μs |    175.11 μs |    214.73 μs |    247.76 μs | 1.10x slower |   0.21x |  52.21 KB |  1.18x more |
|                                                 |              |              |              |              |              |              |              |         |           |             |
| **ExecuteScalar_Manually**                          |     **76.34 μs** |    **19.826 μs** |    **22.832 μs** |     **72.11 μs** |    **112.93 μs** |    **118.11 μs** |     **baseline** |        **** |   **3.04 KB** |            **** |
| ExecuteScalar_SqlConnectionPlus                 |     63.37 μs |     6.517 μs |     7.505 μs |     60.95 μs |     76.08 μs |     78.81 μs | 1.22x faster |   0.38x |   3.94 KB |  1.30x more |
|                                                 |              |              |              |              |              |              |              |         |           |             |
| **ExecuteXmlReader_Manually**                       |    **847.66 μs** |    **84.039 μs** |    **96.779 μs** |    **814.79 μs** |  **1,008.62 μs** |  **1,031.30 μs** |     **baseline** |        **** | **206.25 KB** |            **** |
| ExecuteXmlReader_SqlConnectionPlus              |    542.53 μs |    21.218 μs |    24.435 μs |    541.74 μs |    573.13 μs |    576.18 μs | 1.57x faster |   0.19x | 207.14 KB |  1.00x more |
|                                                 |              |              |              |              |              |              |              |         |           |             |
| **Exists_Manually**                                 |     **62.26 μs** |     **9.209 μs** |    **10.605 μs** |     **61.98 μs** |     **77.26 μs** |     **80.34 μs** |     **baseline** |        **** |   **2.63 KB** |            **** |
| Exists_SqlConnectionPlus                        |     61.08 μs |     5.706 μs |     6.571 μs |     58.60 μs |     71.08 μs |     75.28 μs | 1.01x slower |   0.19x |   3.46 KB |  1.31x more |
|                                                 |              |              |              |              |              |              |              |         |           |             |
| **InsertEntities_Manually**                         | **17,944.09 μs** | **4,734.740 μs** | **5,452.533 μs** | **15,121.07 μs** | **23,066.39 μs** | **31,658.14 μs** |     **baseline** |        **** | **509.78 KB** |            **** |
| InsertEntities_SqlConnectionPlus                | 16,739.20 μs |   653.952 μs |   753.092 μs | 16,553.63 μs | 17,381.18 μs | 17,647.00 μs | 1.07x faster |   0.32x | 429.59 KB |  1.19x less |
|                                                 |              |              |              |              |              |              |              |         |           |             |
| **InsertEntity_Manually**                           |    **231.15 μs** |    **10.402 μs** |    **11.979 μs** |    **229.74 μs** |    **237.70 μs** |    **243.19 μs** |     **baseline** |        **** |   **8.06 KB** |            **** |
| InsertEntity_SqlConnectionPlus                  |    204.53 μs |    12.502 μs |    14.397 μs |    199.55 μs |    232.81 μs |    235.63 μs | 1.14x faster |   0.09x |   8.21 KB |  1.02x more |
|                                                 |              |              |              |              |              |              |              |         |           |             |
| **Parameter_Manually**                              |     **68.55 μs** |     **4.769 μs** |     **5.492 μs** |     **66.49 μs** |     **76.18 μs** |     **77.40 μs** |     **baseline** |        **** |   **5.43 KB** |            **** |
| Parameter_SqlConnectionPlus                     |     45.53 μs |     4.107 μs |     4.730 μs |     44.01 μs |     46.29 μs |     54.59 μs | 1.52x faster |   0.17x |   7.07 KB |  1.30x more |
|                                                 |              |              |              |              |              |              |              |         |           |             |
| **QueryEntities_Manually**                          |    **175.01 μs** |    **10.774 μs** |    **12.408 μs** |    **169.80 μs** |    **192.95 μs** |    **198.29 μs** |     **baseline** |        **** |  **51.62 KB** |            **** |
| QueryEntities_SqlConnectionPlus                 |    185.32 μs |    15.298 μs |    17.617 μs |    177.44 μs |    202.95 μs |    225.37 μs | 1.06x slower |   0.12x |  49.02 KB |  1.05x less |
|                                                 |              |              |              |              |              |              |              |         |           |             |
| **QueryScalars_Manually**                           |     **83.23 μs** |     **2.789 μs** |     **3.212 μs** |     **82.31 μs** |     **87.82 μs** |     **88.09 μs** |     **baseline** |        **** |   **2.11 KB** |            **** |
| QueryScalars_SqlConnectionPlus                  |     87.92 μs |    11.440 μs |    13.174 μs |     83.16 μs |     97.72 μs |    107.85 μs | 1.06x slower |   0.16x |   7.38 KB |  3.49x more |
|                                                 |              |              |              |              |              |              |              |         |           |             |
| **QueryTuples_Manually**                            |    **136.48 μs** |    **22.280 μs** |    **25.658 μs** |    **130.87 μs** |    **137.34 μs** |    **153.50 μs** |     **baseline** |        **** |  **17.83 KB** |            **** |
| QueryTuples_SqlConnectionPlus                   |    149.00 μs |    10.728 μs |    12.355 μs |    155.84 μs |    159.70 μs |    162.11 μs | 1.11x slower |   0.15x |  29.87 KB |  1.68x more |
|                                                 |              |              |              |              |              |              |              |         |           |             |
| **TemporaryTable_ComplexObjects_Manually**          |  **7,837.71 μs** |   **370.742 μs** |   **426.947 μs** |  **7,708.80 μs** |  **8,048.61 μs** |  **8,298.30 μs** |     **baseline** |        **** | **124.71 KB** |            **** |
| TemporaryTable_ComplexObjects_SqlConnectionPlus |  5,307.00 μs |   511.600 μs |   589.160 μs |  5,080.57 μs |  5,648.26 μs |  5,850.84 μs | 1.49x faster |   0.15x | 126.62 KB |  1.02x more |
|                                                 |              |              |              |              |              |              |              |         |           |             |
| **TemporaryTable_ScalarValues_Manually**            |  **6,125.53 μs** |   **198.957 μs** |   **229.120 μs** |  **6,088.75 μs** |  **6,452.06 μs** |  **6,511.31 μs** |     **baseline** |        **** | **177.24 KB** |            **** |
| TemporaryTable_ScalarValues_SqlConnectionPlus   |  4,455.34 μs |   289.132 μs |   332.964 μs |  4,297.05 μs |  4,879.30 μs |  5,210.70 μs | 1.38x faster |   0.10x | 304.17 KB |  1.72x more |
|                                                 |              |              |              |              |              |              |              |         |           |             |
| **UpdateEntities_Manually**                         | **19,928.98 μs** | **5,451.688 μs** | **6,278.172 μs** | **16,359.88 μs** | **30,319.58 μs** | **31,049.16 μs** |     **baseline** |        **** | **524.35 KB** |            **** |
| UpdateEntities_SqlConnectionPlus                | 15,997.96 μs | 2,367.983 μs | 2,726.972 μs | 14,913.02 μs | 17,984.89 μs | 18,629.16 μs | 1.27x faster |   0.42x | 445.12 KB |  1.18x less |
|                                                 |              |              |              |              |              |              |              |         |           |             |
| **UpdateEntity_Manually**                           |    **192.90 μs** |    **26.106 μs** |    **30.064 μs** |    **179.29 μs** |    **219.15 μs** |    **252.65 μs** |     **baseline** |        **** |   **9.02 KB** |            **** |
| UpdateEntity_SqlConnectionPlus                  |    175.18 μs |    13.888 μs |    15.993 μs |    169.02 μs |    185.11 μs |    204.39 μs | 1.11x faster |   0.19x |   9.17 KB |  1.02x more |

Please keep in mind that benchmarking is tricky when SQL Server is involved.
So take these benchmark results with a grain of salt.

### Running the benchmarks
To run the benchmarks, ensure you have an SQL Server instance available.  
The benchmarks will create a database named `SqlConnectionPlusTests`, so make sure your SQL user has the necessary 
rights.

Set the environment variable `ConnectionString` to the connection string to the SQL Server instance:
~~~shell
set ConnectionString="Data Source=.\SqlServer;Integrated Security=True;Encrypt=False;MultipleActiveResultSets=True"
~~~

Then run the following command:
~~~shell
dotnet run --configuration Release --project benchmarks\SqlConnectionPlus.Benchmarks\SqlConnectionPlus.Benchmarks.csproj
~~~

## Running the unit tests
Run the unit tests using the Test Explorer in Visual Studio or via the following command:
~~~shell
dotnet test tests\SqlConnectionPlus.UnitTests\SqlConnectionPlus.UnitTests.csproj --logger "console;verbosity=detailed"
~~~

## Running the integration tests
To run the integration tests, ensure you have an SQL Server instance available and update the connection string to the 
SQL Server instance in the file `tests\SqlConnectionPlus.IntegrationTests\Local.runsettings`.  
The tests will create a database named `SqlConnectionPlusTests`, so make sure your SQL user has the necessary rights.

Make sure the runsettings file is selected in Visual Studio:  
- In the Visual Studio menu, go to `Test` -> `Configure Run Settings` and click on 
`Select Solution Wide runsettings File`.  
- In the file dialog, select the file `tests\SqlConnectionPlus.IntegrationTests\Local.runsettings`.  

Then run the tests using the Test Explorer in Visual Studio or via the following command:
~~~shell
dotnet test tests\SqlConnectionPlus.IntegrationTests\SqlConnectionPlus.IntegrationTests.csproj --settings tests\SqlConnectionPlus.IntegrationTests\Local.runsettings --logger "console;verbosity=detailed"
~~~
  
## Contributing
Contributions and bug reports are welcome and appreciated.  
Please follow the repository's [CONTRIBUTING.md](CONTRIBUTING.md) and code style.  
Open a GitHub issue for problems or a pull request with tests and a clear description of changes.

## License
This library is licensed under the [MIT license](LICENSE.md).

## Documentation
Full API documentation is available
[here](https://rent-a-developer.github.io/SqlConnectionPlus/api/RentADeveloper.SqlConnectionPlus.html).

## Change Log
The change log is available [here](CHANGELOG.md).

## Contributors

- David Liebeherr ([info@rent-a-developer.de](mailto:info@rent-a-developer.de))

