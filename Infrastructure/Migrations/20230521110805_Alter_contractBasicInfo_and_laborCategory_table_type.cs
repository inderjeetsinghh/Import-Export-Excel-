using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Alter_contractBasicInfo_and_laborCategory_table_type : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS spSaveContractWithCategories");
            migrationBuilder.Sql("DROP TYPE dbo.ContractBasicInfoData");
            migrationBuilder.Sql("DROP TYPE dbo.LaborCategoryData");

            migrationBuilder.Sql(@"
            CREATE TYPE dbo.ContractBasicInfoData AS TABLE
            (
                Id NVARCHAR(36),
                ContractName NVARCHAR(max),
                ClientName NVARCHAR(max),
                StartDate DATETIME,
                EndDate DATETIME
            );
        ");

            migrationBuilder.Sql(@"
            CREATE TYPE dbo.LaborCategoryData AS TABLE
            (
                Id NVARCHAR(36),
                CategoryName NVARCHAR(max),
                RatePerHour DECIMAL(18,2),
                ContractId NVARCHAR(36)
            );
        ");

            migrationBuilder.Sql(@"
        CREATE PROCEDURE spSaveContractWithCategories
            @ContractBasicInfoData dbo.ContractBasicInfoData READONLY,
            @LaborCategoryData dbo.LaborCategoryData READONLY,
            @ErrorMessage NVARCHAR(MAX) OUTPUT
        AS
        BEGIN
            BEGIN TRY
                BEGIN TRANSACTION;

                -- Insert ContractBasicInfo records
                INSERT INTO ContractBasicInfo (Id, ContractName, ClientName, StartDate, EndDate)
                SELECT Id, ContractName, ClientName, StartDate, EndDate
                FROM @ContractBasicInfoData;

                -- Insert LaborCategory records
                INSERT INTO LaborCategory (Id, CategoryName, RatePerHour, ContractId)
                SELECT Id, CategoryName, RatePerHour, ContractId
                FROM @LaborCategoryData;

                COMMIT;

                -- Set success message
                SET @ErrorMessage = 'Data saved successfully.';
            END TRY
            BEGIN CATCH
                -- Rollback transaction in case of error
                IF @@TRANCOUNT > 0
                    ROLLBACK;

                -- Set error message
                SET @ErrorMessage = ERROR_MESSAGE();
            END CATCH;
        END
    ");
        }
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS spSaveContractWithCategories");
            migrationBuilder.Sql("DROP TYPE dbo.ContractBasicInfoData");
            migrationBuilder.Sql("DROP TYPE dbo.LaborCategoryData");
        }
    }
}
