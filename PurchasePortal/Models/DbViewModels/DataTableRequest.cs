namespace PurchasePortal.Models.DbViewModels
{
    public class DataTableRequest
    {
        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public SearchData? Search { get; set; }
        public OrderData[]? Order { get; set; }
        public ColumnData[]? Columns { get; set; }
    }

    public class SearchData
    {
        public string? Value { get; set; }
        public bool Regex { get; set; }
    }

    public class OrderData
    {
        public int Column { get; set; }
        public string Dir { get; set; } = "asc";
    }

    public class ColumnData
    {
        public string? Data { get; set; }
        public string? Name { get; set; }
        public bool Searchable { get; set; }
        public bool Orderable { get; set; }
        public SearchData? Search { get; set; }
    }
}