using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Added_TVUT_of_Contract_and_Category : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE TYPE dbo.ContractBasicInfoData AS TABLE (Id uniqueidentifier, ContractName nvarchar(255), ClientName nvarchar(255), StartDate datetime, EndDate datetime)");
            migrationBuilder.Sql("CREATE TYPE dbo.LaborCategoryData AS TABLE (Id uniqueidentifier, CategoryName nvarchar(255), RatePerHour decimal(18,2), ContractId uniqueidentifier)");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
