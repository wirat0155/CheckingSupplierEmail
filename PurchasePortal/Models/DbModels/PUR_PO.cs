using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PurchasePortal.Models.DbModels
{
    [Table("pur_po")]
    public class PUR_PO
    {
        [Key]
        public Guid id { get; set; }
        public string prno { get; set; }
        public decimal amount { get; set; }
        public string area { get; set; }
        public DateTime credate { get; set; }
        public string creuser { get; set; }
        public DateTime updatedate { get; set; }
        public string updateuser { get; set; }
        public bool convertpoflag { get; set; }
        public DateTime? convertpodate { get; set; }
    }
}
