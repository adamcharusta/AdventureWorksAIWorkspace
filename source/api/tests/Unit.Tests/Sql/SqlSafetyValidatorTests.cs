using AdventureWorksAIWorkspace.Infrastructure.Services.Sql;
using Microsoft.Extensions.Logging.Abstractions;

namespace AdventureWorksAIWorkspace.Unit.Tests.Sql;

public sealed class SqlSafetyValidatorTests
{
    private static SqlSafetyValidator CreateValidator() =>
        new(NullLogger<SqlSafetyValidator>.Instance);

    [Theory]
    [InlineData("SELECT TOP 10 Name FROM Production.Product")]
    [InlineData("select productid, name from production.product where listprice > 100")]
    [InlineData("WITH recent AS (SELECT SalesOrderID FROM Sales.SalesOrderHeader) SELECT * FROM recent")]
    [InlineData("SELECT 1; ")]
    [InlineData("SELECT /* inline comment with DROP word */ Name FROM Production.Product")]
    [InlineData("SELECT Name FROM Production.Product -- trailing comment with DELETE\n")]
    public void Validate_Should_AllowReadOnlyQueries(string sql)
    {
        var validator = CreateValidator();

        var result = validator.Validate(sql);

        result.IsValid.Should().BeTrue();
        result.Reason.Should().BeNull();
    }

    [Theory]
    [InlineData("INSERT INTO Production.Product (Name) VALUES ('x')", "INSERT")]
    [InlineData("UPDATE Production.Product SET Name = 'x'", "UPDATE")]
    [InlineData("DELETE FROM Production.Product", "DELETE")]
    [InlineData("DROP TABLE Production.Product", "DROP")]
    [InlineData("ALTER TABLE Production.Product ADD Col INT", "ALTER")]
    [InlineData("TRUNCATE TABLE Production.Product", "TRUNCATE")]
    [InlineData("EXEC sp_who", "EXEC")]
    [InlineData("MERGE INTO Production.Product AS t USING src ON t.Id = src.Id", "MERGE")]
    [InlineData("CREATE TABLE Foo (Id INT)", "CREATE")]
    [InlineData("GRANT SELECT ON Production.Product TO public", "GRANT")]
    [InlineData("REVOKE SELECT ON Production.Product FROM public", "REVOKE")]
    public void Validate_Should_RejectStatementsStartingWithDestructiveCommands(string sql, string _)
    {
        var validator = CreateValidator();

        var result = validator.Validate(sql);

        result.IsValid.Should().BeFalse();
        result.Reason.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Validate_Should_RejectDestructiveKeywordInsideAllowedQuery()
    {
        var validator = CreateValidator();

        var result = validator.Validate("WITH t AS (SELECT 1 AS Id) DELETE FROM Production.Product");

        result.IsValid.Should().BeFalse();
        result.Reason.Should().Contain("DELETE");
    }

    [Fact]
    public void Validate_Should_RejectSelectInto()
    {
        var validator = CreateValidator();

        var result = validator.Validate("SELECT * INTO Backup FROM Production.Product");

        result.IsValid.Should().BeFalse();
        result.Reason.Should().Contain("INTO");
    }

    [Fact]
    public void Validate_Should_RejectStackedStatements()
    {
        var validator = CreateValidator();

        var result = validator.Validate("SELECT 1; DROP TABLE Production.Product");

        result.IsValid.Should().BeFalse();
        result.Reason.Should().Contain("Multiple SQL statements");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_Should_RejectEmptyInput(string? sql)
    {
        var validator = CreateValidator();

        var result = validator.Validate(sql!);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_Should_RejectNonSelectEntryPoint()
    {
        var validator = CreateValidator();

        var result = validator.Validate("EXPLAIN SELECT 1");

        result.IsValid.Should().BeFalse();
        result.Reason.Should().Contain("SELECT or WITH");
    }
}
