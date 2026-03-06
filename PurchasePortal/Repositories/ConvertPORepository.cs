using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using PurchasePortal.Models.DbModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace PurchasePortal.Repositories
{
    public class ConvertPORepository
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public ConvertPORepository(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("UICT2");
        }

        public async Task<(IEnumerable<PUR_PO> data, int recordsTotal, int recordsFiltered)> GetPURPOListAsync(
            int start, int length, string searchValue, string sortColumn, string sortDirection, bool? convertpoflag)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Base query
                var baseQuery = @"FROM [UICT2].[dbo].[pur_po]";
                var whereClause = "";
                var whereClauses = new List<string>();

                // Filter by convertpoflag
                if (convertpoflag.HasValue)
                {
                    whereClauses.Add("[convertpoflag] = @ConvertPoFlag");
                }

                // Search
                if (!string.IsNullOrEmpty(searchValue))
                {
                    whereClauses.Add(@"([prno] LIKE @SearchValue 
                                    OR [area] LIKE @SearchValue 
                                    OR [creuser] LIKE @SearchValue 
                                    OR [updateuser] LIKE @SearchValue)");
                }

                if (whereClauses.Any())
                {
                    whereClause = " WHERE " + string.Join(" AND ", whereClauses);
                }

                // Get total count
                var countQuery = $"SELECT COUNT(*) {baseQuery}";
                var recordsTotal = await connection.ExecuteScalarAsync<int>(countQuery);

                // Get filtered count
                var filteredCountQuery = $"SELECT COUNT(*) {baseQuery} {whereClause}";
                var recordsFiltered = await connection.ExecuteScalarAsync<int>(filteredCountQuery, 
                    new { SearchValue = $"%{searchValue}%", ConvertPoFlag = convertpoflag });

                // Order by
                var orderBy = "ORDER BY [credate] DESC";
                if (!string.IsNullOrEmpty(sortColumn))
                {
                    orderBy = $"ORDER BY [{sortColumn}] {sortDirection}";
                }

                // Get data with pagination
                var dataQuery = $@"
                    SELECT [id],[prno],[amount],[area],[credate],[creuser],[updatedate],[updateuser],[convertpoflag],[convertpodate]
                    {baseQuery} 
                    {whereClause}
                    {orderBy}
                    OFFSET @Start ROWS FETCH NEXT @Length ROWS ONLY";

                var data = await connection.QueryAsync<PUR_PO>(dataQuery, new
                {
                    SearchValue = $"%{searchValue}%",
                    ConvertPoFlag = convertpoflag,
                    Start = start,
                    Length = length
                });

                return (data, recordsTotal, recordsFiltered);
            }
        }

        public async Task<(int totalCount, int convertedCount, int notConvertedCount)> GetCountsAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = @"
                    SELECT 
                        COUNT(*) as TotalCount,
                        SUM(CASE WHEN [convertpoflag] = 1 THEN 1 ELSE 0 END) as ConvertedCount,
                        SUM(CASE WHEN [convertpoflag] = 0 THEN 1 ELSE 0 END) as NotConvertedCount
                    FROM [UICT2].[dbo].[pur_po]";

                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(query);

                return (
                    totalCount: result.TotalCount,
                    convertedCount: result.ConvertedCount,
                    notConvertedCount: result.NotConvertedCount
                );
            }
        }

        public async Task<bool> UpdateAreaAsync(Guid id, string area, string updateUser)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = @"UPDATE [UICT2].[dbo].[pur_po] 
                             SET [area] = @Area, 
                                 [updatedate] = @UpdateDate, 
                                 [updateuser] = @UpdateUser
                             WHERE [id] = @Id";

                var result = await connection.ExecuteAsync(query, new
                {
                    Id = id,
                    Area = area,
                    UpdateDate = DateTime.Now,
                    UpdateUser = updateUser
                });

                return result > 0;
            }
        }
    }
}
