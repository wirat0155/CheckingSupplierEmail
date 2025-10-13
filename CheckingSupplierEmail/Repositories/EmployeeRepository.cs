using CheckingSupplierEmail.Models.DbViewModels;
using CheckingSupplierEmail.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CheckingSupplierEmail.Repositories
{
    public class EmployeeRepository
    {
        private readonly DapperService _dapper;

        public EmployeeRepository(
            DapperService dapper)
        {
            _dapper = dapper;
        }

        public async Task<vw_emp> GetByEmpno(string txt_empno)
        {
            string sql = $@"SELECT [empno], [empnameeng], [empstatusno] FROM [vw_emp] WHERE [empno] = @txt_empno";
            return await _dapper.QueryFirst<vw_emp>("1", sql, new { txt_empno });
        }
    }
}
