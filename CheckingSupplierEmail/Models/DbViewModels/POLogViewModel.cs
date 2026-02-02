using System;

namespace CheckingSupplierEmail.Models.DbViewModels
{
    public class POLogViewModel
    {
        public string PoNo { get; set; }
        public DateTime? SendDate { get; set; }
        public string SendBy { get; set; }
        public DateTime? ReadDate { get; set; }
        public string Status { get; set; }
    }
}
