using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdventureWorksAIWorkspaceAPI.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddReportPresentationSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ChartsJson",
                table: "Reports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResultJson",
                table: "Reports",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChartsJson",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ResultJson",
                table: "Reports");
        }
    }
}
