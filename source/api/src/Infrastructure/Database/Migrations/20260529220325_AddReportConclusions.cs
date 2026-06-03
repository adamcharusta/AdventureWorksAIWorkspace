using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdventureWorksAIWorkspace.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddReportConclusions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Conclusions",
                table: "Reports",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Conclusions",
                table: "GeneratedSqlQueries",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Conclusions",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "Conclusions",
                table: "GeneratedSqlQueries");
        }
    }
}
