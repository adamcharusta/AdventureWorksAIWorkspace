namespace AdventureWorksAIWorkspace.Application.Common.Dtos.AdventureWorks;

/// <summary>
/// Describes a single column of a generic AdventureWorks query result.
/// </summary>
/// <param name="Name">The column name as reported by the result set.</param>
/// <param name="DataType">The provider data type name (for example, <c>int</c> or <c>nvarchar</c>).</param>
public sealed record TabularColumn(string Name, string DataType);
