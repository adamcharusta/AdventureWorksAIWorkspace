using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdventureWorksAIWorkspace.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddReportChatPersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    OriginalPrompt = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    IsFavorite = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reports_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportConversations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ReportId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportConversations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportConversations_Reports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "Reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GeneratedSqlQueries",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ReportId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    SourceMessageId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UserPrompt = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    SqlText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Explanation = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ValidationStatus = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    ValidationMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ExecutionStatus = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    ExecutionMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    InputTokens = table.Column<int>(type: "int", nullable: true),
                    OutputTokens = table.Column<int>(type: "int", nullable: true),
                    ResultRowCount = table.Column<int>(type: "int", nullable: true),
                    ResultColumnCount = table.Column<int>(type: "int", nullable: true),
                    DurationMs = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneratedSqlQueries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GeneratedSqlQueries_Reports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "Reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportMessages",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ConversationId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    RelatedSqlQueryId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportMessages_GeneratedSqlQueries_RelatedSqlQueryId",
                        column: x => x.RelatedSqlQueryId,
                        principalTable: "GeneratedSqlQueries",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReportMessages_ReportConversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "ReportConversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedSqlQueries_ReportId_CreatedAt",
                table: "GeneratedSqlQueries",
                columns: new[] { "ReportId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedSqlQueries_SourceMessageId",
                table: "GeneratedSqlQueries",
                column: "SourceMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportConversations_ReportId",
                table: "ReportConversations",
                column: "ReportId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReportMessages_ConversationId_SortOrder",
                table: "ReportMessages",
                columns: new[] { "ConversationId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReportMessages_RelatedSqlQueryId",
                table: "ReportMessages",
                column: "RelatedSqlQueryId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_UserId_UpdatedAt",
                table: "Reports",
                columns: new[] { "UserId", "UpdatedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_GeneratedSqlQueries_ReportMessages_SourceMessageId",
                table: "GeneratedSqlQueries",
                column: "SourceMessageId",
                principalTable: "ReportMessages",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GeneratedSqlQueries_ReportMessages_SourceMessageId",
                table: "GeneratedSqlQueries");

            migrationBuilder.DropTable(
                name: "ReportMessages");

            migrationBuilder.DropTable(
                name: "GeneratedSqlQueries");

            migrationBuilder.DropTable(
                name: "ReportConversations");

            migrationBuilder.DropTable(
                name: "Reports");
        }
    }
}
