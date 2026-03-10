using PurchasePortal.Models.DbViewModels;
using PurchasePortal.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PurchasePortal.Repositories
{
    public class POLogRepository
    {
        private readonly DapperService _dapper;

        public POLogRepository(DapperService dapper)
        {
            _dapper = dapper;
        }

        public async Task<DataTableResponse<POLogViewModel>> GetPOLogsDataTable(
            DateTime? startDate = null, 
            DateTime? endDate = null, 
            string? status = null,
            int start = 0, 
            int length = 10, 
            int draw = 1,
            string? searchValue = null,
            int orderColumn = 1,
            string orderDir = "desc")
        {
            var response = new DataTableResponse<POLogViewModel> { Draw = draw };

            // Default to today if no date provided
            if (!startDate.HasValue) startDate = DateTime.Today;
            if (!endDate.HasValue) endDate = DateTime.Today;

            // Adjust endDate to include the full day (23:59:59)
            var endDateTime = endDate.Value.Date.AddDays(1).AddSeconds(-1);

            // Count total records before filtering
            string countSql = @"
SELECT COUNT(*)
FROM [UICT].[dbo].[pur_polog] p
WHERE (p.send_vendor_date BETWEEN @startDate AND @endDateTime)";

            if (!string.IsNullOrEmpty(status) && status != "ALL")
            {
                countSql += " AND p.[statusno] = @status";
            }

            response.RecordsTotal = await _dapper.ExecuteScalarAsync<int>("1", countSql, new { startDate, endDateTime, status });

            // Build main query with filters
            string sql = @"
SELECT 
    p.[por_purchorderid] AS 'PoNo',
    p.[send_vendor_date] AS 'SendDate',
    p.[send_by] AS 'SendBy',
    p.[vendor_read_date] AS 'ReadDate',
    p.[statusno] AS 'Status'
FROM [UICT].[dbo].[pur_polog] p
WHERE (p.send_vendor_date BETWEEN @startDate AND @endDateTime)";

            if (!string.IsNullOrEmpty(status) && status != "ALL")
            {
                sql += " AND p.[statusno] = @status";
            }

            // Add search filter
            if (!string.IsNullOrEmpty(searchValue))
            {
                sql += @" AND (
                    p.[por_purchorderid] LIKE @searchValue OR 
                    p.[send_by] LIKE @searchValue OR
                    p.[statusno] LIKE @searchValue
                )";
            }

            // Get filtered count
            string filteredCountSql = "SELECT COUNT(*) FROM (" + sql + ") AS filtered";
            response.RecordsFiltered = await _dapper.ExecuteScalarAsync<int>("1", filteredCountSql, 
                new { startDate, endDateTime, status, searchValue = $"%{searchValue}%" });

            // Add ordering
            string orderColumnName = orderColumn switch
            {
                0 => "p.[por_purchorderid]",
                1 => "p.[send_vendor_date]",
                2 => "p.[send_by]",
                3 => "p.[vendor_read_date]",
                4 => "p.[statusno]",
                _ => "p.[send_vendor_date]"
            };
            sql += $" ORDER BY {orderColumnName} {orderDir} OFFSET @start ROWS FETCH NEXT @length ROWS ONLY";

            var logs = (await _dapper.Query<POLogViewModel>("1", sql, 
                new { startDate, endDateTime, status, searchValue = $"%{searchValue}%", start, length })).ToList();

            // Get unique PO numbers to fetch vendor names from ERP
            var poNos = logs.Select(l => l.PoNo).Distinct().ToList();
            if (poNos.Any())
            {
                var poList = string.Join("','", poNos);
                string vendorSql = $@"
                    SELECT DISTINCT POM_PurchorderID AS PoNo, POM_VendorName AS VendorName
                    FROM [iERP85].[dbo].[vw_mfc_rptPOPrint]
                    WHERE POM_PurchorderID IN ('{poList}')";
                
                var vendors = (await _dapper.Query<POLogViewModel>("E", vendorSql)).ToDictionary(v => v.PoNo, v => v.VendorName);
                
                foreach (var log in logs)
                {
                    if (vendors.TryGetValue(log.PoNo, out var vendorName))
                    {
                        log.VendorName = vendorName;
                    }
                }
            }

            response.Data = logs;
            return response;
        }

        public async Task<IEnumerable<POLogViewModel>> GetPOLogs(DateTime? startDate = null, DateTime? endDate = null, string? status = null)
        {
            // Default to today if no date provided
            if (!startDate.HasValue) startDate = DateTime.Today;
            if (!endDate.HasValue) endDate = DateTime.Today;

            // Adjust endDate to include the full day (23:59:59)
            var endDateTime = endDate.Value.Date.AddDays(1).AddSeconds(-1);

            string sql = @"
SELECT 
    p.[por_purchorderid] AS 'PoNo',
    p.[send_vendor_date] AS 'SendDate',
    p.[send_by] AS 'SendBy',
    p.[vendor_read_date] AS 'ReadDate',
    p.[statusno] AS 'Status'
FROM [UICT].[dbo].[pur_polog] p
WHERE (p.send_vendor_date BETWEEN @startDate AND @endDateTime)";

            if (!string.IsNullOrEmpty(status))
            {
                if (status == "ALL")
                {
                   // No filter on status
                }
                else
                {
                   sql += " AND p.[statusno] = @status";
                }
            }
            
            sql += " ORDER BY p.send_vendor_date DESC";

            var logs = (await _dapper.Query<POLogViewModel>("1", sql, new { startDate, endDateTime, status })).ToList();

            // Get unique PO numbers to fetch vendor names from ERP
            var poNos = logs.Select(l => l.PoNo).Distinct().ToList();
            if (poNos.Any())
            {
                var poList = string.Join("','", poNos);
                string vendorSql = $@"
                    SELECT DISTINCT POM_PurchorderID AS PoNo, POM_VendorName AS VendorName
                    FROM [iERP85].[dbo].[vw_mfc_rptPOPrint]
                    WHERE POM_PurchorderID IN ('{poList}')";
                
                var vendors = (await _dapper.Query<POLogViewModel>("E", vendorSql)).ToDictionary(v => v.PoNo, v => v.VendorName);
                
                foreach (var log in logs)
                {
                    if (vendors.TryGetValue(log.PoNo, out var vendorName))
                    {
                        log.VendorName = vendorName;
                    }
                }
            }

            return logs;
        }
        public async Task<IEnumerable<PODetailViewModel>> GetPODetails(string poNo)
        {
            string sql = @"
SELECT 
    [POM_PurchorderID] AS 'PoNo',
    [POM_VendorID] AS 'VendorId',
    [POM_VendorName] AS 'VendorName',
    [POI_POLineNbr] AS 'LineNo',
    [POI_ItemName] AS 'ItemName',
    [POD_POUnitPrice] AS 'UnitPrice',
    [POM_APCurrencyType] AS 'Currency',
    [POD_RequiredQty] AS 'Qty',
    [POI_PurConvUnitMeasure] AS 'Unit',
    [RCP_ReceiptQty] AS 'ReceiptQty',
    [rcp_receiverdate] AS 'ReceiptDate',
    [rcp_m_invoicedate] AS 'InvoiceDate',
    [rcp_vendorpackslipid] AS 'InvoiceNo',
    rcp_ontimestatus AS 'ReceiptStatus'
FROM [iERP85].[dbo].[vw_mfc_rptPOPrint] po
LEFT JOIN rcp
    ON rcp.rcp_purchorderid = po.[POM_PurchorderID]
    AND rcp.rcp_polinenbr = po.POI_POLineNbr
WHERE [POM_PurchorderID] = @poNo
ORDER BY POI_POLineNbr ASC";

            // Use "p" or "E" or whatever maps to the ERP connection.
            // DapperService.cs says: if (dbCharacter == "E") -> ConnectionStrings:ERP
            return await _dapper.Query<PODetailViewModel>("E", sql, new { poNo });
        }
    }
}
