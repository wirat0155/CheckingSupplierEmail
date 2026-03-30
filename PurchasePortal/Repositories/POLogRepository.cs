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
            int orderColumn = 2,
            string orderDir = "desc")
        {
            var response = new DataTableResponse<POLogViewModel> { Draw = draw };

            // Default to today if no date provided
            if (!startDate.HasValue) startDate = DateTime.Today;
            if (!endDate.HasValue) endDate = DateTime.Today;

            // Adjust endDate to include the full day (23:59:59)
            var endDateTime = endDate.Value.Date.AddDays(1).AddSeconds(-1);

            // Count total records for the date range and status before search filtering
            string countSql = "SELECT COUNT(*) FROM [UICT].[dbo].[pur_polog] p WHERE (p.send_vendor_date BETWEEN @startDate AND @endDateTime)";
            if (!string.IsNullOrEmpty(status) && status != "ALL")
                countSql += " AND p.[statusno] = @status";
            
            response.RecordsTotal = await _dapper.ExecuteScalarAsync<int>("1", countSql, new { startDate, endDateTime, status });

            // Fetch all logs within the filter from UICT
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
                sql += " AND p.[statusno] = @status";

            var logs = (await _dapper.Query<POLogViewModel>("1", sql, new { startDate, endDateTime, status })).ToList();

            // Fetch Vendor Name and PR Number from ERP for these logs
            var distinctPoNos = logs.Select(l => l.PoNo).Distinct().ToList();
            if (distinctPoNos.Any())
            {
                // To avoid large IN clause issues, we could batch but let's assume range is reasonable
                var poListStrings = distinctPoNos.Select(id => "'" + id.Replace("'", "''") + "'");
                var poList = string.Join(",", poListStrings);
                
                string erpSql = $@"
                    SELECT DISTINCT POM_PurchorderID AS PoNo, POM_VendorName AS VendorName, PRNbr AS PrNo
                    FROM [iERP85].[dbo].[vw_mfc_rptPOPrint]
                    WHERE POM_PurchorderID IN ({poList})
                      AND PRNbr IS NOT NULL AND POM_VendorID IS NOT NULL";
                
                var erpData = (await _dapper.Query<POLogViewModel>("E", erpSql))
                    .GroupBy(x => x.PoNo)
                    .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

                foreach (var log in logs)
                {
                    if (erpData.TryGetValue(log.PoNo, out var erp))
                    {
                        log.VendorName = erp.VendorName;
                        log.PrNo = erp.PrNo ?? "-";
                    }
                    else
                    {
                        log.PrNo = "-";
                    }
                }
            }

            // Apply search filter in memory
            var filteredLogs = logs.AsQueryable();
            if (!string.IsNullOrEmpty(searchValue))
            {
                var lowerSearch = searchValue.ToLower();
                filteredLogs = filteredLogs.Where(l => 
                    (l.PoNo != null && l.PoNo.ToLower().Contains(lowerSearch)) ||
                    (l.PrNo != null && l.PrNo.ToLower().Contains(lowerSearch)) ||
                    (l.VendorName != null && l.VendorName.ToLower().Contains(lowerSearch)) ||
                    (l.SendBy != null && l.SendBy.ToLower().Contains(lowerSearch)) ||
                    (l.Status != null && l.Status.ToLower().Contains(lowerSearch))
                );
            }

            response.RecordsFiltered = filteredLogs.Count();

            // Apply sorting in memory
            bool isAsc = orderDir?.ToLower() == "asc";
            filteredLogs = orderColumn switch
            {
                1 => isAsc ? filteredLogs.OrderBy(l => l.PoNo) : filteredLogs.OrderByDescending(l => l.PoNo),
                2 => isAsc ? filteredLogs.OrderBy(l => l.SendDate) : filteredLogs.OrderByDescending(l => l.SendDate),
                3 => isAsc ? filteredLogs.OrderBy(l => l.VendorName) : filteredLogs.OrderByDescending(l => l.VendorName),
                4 => isAsc ? filteredLogs.OrderBy(l => l.SendBy) : filteredLogs.OrderByDescending(l => l.SendBy),
                5 => isAsc ? filteredLogs.OrderBy(l => l.ReadDate) : filteredLogs.OrderByDescending(l => l.ReadDate),
                6 => isAsc ? filteredLogs.OrderBy(l => l.Status) : filteredLogs.OrderByDescending(l => l.Status),
                _ => filteredLogs.OrderByDescending(l => l.SendDate)
            };

            // Apply pagination
            response.Data = filteredLogs.Skip(start).Take(length).ToList();

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

            // Get unique PO numbers to fetch vendor names and PR numbers from ERP
            var poNos = logs.Select(l => l.PoNo).Distinct().ToList();
            if (poNos.Any())
            {
                var poList = string.Join("','", poNos);
                string vendorSql = $@"
                    SELECT DISTINCT POM_PurchorderID AS PoNo, POM_VendorName AS VendorName, PRNbr AS PrNo
                    FROM [iERP85].[dbo].[vw_mfc_rptPOPrint]
                    WHERE POM_PurchorderID IN ('{poList}')";
                
                var poData = (await _dapper.Query<POLogViewModel>("E", vendorSql))
                    .GroupBy(p => p.PoNo)
                    .ToDictionary(g => g.Key, g => g.First());
                
                foreach (var log in logs)
                {
                    if (poData.TryGetValue(log.PoNo, out var detail))
                    {
                        log.VendorName = detail.VendorName;
                        log.PrNo = detail.PrNo ?? "-";
                    }
                    else
                    {
                        log.PrNo = "-";
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
