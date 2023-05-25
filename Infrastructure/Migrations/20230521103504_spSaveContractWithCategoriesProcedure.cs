using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class spSaveContractWithCategoriesProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

    }
}
