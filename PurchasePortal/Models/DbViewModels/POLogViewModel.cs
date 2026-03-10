using System;

namespace PurchasePortal.Models.DbViewModels
{
    public class POLogViewModel
    {
        public string PoNo { get; set; }
        public DateTime? SendDate { get; set; }
        public string SendBy { get; set; }
        public DateTime? ReadDate { get; set; }
        public string Status { get; set; }
        public string VendorName { get; set; }
    }
}
