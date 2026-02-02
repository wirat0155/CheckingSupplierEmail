using System;

namespace CheckingSupplierEmail.Models.DbViewModels
{
    public class PODetailViewModel
    {
        public string PoNo { get; set; }
        public string VendorId { get; set; }
        public string VendorName { get; set; }
        public int LineNo { get; set; }
        public string ItemName { get; set; }
        public decimal UnitPrice { get; set; }
        public string Currency { get; set; }
        public decimal Qty { get; set; }
        public string Unit { get; set; }
        public decimal? ReceiptQty { get; set; }
        public DateTime? ReceiptDate { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string InvoiceNo { get; set; }
        public string ReceiptStatus { get; set; }
    }
}
