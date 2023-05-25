using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial_Migration_Including_ContractBasicInfo_And_LaborCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContractBasicInfo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    ContractName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractBasicInfo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LaborCategory",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    CategoryName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RatePerHour = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ContractId = table.Column<string>(type: "nvarchar(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LaborCategory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LaborCategory_ContractBasicInfo_ContractId",
                        column: x => x.ContractId,
                        principalTable: "ContractBasicInfo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LaborCategory_ContractId",
                table: "LaborCategory",
                column: "ContractId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LaborCategory");

            migrationBuilder.DropTable(
                name: "ContractBasicInfo");
        }
    }
}
