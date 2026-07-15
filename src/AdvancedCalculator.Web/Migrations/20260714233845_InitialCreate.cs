using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdvancedCalculator.Web.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CalculationHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Expression = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Result = table.Column<double>(type: "REAL", nullable: false),
                    IsFavorite = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalculationHistories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CalculationHistories_CreatedAtUtc",
                table: "CalculationHistories",
                column: "CreatedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalculationHistories");
        }
    }
}
