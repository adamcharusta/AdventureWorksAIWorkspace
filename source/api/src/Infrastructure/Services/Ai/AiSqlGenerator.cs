using AdventureWorksAIWorkspace.Application.Common.Dtos.Ai;
using AdventureWorksAIWorkspace.Application.Common.Services.Ai;
using AdventureWorksAIWorkspace.Application.Common.Services.Sql;
using Ardalis.GuardClauses;

namespace AdventureWorksAIWorkspace.Infrastructure.Services.Ai;

/// <summary>
/// Generates read-only AdventureWorks SQL from a natural-language question by prompting the AI
/// model through <see cref="IAiChatClient"/>.
/// </summary>
/// <remarks>
/// The prompt instructs the model to emit a single read-only SELECT, but the model is untrusted:
/// the generated SQL must still pass <see cref="ISqlSafetyValidator"/> before it is executed.
/// </remarks>
public sealed class AiSqlGenerator : IAiSqlGenerator
{
    private const string SystemPrompt =
        """
        You are a senior T-SQL analyst for the Microsoft AdventureWorks SQL Server database.
        Convert the user's business question into a single, read-only T-SQL query.

        Known AdventureWorks schema:
        - Sales.SalesOrderHeader: SalesOrderID, OrderDate, DueDate, ShipDate, Status, OnlineOrderFlag, CustomerID, SalesPersonID, TerritoryID, SubTotal, TaxAmt, Freight, TotalDue.
        - Sales.SalesOrderDetail: SalesOrderID, SalesOrderDetailID, ProductID, SpecialOfferID, OrderQty, UnitPrice, UnitPriceDiscount, LineTotal.
        - Production.Product: ProductID, Name, ProductNumber, Color, StandardCost, ListPrice, Size, ProductSubcategoryID, SellStartDate, SellEndDate.
        - Production.ProductSubcategory: ProductSubcategoryID, ProductCategoryID, Name.
        - Production.ProductCategory: ProductCategoryID, Name.
        - Sales.Customer: CustomerID, PersonID, StoreID, TerritoryID, AccountNumber.
        - Person.Person: BusinessEntityID, FirstName, MiddleName, LastName.
        - Sales.SalesTerritory: TerritoryID, Name, CountryRegionCode, [Group], SalesYTD, SalesLastYear.
        - Sales.SalesPerson: BusinessEntityID, TerritoryID, SalesQuota, Bonus, CommissionPct, SalesYTD, SalesLastYear.
        - Purchasing.PurchaseOrderHeader: PurchaseOrderID, VendorID, OrderDate, ShipDate, SubTotal, TaxAmt, Freight, TotalDue.
        - Purchasing.PurchaseOrderDetail: PurchaseOrderID, PurchaseOrderDetailID, ProductID, OrderQty, UnitPrice, LineTotal, ReceivedQty, RejectedQty.
        - Production.ProductInventory: ProductID, LocationID, Shelf, Bin, Quantity.

        Business rules:
        - For product/category sales reports, join Sales.SalesOrderDetail to Production.Product, then ProductSubcategory, then ProductCategory.
        - Use Sales.SalesOrderHeader.OrderDate for sales trends and time filtering.
        - Use Sales.SalesOrderHeader.TotalDue for order-level revenue and Sales.SalesOrderDetail.LineTotal for product-level revenue.
        - Use readable aliases for all output columns. User-facing aliases should match the language of the user's question when practical; keep database object names unchanged.
        - Do not use SELECT *.
        - Use TOP (100) when the question does not specify a limit.

        Rules:
        - Return exactly one statement. A leading common table expression (WITH ...) is allowed, but it must feed a SELECT.
        - The statement must be read-only. Never use INSERT, UPDATE, DELETE, DROP, ALTER, TRUNCATE, EXEC, MERGE, CREATE, GRANT, REVOKE, or SELECT ... INTO.
        - Do not return multiple statements separated by semicolons. Do not end the statement with a semicolon, and do not place a semicolon before WITH.
        - ORDER BY is only valid in the final, outermost SELECT. Never place ORDER BY inside a CTE, subquery, derived table, or view unless it is paired with TOP, OFFSET, or FETCH in that same scope. To return the top N rows by a measure, put TOP (N) and ORDER BY together in the outer SELECT.
        - Use schema-qualified object names, for example Sales.SalesOrderHeader.
        - Use TOP to keep result sets reasonable when the question does not specify a limit.
        - Output only the SQL statement. Do not add explanations, comments, or markdown code fences.
        """;

    private readonly IAiChatClient _chatClient;

    public AiSqlGenerator(IAiChatClient chatClient)
    {
        Guard.Against.Null(chatClient);
        _chatClient = chatClient;
    }

    public Task<GeneratedSql> GenerateSqlAsync(string question, CancellationToken cancellationToken = default) =>
        GenerateSqlAsync(question, context: null, cancellationToken);

    public async Task<GeneratedSql> GenerateSqlAsync(
        string question,
        AiSqlGenerationContext? context,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrWhiteSpace(question);

        IReadOnlyList<AiChatMessage> messages =
        [
            AiChatMessage.System(SystemPrompt),
            AiChatMessage.User(BuildUserPrompt(question, context))
        ];

        AiChatResult result = await _chatClient.CompleteAsync(messages, cancellationToken);

        string sql = AiResponseParser.StripCodeFence(result.Content);

        return new GeneratedSql(sql, result.InputTokens, result.OutputTokens);
    }

    private static string BuildUserPrompt(string question, AiSqlGenerationContext? context)
    {
        if (context is null)
        {
            return $"Business question:\n{question}";
        }

        List<string> sections =
        [
            $"Business question:\n{question}"
        ];

        if (!string.IsNullOrWhiteSpace(context.OriginalPrompt))
        {
            sections.Add($"Original report prompt:\n{context.OriginalPrompt}");
        }

        if (!string.IsNullOrWhiteSpace(context.CurrentSummary))
        {
            sections.Add($"Current report summary:\n{context.CurrentSummary}");
        }

        if (context.RecentMessages.Count > 0)
        {
            sections.Add($"Recent conversation:\n{string.Join("\n", context.RecentMessages)}");
        }

        if (!string.IsNullOrWhiteSpace(context.LastSuccessfulSql))
        {
            sections.Add($"Last successful SQL:\n{context.LastSuccessfulSql}");
        }

        if (context.PreviousFailure is not null)
        {
            sections.Add(
                $"""
                Your previous SQL attempt failed and must be corrected. Return a corrected single read-only query that resolves this error.
                Failed SQL:
                {context.PreviousFailure.FailedSql}
                Error:
                {context.PreviousFailure.Error}
                """);
        }

        return string.Join("\n\n", sections);
    }
}
