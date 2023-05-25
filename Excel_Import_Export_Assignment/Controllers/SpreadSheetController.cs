using Domain.Entity;
using Domain.Models;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.IIS.Core;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Data;
using System.IO;

namespace Excel_Import_Export_Assignment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpreadSheetController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public SpreadSheetController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public ActionResult<GenericBaseResult<bool>> UploadFile(IFormFile file)
        {
            try
            {
                if (file == null)
                    throw new Exception("No files to process");

                //Read contracts and laborCategory data
                var (contractList, laborCategoryList) = ReadSpreadSheetsData(file);

                var contractBasicInfoDataTable = CreateContractBasicInfoDataTable(contractList);
                var laborCategoryDataTable = CreateLaborCategoryDataTable(laborCategoryList);

                // Execute the stored procedure
                var errorMessageParameter = new SqlParameter("@ErrorMessage", SqlDbType.NVarChar, -1)
                {
                    Direction = ParameterDirection.Output
                };

                _context.Database.ExecuteSqlRaw("EXEC spSaveContractWithCategories @ContractBasicInfoData, @LaborCategoryData, @ErrorMessage OUTPUT",
                 new SqlParameter() { SqlDbType = SqlDbType.Structured, TypeName = "dbo.ContractBasicInfoData", ParameterName = "@ContractBasicInfoData", Value = contractBasicInfoDataTable },
                 new SqlParameter() { SqlDbType = SqlDbType.Structured, TypeName = "dbo.LaborCategoryData", ParameterName = "@LaborCategoryData", Value = laborCategoryDataTable },
                 errorMessageParameter);

                var errorMessage = errorMessageParameter.Value.ToString();
                if (!string.IsNullOrEmpty(errorMessage) && errorMessage.Equals("Data saved successfully."))
                {
                    // Data saved successfully
                    return new GenericBaseResult<bool>(true)
                    {
                        Message = "Data saved successfully."
                    };
                }
                else
                {
                    // Error occurred while saving data
                    throw new Exception("Something went wrong, please check your excel file!");
                }
            }
            catch (Exception ex)
            {
                var result = new GenericBaseResult<bool>(false);
                result.AddExceptionLog(ex);
                result.Message = ex.Message;
                return result;
            }
        }

        [HttpGet]
        public IActionResult GetContracts()
        {
            try
            {
                var contractBasicInfos = _context.ContractBasicInfo.Include(c => c.LaborCategories).Select(x => new ContractBasicInfoModel
                {
                    Id = x.Id,
                    ContractName = x.ContractName,
                    ClientName = x.ClientName,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    LaborCategories = x.LaborCategories.Select(y => new LaborCategoryModel { Id = y.Id, CategoryName = y.CategoryName, RatePerHour = y.RatePerHour, ContractId = y.ContractId }).ToList()
                }).ToList();
                return Ok(new { ContractBasicInfos = contractBasicInfos });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while fetching contracts.");
            }
        }

        [HttpPost("exportData")]
        public ActionResult<GenericBaseResult<bool>> ExportData()
        {
            try
            {
                var contractsResult = _context.ContractBasicInfo.Include(x => x.LaborCategories).ToList();

                // Create a new Excel workbook
                IWorkbook workbook = new XSSFWorkbook();

                // Create the worksheet for ContractBasicInfo data
                ISheet contractSheet = workbook.CreateSheet("ContractBasicInfo");

                // Create the worksheet for LaborCategory data
                ISheet laborCategorySheet = workbook.CreateSheet("LaborCategory");

                // Create the headers for ContractBasicInfo
                IRow contractHeaderRow = contractSheet.CreateRow(0);
                contractHeaderRow.CreateCell(0).SetCellValue("Id");
                contractHeaderRow.CreateCell(1).SetCellValue("Contract Name");
                contractHeaderRow.CreateCell(2).SetCellValue("Client Name");
                contractHeaderRow.CreateCell(3).SetCellValue("Start Date");
                contractHeaderRow.CreateCell(4).SetCellValue("End Date");

                // Populate the ContractBasicInfo data
                int rowIndex = 1;
                foreach (var contract in contractsResult)
                {
                    IRow row = contractSheet.CreateRow(rowIndex);
                    row.CreateCell(0).SetCellValue(contract.Id);
                    row.CreateCell(1).SetCellValue(contract.ContractName);
                    row.CreateCell(2).SetCellValue(contract.ClientName);
                    row.CreateCell(3).SetCellValue(contract.StartDate.ToString());
                    row.CreateCell(4).SetCellValue(contract.EndDate.ToString());

                    rowIndex++;
                }


                // Create the headers for LaborCategory
                IRow laborCategoryHeaderRow = laborCategorySheet.CreateRow(0);
                laborCategoryHeaderRow.CreateCell(0).SetCellValue("Id");
                laborCategoryHeaderRow.CreateCell(1).SetCellValue("Category Name");
                laborCategoryHeaderRow.CreateCell(2).SetCellValue("Rate Per Hour");
                laborCategoryHeaderRow.CreateCell(3).SetCellValue("ContractId");

                // Populate the LaborCategory data
                int rowIndex2 = 1;
                foreach (var laborCategory in contractsResult.SelectMany(x => x.LaborCategories))
                {
                    IRow row = laborCategorySheet.CreateRow(rowIndex2);
                    row.CreateCell(0).SetCellValue(laborCategory.Id);
                    row.CreateCell(1).SetCellValue(laborCategory.CategoryName);
                    row.CreateCell(2).SetCellValue((double)laborCategory.RatePerHour);
                    row.CreateCell(3).SetCellValue(laborCategory.ContractId);

                    rowIndex2++;
                }

                // Save the workbook to a file
                string fileName = "ExportedContracts.xlsx";
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);


                //string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "ExportedContracts.xlsx");
                using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    workbook.Write(fileStream, true);
                }

                // Data exported successfully
                return new GenericBaseResult<bool>(true)
                {
                    Message = "Exported successfully, Check file ExportedContracts.xlsx in MyDocuments"
                };
            }
            catch(Exception ex)
            {
                var result = new GenericBaseResult<bool>(false);
                result.AddExceptionLog(ex);
                result.Message = "Something went wrong while exporting data!";
                return result;
            }

        }

        private (List<ContractBasicInfo>, List<LaborCategory>) ReadSpreadSheetsData(IFormFile file)
        {
            var contractsList = new List<ContractBasicInfo>();
            var laborCategoryList = new List<LaborCategory>();

            // Open the Excel file using NPOI
            using (var fileStream = file.OpenReadStream())
            {
                IWorkbook workbook;
                workbook = new XSSFWorkbook(fileStream);

                // Access the contracts worksheet
                ISheet worksheet1 = workbook.GetSheetAt(0);
                int rowCount = worksheet1.PhysicalNumberOfRows;
                int colCount = worksheet1.GetRow(0).PhysicalNumberOfCells;

                // Iterate through the contracts
                for (int row = 1; row < rowCount; row++) // Assuming the first row is a header row, so starting from row 1
                {
                    string? id = worksheet1.GetRow(row).GetCell(0)?.ToString();
                    string? contractName = worksheet1.GetRow(row).GetCell(1)?.ToString();
                    string? clientName = worksheet1.GetRow(row).GetCell(2)?.ToString();
                    string? startDate = worksheet1.GetRow(row).GetCell(3)?.ToString();
                    string? endDate = worksheet1.GetRow(row).GetCell(4)?.ToString();

                    // Create an instance of the ContractBasicInfo class and populate its properties
                    var contractBasicInfo = new ContractBasicInfo
                    {
                        Id = id,
                        ContractName = contractName,
                        ClientName = clientName,
                        StartDate = DateTime.Parse(startDate),
                        EndDate = DateTime.Parse(endDate),
                    };

                    // Add the record to the contracts list
                    contractsList.Add(contractBasicInfo);
                }

                // Access the laborCategory worksheet
                ISheet worksheet2 = workbook.GetSheetAt(1);
                int rowCount2 = worksheet2.PhysicalNumberOfRows;
                int colCount2 = worksheet2.GetRow(0).PhysicalNumberOfCells;

                // Iterate through the laborCategories
                for (int row = 1; row < rowCount2; row++) // Assuming the first row is a header row, so starting from row 1
                {
                    string? id = worksheet2.GetRow(row).GetCell(0)?.ToString();
                    string? categoryName = worksheet2.GetRow(row).GetCell(1)?.ToString();
                    decimal ratePerHour = decimal.Parse(worksheet2.GetRow(row).GetCell(2)?.ToString());
                    string? contractId = worksheet2.GetRow(row).GetCell(3)?.ToString();

                    // Create an instance of the LaborCategory class and populate its properties
                    var laborCategory = new LaborCategory
                    {
                        Id = id,
                        CategoryName = categoryName,
                        RatePerHour = ratePerHour,
                        ContractId = contractId
                    };

                    // Add the record to the laborCategory list
                    laborCategoryList.Add(laborCategory);
                }
            }

            return (contractsList, laborCategoryList);
        }


        // Helper methods for creating DataTables
        private DataTable CreateContractBasicInfoDataTable(List<ContractBasicInfo> contractBasicInfos)
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("Id", typeof(Guid));
            dataTable.Columns.Add("ContractName", typeof(string));
            dataTable.Columns.Add("ClientName", typeof(string));
            dataTable.Columns.Add("StartDate", typeof(DateTime));
            dataTable.Columns.Add("EndDate", typeof(DateTime));

            foreach (var contractBasicInfo in contractBasicInfos)
            {
                dataTable.Rows.Add(contractBasicInfo.Id, contractBasicInfo.ContractName, contractBasicInfo.ClientName, contractBasicInfo.StartDate, contractBasicInfo.EndDate);
            }

            return dataTable;
        }

        private DataTable CreateLaborCategoryDataTable(List<LaborCategory> laborCategories)
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("Id", typeof(Guid));
            dataTable.Columns.Add("CategoryName", typeof(string));
            dataTable.Columns.Add("RatePerHour", typeof(decimal));
            dataTable.Columns.Add("ContractId", typeof(Guid));

            foreach (var laborCategory in laborCategories)
            {
                dataTable.Rows.Add(laborCategory.Id, laborCategory.CategoryName, laborCategory.RatePerHour, laborCategory.ContractId);
            }

            return dataTable;
        }
    }
}
