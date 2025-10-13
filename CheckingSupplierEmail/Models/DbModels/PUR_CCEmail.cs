using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CheckingSupplierEmail.Models.DbModels
{
    [Table("PUR_CCEmail")]
    public class PUR_CCEmail
    {
        [Key]
        public int id { get; set; }
        public string username { get; set; }
        public string type { get; set; }
        public string value { get; set; }
        public string statusno { get; set; }
        public DateTime credate { get; set; }
        public DateTime updatedate { get; set; }
        public string creuser { get; set; }
        public string updateuser { get; set; }
    }
}
