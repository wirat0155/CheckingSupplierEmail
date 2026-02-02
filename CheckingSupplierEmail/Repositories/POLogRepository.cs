using CheckingSupplierEmail.Models.DbViewModels;
using CheckingSupplierEmail.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CheckingSupplierEmail.Repositories
{
    public class POLogRepository
    {
        private readonly DapperService _dapper;

        public POLogRepository(DapperService dapper)
        {
            _dapper = dapper;
        }

        public async Task<IEnumerable<POLogViewModel>> GetPOLogs(DateTime? startDate = null, DateTime? endDate = null, string status = null)
        {
            // Default to today if no date provided
            if (!startDate.HasValue) startDate = DateTime.Today;
            if (!endDate.HasValue) endDate = DateTime.Today;

            // Adjust endDate to include the full day (23:59:59)
            var endDateTime = endDate.Value.Date.AddDays(1).AddSeconds(-1);

            string sql = @"
SELECT 
    [por_purchorderid] AS 'PoNo',
    [send_vendor_date] AS 'SendDate',
    [send_by] AS 'SendBy',
    [vendor_read_date] AS 'ReadDate',
    [statusno] AS 'Status'
FROM [UICT].[dbo].[pur_polog]
WHERE (send_vendor_date BETWEEN @startDate AND @endDateTime)";

            if (!string.IsNullOrEmpty(status))
            {
                if (status == "ALL")
                {
                   // No filter on status
                }
                else
                {
                   sql += " AND [statusno] = @status";
                }
            }
            
            sql += " ORDER BY send_vendor_date DESC";

            return await _dapper.Query<POLogViewModel>("1", sql, new { startDate, endDateTime, status });
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
