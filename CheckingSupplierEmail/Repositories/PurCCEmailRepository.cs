using CheckingSupplierEmail.Models.DbModels;
using CheckingSupplierEmail.Models.DbViewModels;
using CheckingSupplierEmail.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CheckingSupplierEmail.Repositories
{
    public class PurCCEmailRepository
    {
        private readonly DapperService _dapper;
        private readonly string _tableName;

        public PurCCEmailRepository(DapperService dapper)
        {
            _dapper = dapper;
            _tableName = "pur_ccemail";
        }

        public async Task<IEnumerable<vw_PUR_CCEmail>> GetAll()
        {
            string sql = $@"SELECT * FROM [vw_PUR_CCEmail]
 ORDER BY [username] ASC, [type] DESC";
            return await _dapper.Query<vw_PUR_CCEmail>("1", sql);
        }

        public async Task<IEnumerable<vw_PUR_CCEmail>> UpdateUsername(string txt_oldusername, string txt_newusername, string txt_user)
        {
            string sql = $@"UPDATE [pur_ccemail] SET username = @txt_newusername,
updateuser = @txt_user,
updatedate = GETDATE()
WHERE username = @txt_oldusername";
            return await _dapper.Query<vw_PUR_CCEmail>("1", sql, new { txt_oldusername, txt_newusername, txt_user });
        }

        public async Task<bool> RemoveUsername(string txt_username, string txt_user)
        {
            string sql = $@"UPDATE [pur_ccemail] SET statusno = 'X',
updateuser = @txt_user,
updatedate = GETDATE()
WHERE username = @txt_username";
            await _dapper.Execute("1", sql, new { txt_username, txt_user });
            return true;
        }

        public async Task<bool> Add(string txt_username, string txt_type, string txt_value, string txt_user)
        {
            PUR_CCEmail model = new PUR_CCEmail
            {
                username = txt_username,
                type = txt_type,
                value = txt_value,
                statusno = "A",
                credate = DateTime.Now,
                updatedate = DateTime.Now,
                creuser = txt_user,
                updateuser = txt_user
            };
            await _dapper.Insert<PUR_CCEmail>("1", model);
            return true;
        }

        public async Task<bool> CheckDuplicateEmail(string txt_username, string txt_email, int txt_id = 0)
        {
            string sql = $@"SELECT [id] FROM [{_tableName}] 
WHERE [id] <> @txt_id 
AND [type] = 'EMAIL' 
AND [username] = @txt_username 
AND [value] = @txt_email
AND [statusno] = 'A'";
            var obj_email = await _dapper.QueryFirst<PUR_CCEmail>("1", sql, new { txt_username, txt_email, txt_id });
            return obj_email != null;
        }

        public async Task<bool> CheckDuplicateEmpno(string txt_username, string txt_empno, int txt_id = 0)
        {
            string sql = $@"SELECT [id] FROM [{_tableName}] 
WHERE [id] <> @txt_id 
AND [type] = 'EMPNO' 
AND [username] = @txt_username 
AND [value] = @txt_empno
AND [statusno] = 'A'";
            var obj_email = await _dapper.QueryFirst<PUR_CCEmail>("1", sql, new { txt_username, txt_empno, txt_id });
            return obj_email != null;
        }

        public async Task<bool> Delete(int txt_id, string txt_user)
        {
            string sql = $@"UPDATE [{_tableName}] SET [statusno] = 'X', updatedate = GETDATE(), updateuser = @txt_user WHERE [id] = @txt_id";
            await _dapper.Execute("1", sql, new { txt_id, txt_user });
            return true;
        }

        public async Task<bool> Update(int txt_id, string txt_email, string txt_user)
        {
            string sql = $@"UPDATE [{_tableName}] SET [value] = @txt_email, updateuser = @txt_user, updatedate = GETDATE() WHERE [id] = @txt_id";
            await _dapper.Execute("1", sql, new { txt_email, txt_id, txt_user });
            return true;
        }
    }
}
