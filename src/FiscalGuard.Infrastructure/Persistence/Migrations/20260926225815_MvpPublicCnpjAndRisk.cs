using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FiscalGuard.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MvpPublicCnpjAndRisk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EvidenceType",
                table: "FiscalIssues",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Recommendation",
                table: "FiscalIssues",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsMeiOption",
                table: "Companies",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSimplesOption",
                table: "Companies",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MainCnaeCode",
                table: "Companies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MainCnaeDescription",
                table: "Companies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublicAddress",
                table: "Companies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublicDataSource",
                table: "Companies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PublicDataUpdatedAt",
                table: "Companies",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegistrationStatus",
                table: "Companies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "RegistrationStatusDate",
                table: "Companies",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RiskLevel",
                table: "Companies",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "RiskScore",
                table: "Companies",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RiskSummary",
                table: "Companies",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EvidenceType",
                table: "FiscalIssues");

            migrationBuilder.DropColumn(
                name: "Recommendation",
                table: "FiscalIssues");

            migrationBuilder.DropColumn(
                name: "IsMeiOption",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "IsSimplesOption",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "MainCnaeCode",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "MainCnaeDescription",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "PublicAddress",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "PublicDataSource",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "PublicDataUpdatedAt",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "RegistrationStatus",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "RegistrationStatusDate",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "RiskLevel",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "RiskScore",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "RiskSummary",
                table: "Companies");
        }
    }
}
