using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CheckingSupplierEmail.Models.DbModels
{
    public class VEN
    {
        [Key]
        public string VEN_VendorID { get; set; }
        public string VEN_POName { get; set; }
        public string? VEN_POEmail { get; set; }
        public string? VEN_StatusCode { get; set; }

        [NotMapped]
        public string? Reason { get; set; }
    }
}
