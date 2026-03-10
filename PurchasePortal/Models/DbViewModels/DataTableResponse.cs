using System.Collections.Generic;

namespace PurchasePortal.Models.DbViewModels
{
    public class DataTableResponse<T>
    {
        public int Draw { get; set; }
        public int RecordsTotal { get; set; }
        public int RecordsFiltered { get; set; }
        public List<T> Data { get; set; } = new List<T>();
        public string? Error { get; set; }
    }
}