using System;

namespace CheckingSupplierEmail.Models.DbViewModels
{
    public class MonitorPRViewModel
    {
        public string PORQ_RequisitionNumber { get; set; }
        public decimal? ERPAmount { get; set; }
        public decimal? UICTAmount { get; set; }
        public string PORQ_Notes { get; set; }
        public DateTime? PORQ_DateSubmitted { get; set; }
        public DateTime? PORQ_LastChangeDate { get; set; }
        public string PORQ_M_Department { get; set; }
        public string PORQ_M_Division { get; set; }
        public string PORQ_M_Remark { get; set; }
        public string LastChangeBy { get; set; }
        public string PONumber { get; set; }
        public string PORQ_M_QuotationNo { get; set; }
        public string PORQ_M_ShipToDesc { get; set; }
    }
}
