using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdventureWorksAIWorkspace.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddGeneratedSqlPresentationSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ChartsJson",
                table: "GeneratedSqlQueries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PresentationTitle",
                table: "GeneratedSqlQueries",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResultJson",
                table: "GeneratedSqlQueries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Summary",
                table: "GeneratedSqlQueries",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChartsJson",
                table: "GeneratedSqlQueries");

            migrationBuilder.DropColumn(
                name: "PresentationTitle",
                table: "GeneratedSqlQueries");

            migrationBuilder.DropColumn(
                name: "ResultJson",
                table: "GeneratedSqlQueries");

            migrationBuilder.DropColumn(
                name: "Summary",
                table: "GeneratedSqlQueries");
        }
    }
}
