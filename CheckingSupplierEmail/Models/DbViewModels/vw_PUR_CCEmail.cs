using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CheckingSupplierEmail.Models.DbViewModels
{
    public class vw_PUR_CCEmail
    {
        public int id { get; set; }
        public string username { get; set; }
        public string type { get; set; }
        public string value { get; set; }
        public string? userempnameeng { get; set; }
        public string? userdepartmentnameeng {get; set;} 
        public string? useremailaccount { get; set; }
        public string? userempstatusno { get; set; }
        public string? ccempnameeng { get; set; }
        public string? ccdepartmentnameeng { get; set; }
        public string? ccemailaccount { get; set; }
        public string? ccempstatusno { get; set; }
    }
}
