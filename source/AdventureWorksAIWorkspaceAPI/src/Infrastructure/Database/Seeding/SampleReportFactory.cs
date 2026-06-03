using System.Text.Json;
using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.AdventureWorks;
using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.Charts;
using AdventureWorksAIWorkspaceAPI.Domain.Reports;

namespace AdventureWorksAIWorkspaceAPI.Infrastructure.Database.Seeding;

/// <summary>
/// Builds the demo report shown to a freshly seeded admin so the workspace opens fully populated:
/// an AI business summary, charts, a SQL audit trail, and a two-turn chat history. The numbers
/// mirror the shape of the AdventureWorks 2013 sales data and are internally consistent (category
/// revenue and the monthly trend both total $47,905,470). This is sample content only; keeping it
/// here separates the demo data from the database bootstrap in <see cref="AppDbContextInitializer"/>.
/// </summary>
internal static class SampleReportFactory
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Creates the sample report graph (report, conversation, messages, and generated SQL history)
    /// owned by <paramref name="adminUserId"/>. The caller persists it.
    /// </summary>
    public static Report Create(string adminUserId)
    {
        var createdAt = DateTime.UtcNow.AddDays(-3);

        // --- Turn 1: product-category overview (bar + pie) ---
        const string categoryPrompt = "How did each product category perform in 2013?";
        const string categoryInsights =
            "Bikes dominate 2013 revenue at roughly $40.8M (85% of the $47.9M total), even though they " +
            "account for the fewest units sold. Accessories show the opposite profile: the highest unit " +
            "volume (61.3K units) but only about $1.1M in revenue, confirming their role as low-price " +
            "add-ons. Clothing and Components sit in between. The clearest opportunity is lifting the " +
            "accessory attach-rate on every bike sale to grow margin without depending solely on big-ticket bikes.";
        const string categoryConclusions =
            "Revenue is dangerously concentrated in a single category: with 85% of sales tied to Bikes, " +
            "a downturn in that segment would hit the whole business. Treat lifting the accessory " +
            "attach-rate as the primary growth lever for the next planning cycle — even a few extra " +
            "accessories per bike sale would meaningfully diversify revenue without needing more bike volume.";
        var categoryResult = new TabularResult(
            [
                new TabularColumn("Category", "nvarchar"),
                new TabularColumn("Revenue", "decimal"),
                new TabularColumn("UnitsSold", "int")
            ],
            [
                [(object?)"Bikes", 40_778_900m, 31_200],
                [(object?)"Components", 4_102_340m, 28_400],
                [(object?)"Clothing", 1_884_560m, 41_900],
                [(object?)"Accessories", 1_139_670m, 61_300]
            ],
            4,
            false,
            38);
        ChartSpec[] categoryCharts =
        [
            new(
                ChartKind.Bar,
                "Revenue by category",
                "Category",
                [new ChartSeries("Revenue", "Revenue (USD)")],
                "Total 2013 revenue per product category."),
            new(
                ChartKind.Pie,
                "Revenue share",
                "Category",
                [new ChartSeries("Revenue", "Revenue")],
                "Each category's share of total 2013 revenue.")
        ];

        // --- Turn 2 (displayed result): monthly revenue & order-volume trend (line + bar) ---
        const string trendPrompt = "Now show me how monthly revenue and order volume trended through 2013.";
        const string trendInsights =
            "Revenue climbed steadily across 2013, from about $2.75M in January to $5.18M in December " +
            "— an 88% increase over the year. Order volume tells a sharper story: monthly orders held " +
            "around 800 through June, then jumped from ~1,540 in July to nearly 3,000 by December as the " +
            "online channel scaled. That surge pulled the average order value down from roughly $3,480 to " +
            "$1,740, signalling a shift from a few large reseller orders toward many smaller online purchases. " +
            "The takeaway: the business is successfully broadening its customer base, so the focus now should " +
            "be on protecting margin per order as the channel mix tilts toward direct-to-consumer sales.";
        const string trendConclusions =
            "The chart shows a clear, sustained upward trend. If the 2013 growth rate holds, monthly " +
            "revenue would roughly double again within two years, so planning capacity and inventory " +
            "ahead of demand is worthwhile. Watch the falling average order value, though: volume is " +
            "carrying the growth while per-order value erodes, so the next priority should be raising " +
            "basket size on online orders before scaling spend further.";
        var trendResult = new TabularResult(
            [
                new TabularColumn("Month", "nvarchar"),
                new TabularColumn("Revenue", "decimal"),
                new TabularColumn("Orders", "int"),
                new TabularColumn("AverageOrderValue", "decimal")
            ],
            [
                [(object?)"2013-01", 2_750_360m, 790, 3_481.47m],
                [(object?)"2013-02", 2_914_330m, 768, 3_794.70m],
                [(object?)"2013-03", 3_412_070m, 812, 4_202.06m],
                [(object?)"2013-04", 3_290_840m, 798, 4_124.86m],
                [(object?)"2013-05", 3_797_610m, 845, 4_494.21m],
                [(object?)"2013-06", 4_114_920m, 905, 4_546.87m],
                [(object?)"2013-07", 3_968_330m, 1_540, 2_576.84m],
                [(object?)"2013-08", 4_302_880m, 2_010, 2_140.74m],
                [(object?)"2013-09", 4_521_990m, 2_260, 2_000.88m],
                [(object?)"2013-10", 4_936_470m, 2_540, 1_943.49m],
                [(object?)"2013-11", 4_711_250m, 2_690, 1_751.39m],
                [(object?)"2013-12", 5_184_420m, 2_980, 1_739.74m]
            ],
            12,
            false,
            61);
        ChartSpec[] trendCharts =
        [
            new(
                ChartKind.Line,
                "Monthly revenue trend",
                "Month",
                [new ChartSeries("Revenue", "Revenue (USD)")],
                "Total revenue per month across 2013."),
            new(
                ChartKind.Bar,
                "Orders per month",
                "Month",
                [new ChartSeries("Orders", "Orders")],
                "Number of sales orders placed each month."),
            new(
                ChartKind.Line,
                "Average order value",
                "Month",
                [new ChartSeries("AverageOrderValue", "Avg order value (USD)")],
                "Average revenue per order, month over month.")
        ];

        var report = new Report
        {
            UserId = adminUserId,
            Title = "2013 sales performance overview",
            OriginalPrompt = categoryPrompt,
            // The workspace shows the most recent turn's result, charts, and summary.
            Summary = trendInsights,
            Conclusions = trendConclusions,
            ResultJson = JsonSerializer.Serialize(trendResult, JsonOptions),
            ChartsJson = JsonSerializer.Serialize(trendCharts, JsonOptions),
            Status = ReportStatus.Ready,
            IsFavorite = true,
            CreatedAt = createdAt,
            UpdatedAt = createdAt.AddMinutes(2)
        };

        var conversation = new ReportConversation
        {
            ReportId = report.Id,
            CreatedAt = createdAt,
            UpdatedAt = createdAt.AddMinutes(2)
        };
        report.Conversation = conversation;

        var categoryUserMessage = new ReportMessage
        {
            ConversationId = conversation.Id,
            Role = ReportMessageRole.User,
            Content = categoryPrompt,
            SortOrder = 1,
            CreatedAt = createdAt
        };

        var categorySqlQuery = new GeneratedSqlQuery
        {
            ReportId = report.Id,
            SourceMessageId = categoryUserMessage.Id,
            UserPrompt = categoryPrompt,
            SqlText =
                "SELECT pc.Name AS Category, SUM(sod.LineTotal) AS Revenue, SUM(sod.OrderQty) AS UnitsSold " +
                "FROM Sales.SalesOrderDetail sod " +
                "JOIN Sales.SalesOrderHeader soh ON soh.SalesOrderID = sod.SalesOrderID " +
                "JOIN Production.Product p ON p.ProductID = sod.ProductID " +
                "JOIN Production.ProductSubcategory ps ON ps.ProductSubcategoryID = p.ProductSubcategoryID " +
                "JOIN Production.ProductCategory pc ON pc.ProductCategoryID = ps.ProductCategoryID " +
                "WHERE soh.OrderDate >= '2013-01-01' AND soh.OrderDate < '2014-01-01' " +
                "GROUP BY pc.Name ORDER BY Revenue DESC;",
            Explanation =
                "Joins order detail lines to the product-category hierarchy, filters to 2013 order dates, " +
                "and aggregates revenue and units sold per category.",
            PresentationTitle = "Product category performance",
            Summary = categoryInsights,
            Conclusions = categoryConclusions,
            ResultJson = JsonSerializer.Serialize(categoryResult, JsonOptions),
            ChartsJson = JsonSerializer.Serialize(categoryCharts, JsonOptions),
            ValidationStatus = SqlValidationStatus.Valid,
            ExecutionStatus = SqlExecutionStatus.Executed,
            InputTokens = 142,
            OutputTokens = 96,
            ResultRowCount = 4,
            ResultColumnCount = 3,
            DurationMs = 38,
            CreatedAt = createdAt
        };
        report.GeneratedSqlQueries.Add(categorySqlQuery);

        var categoryAssistantMessage = new ReportMessage
        {
            ConversationId = conversation.Id,
            Role = ReportMessageRole.Assistant,
            Content = categoryInsights,
            SortOrder = 2,
            RelatedSqlQueryId = categorySqlQuery.Id,
            CreatedAt = createdAt.AddSeconds(6)
        };

        var trendUserMessage = new ReportMessage
        {
            ConversationId = conversation.Id,
            Role = ReportMessageRole.User,
            Content = trendPrompt,
            SortOrder = 3,
            CreatedAt = createdAt.AddMinutes(1)
        };

        var trendSqlQuery = new GeneratedSqlQuery
        {
            ReportId = report.Id,
            SourceMessageId = trendUserMessage.Id,
            UserPrompt = trendPrompt,
            SqlText =
                "SELECT FORMAT(soh.OrderDate, 'yyyy-MM') AS Month, " +
                "SUM(soh.TotalDue) AS Revenue, " +
                "COUNT(DISTINCT soh.SalesOrderID) AS Orders, " +
                "SUM(soh.TotalDue) / COUNT(DISTINCT soh.SalesOrderID) AS AverageOrderValue " +
                "FROM Sales.SalesOrderHeader soh " +
                "WHERE soh.OrderDate >= '2013-01-01' AND soh.OrderDate < '2014-01-01' " +
                "GROUP BY FORMAT(soh.OrderDate, 'yyyy-MM') ORDER BY Month;",
            Explanation =
                "Buckets sales orders by calendar month for 2013 and reports revenue, distinct order count, " +
                "and the derived average order value per month.",
            PresentationTitle = "Monthly revenue and orders",
            Summary = trendInsights,
            Conclusions = trendConclusions,
            ResultJson = JsonSerializer.Serialize(trendResult, JsonOptions),
            ChartsJson = JsonSerializer.Serialize(trendCharts, JsonOptions),
            ValidationStatus = SqlValidationStatus.Valid,
            ExecutionStatus = SqlExecutionStatus.Executed,
            InputTokens = 168,
            OutputTokens = 110,
            ResultRowCount = 12,
            ResultColumnCount = 4,
            DurationMs = 61,
            CreatedAt = createdAt.AddMinutes(1)
        };
        report.GeneratedSqlQueries.Add(trendSqlQuery);

        var trendAssistantMessage = new ReportMessage
        {
            ConversationId = conversation.Id,
            Role = ReportMessageRole.Assistant,
            Content = trendInsights,
            SortOrder = 4,
            RelatedSqlQueryId = trendSqlQuery.Id,
            CreatedAt = createdAt.AddMinutes(1).AddSeconds(8)
        };

        conversation.Messages.Add(categoryUserMessage);
        conversation.Messages.Add(categoryAssistantMessage);
        conversation.Messages.Add(trendUserMessage);
        conversation.Messages.Add(trendAssistantMessage);

        return report;
    }
}
