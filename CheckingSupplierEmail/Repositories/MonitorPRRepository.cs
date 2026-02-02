using CheckingSupplierEmail.Models.DbViewModels;
using CheckingSupplierEmail.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CheckingSupplierEmail.Repositories
{
    public class MonitorPRRepository
    {
        private readonly DapperService _dapper;

        public MonitorPRRepository(DapperService dapper)
        {
            _dapper = dapper;
        }

        public async Task<(IEnumerable<MonitorPRViewModel> Data, int TotalRecords, int FilteredRecords)> GetMonitorPRData(
            int start, int length, string searchValue, string sortColumn, string sortDirection, string month = null)
        {
            // Date Filter Logic
            DateTime? startDate = null;
            DateTime? endDate = null;
            if (!string.IsNullOrEmpty(month))
            {
                if (DateTime.TryParseExact(month, "yyyy-MM", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
                {
                    startDate = new DateTime(parsedDate.Year, parsedDate.Month, 1);
                    endDate = startDate.Value.AddMonths(1);
                }
            }

            // Date Filter for Base Query
            string dateFilter = "";
            if (startDate.HasValue)
            {
                dateFilter = " AND PORequisition.PORQ_DateSubmitted >= @startDate AND PORequisition.PORQ_DateSubmitted < @endDate";
            }

            // Base query provided by user
            string baseQuery = $@"
                SELECT 
                    PORequisition.PORQ_RequisitionNumber,
                    SUM(PORequisitionLine.PORQL_TotalCost) AS ERPAmount,
                    PORequisition.PORQ_Notes,
                    PORequisition.PORQ_DateSubmitted,
                    PORequisition.PORQ_LastChangeDate,
                    PORequisition.PORQ_M_Department,
                    PORequisition.PORQ_M_Division,
                    PORequisition.PORQ_M_Remark,
                    PORequisition.PORQ_M_QuotationNo,
                    PORequisition.PORQ_M_ShipToDesc,
                    (ISNULL(e.EMP_FirstName, '') + ' ' + ISNULL(e.EMP_LastName, '')) AS LastChangeBy
                FROM PORequisition
                INNER JOIN PORequisitionLine
                    ON PORequisition.PORQ_RecordID = PORequisitionLine.PORQL_PORQ_RecordID
                LEFT JOIN [dbo].[EMP] e 
                    ON PORequisition.PORQ_EMP_RecordID_ChangedBy = e.EMP_RecordID
                WHERE 1=1 {dateFilter}
                GROUP BY 
                    PORequisition.PORQ_RequisitionNumber,
                    PORequisition.PORQ_Notes,
                    PORequisition.PORQ_DateSubmitted,
                    PORequisition.PORQ_LastChangeDate,
                    PORequisition.PORQ_M_Department,
                    PORequisition.PORQ_M_Division,
                    PORequisition.PORQ_M_Remark,
                    PORequisition.PORQ_M_QuotationNo,
                    PORequisition.PORQ_M_ShipToDesc,
                    e.EMP_FirstName,
                    e.EMP_LastName";

            // Default sorting
            if (string.IsNullOrEmpty(sortColumn))
            {
                sortColumn = "PORQ_LastChangeDate";
                sortDirection = "DESC";
            }

            // Build Where Clause
            string whereClause = "";
            if (!string.IsNullOrEmpty(searchValue))
            {
                whereClause += @" AND (
                    PORQ_RequisitionNumber LIKE @searchValue OR 
                    PORQ_Notes LIKE @searchValue OR
                    PORQ_M_Department LIKE @searchValue OR
                    PORQ_M_Division LIKE @searchValue OR 
                    PORQ_M_Remark LIKE @searchValue OR
                    PORQ_M_QuotationNo LIKE @searchValue OR
                    PORQ_M_ShipToDesc LIKE @searchValue
                )";
            }

            // CTE with ROW_NUMBER for paging (Compatible with older SQL Server)
            string sql = $@"
                WITH CTE_Data AS (
                    {baseQuery}
                ),
                CTE_RowNumber AS (
                    SELECT *, ROW_NUMBER() OVER (ORDER BY {sortColumn} {sortDirection}) AS RowNum
                    FROM CTE_Data
                    WHERE 1=1 {whereClause}
                )
                SELECT * FROM CTE_RowNumber
                WHERE RowNum BETWEEN @start + 1 AND @start + @length
                OPTION (RECOMPILE)";

            // Total Count (All)
            string totalRecordsSql = $@"
                WITH CTE_Data AS (
                    {baseQuery}
                )
                SELECT COUNT(*) FROM CTE_Data OPTION (RECOMPILE)";

            // Filtered Count
            string filteredRecordsSql = $@"
                WITH CTE_Data AS (
                    {baseQuery}
                )
                SELECT COUNT(*) FROM CTE_Data WHERE 1=1 {whereClause} OPTION (RECOMPILE)";

            var p = new
            {
                start,
                length,
                searchValue = $"%{searchValue}%",
                startDate,
                endDate
            };

            var data = await _dapper.Query<MonitorPRViewModel>("E", sql, p, commandTimeout: 0);
            
            // Fetch UICT2 Amount
            var dataList = data.ToList();
            if (dataList.Any())
            {
                var prNumbers = dataList.Select(x => x.PORQ_RequisitionNumber).Distinct().ToList();
                // Dapper IN clause support
                string uictSql = "SELECT prno, amount FROM [UICT2].[dbo].[pur_po] WHERE prno IN @prNumbers";
                
                // Use "2" for UICT2
                var uictData = await _dapper.Query<dynamic>("2", uictSql, new { prNumbers });
                
                foreach (var item in dataList)
                {
                    var uictItem = uictData.FirstOrDefault(x => x.prno == item.PORQ_RequisitionNumber);
                    if (uictItem != null)
                    {
                        item.UICTAmount = (decimal?)uictItem.amount;
                    }
                }
            }

            int totalRecords = await _dapper.ExecuteScalar<int>("E", totalRecordsSql, p, commandTimeout: 0);
            
            int filteredRecords;
            if (string.IsNullOrEmpty(searchValue))
            {
                filteredRecords = totalRecords;
            }
            else
            {
                filteredRecords = await _dapper.ExecuteScalar<int>("E", filteredRecordsSql, p, commandTimeout: 0);
            }

            return (dataList, totalRecords, filteredRecords);
        }
        public async Task UpdatePRAmountAsync(List<string> prNumbers, string updateUser)
        {
            if (prNumbers == null || !prNumbers.Any()) return;

            // Fetch current ERP Amount for these PRs to ensure accuracy
            string erpQuery = @"
                SELECT 
                    PORequisition.PORQ_RequisitionNumber AS PRNo,
                    SUM(PORequisitionLine.PORQL_TotalCost) AS ERPAmount
                FROM PORequisition
                INNER JOIN PORequisitionLine
                    ON PORequisition.PORQ_RecordID = PORequisitionLine.PORQL_PORQ_RecordID
                WHERE PORequisition.PORQ_RequisitionNumber IN @prNumbers
                GROUP BY PORequisition.PORQ_RequisitionNumber";

            var erpAmounts = await _dapper.Query<dynamic>("E", erpQuery, new { prNumbers });

            // Prepare update query
            string updateQuery = @"
                UPDATE [dbo].[pur_po]
                   SET [amount] = @Amount
                      ,[updatedate] = GETDATE()
                      ,[updateuser] = @UpdateUser
                 WHERE [prno] = @PrNo";

            foreach (var item in erpAmounts)
            {
                await _dapper.Execute("2", updateQuery, new { Amount = item.ERPAmount, PrNo = item.PRNo, UpdateUser = updateUser });
            }
        }
    }
}
