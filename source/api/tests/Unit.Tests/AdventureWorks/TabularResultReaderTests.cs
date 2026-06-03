using System.Data;
using AdventureWorksAIWorkspace.Infrastructure.Services.AdventureWorks;

namespace AdventureWorksAIWorkspace.Unit.Tests.AdventureWorks;

public sealed class TabularResultReaderTests
{
    private static DataTableReader CreateReader()
    {
        var table = new DataTable();
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("Name", typeof(string));
        table.Rows.Add(1, "Alpha");
        table.Rows.Add(2, "Beta");
        table.Rows.Add(3, null);

        return table.CreateDataReader();
    }

    [Fact]
    public void Read_Should_MapColumnsRowsAndNulls()
    {
        using DataTableReader reader = CreateReader();

        var result = TabularResultReader.Read(reader, maxRows: 100);

        result.Columns.Should().HaveCount(2);
        result.Columns[0].Name.Should().Be("Id");
        result.Columns[1].Name.Should().Be("Name");
        result.Columns.Should().OnlyContain(column => !string.IsNullOrWhiteSpace(column.DataType));

        result.RowCount.Should().Be(3);
        result.Rows.Should().HaveCount(3);
        result.Rows[0].Should().Equal(1, "Alpha");
        result.Rows[2][1].Should().BeNull();

        result.Truncated.Should().BeFalse();
        result.ElapsedMilliseconds.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void Read_Should_TruncateWhenRowLimitIsReached()
    {
        using DataTableReader reader = CreateReader();

        var result = TabularResultReader.Read(reader, maxRows: 2);

        result.RowCount.Should().Be(2);
        result.Rows.Should().HaveCount(2);
        result.Truncated.Should().BeTrue();
    }
}
