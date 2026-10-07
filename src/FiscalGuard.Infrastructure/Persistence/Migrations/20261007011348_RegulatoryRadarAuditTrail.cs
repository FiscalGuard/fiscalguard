using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FiscalGuard.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RegulatoryRadarAuditTrail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RegulatoryDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalId = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    SourceKey = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    SourceName = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    SourceType = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Summary = table.Column<string>(type: "text", nullable: false),
                    SourceUrl = table.Column<string>(type: "text", nullable: false),
                    Theme = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    ImpactLevel = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ImpactReason = table.Column<string>(type: "text", nullable: false),
                    BusinessImpact = table.Column<string>(type: "text", nullable: false),
                    SuggestedAction = table.Column<string>(type: "text", nullable: false),
                    RawText = table.Column<string>(type: "text", nullable: true),
                    SearchText = table.Column<string>(type: "text", nullable: true),
                    PresentedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CapturedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RelevanceScore = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegulatoryDocuments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RegulatorySourceRuns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceKey = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    SourceName = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    RequestUrl = table.Column<string>(type: "text", nullable: true),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FinishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Success = table.Column<bool>(type: "boolean", nullable: false),
                    DocumentsFound = table.Column<int>(type: "integer", nullable: false),
                    DocumentsAccepted = table.Column<int>(type: "integer", nullable: false),
                    DurationMs = table.Column<long>(type: "bigint", nullable: false),
                    Error = table.Column<string>(type: "text", nullable: true),
                    StatusMessage = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegulatorySourceRuns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RegulatoryMatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RegulatoryDocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    MatchedTerms = table.Column<string>(type: "text", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegulatoryMatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegulatoryMatches_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RegulatoryMatches_RegulatoryDocuments_RegulatoryDocumentId",
                        column: x => x.RegulatoryDocumentId,
                        principalTable: "RegulatoryDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RegulatoryDocuments_OrganizationId_ExternalId",
                table: "RegulatoryDocuments",
                columns: new[] { "OrganizationId", "ExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegulatoryDocuments_OrganizationId_PresentedAt",
                table: "RegulatoryDocuments",
                columns: new[] { "OrganizationId", "PresentedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_RegulatoryMatches_CompanyId",
                table: "RegulatoryMatches",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_RegulatoryMatches_OrganizationId_CompanyId",
                table: "RegulatoryMatches",
                columns: new[] { "OrganizationId", "CompanyId" });

            migrationBuilder.CreateIndex(
                name: "IX_RegulatoryMatches_OrganizationId_RegulatoryDocumentId_Compa~",
                table: "RegulatoryMatches",
                columns: new[] { "OrganizationId", "RegulatoryDocumentId", "CompanyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegulatoryMatches_RegulatoryDocumentId",
                table: "RegulatoryMatches",
                column: "RegulatoryDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_RegulatorySourceRuns_OrganizationId_StartedAt",
                table: "RegulatorySourceRuns",
                columns: new[] { "OrganizationId", "StartedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegulatoryMatches");

            migrationBuilder.DropTable(
                name: "RegulatorySourceRuns");

            migrationBuilder.DropTable(
                name: "RegulatoryDocuments");
        }
    }
}
