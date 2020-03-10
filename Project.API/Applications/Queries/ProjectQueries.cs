using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MySql.Data.MySqlClient;

namespace Project.API.Applications.Queries
{
    public class ProjectQueries : IProjectQueries
    {
        private readonly string _connStr;
        public ProjectQueries(string connStr)
        {
            this._connStr = connStr;
        }

        public async Task<dynamic> GetProjectByUserId(int userId)
        {
            using (var conn = new MySqlConnection(_connStr))
            {
                conn.Open();

                var sql = @"SELECT p.Id,p.Avatar,p.Company,p.FinStage,p.Introduction,p.Tags,p.ShowSecurityInfo,p.CreatedTime FROM Projects p where p.UserId = @userId";

                var result = await conn.QueryAsync<decimal>(sql, new { userId });

                return result;
            }
        }

        public async Task<dynamic> GetProjectDetail(int projectId)
        {

            using (var conn = new MySqlConnection(_connStr))
            {
                conn.Open();

                string sql = @"SELECT a.Company,a.Area,a.Province,a.FinStage,a.FinMoney,a.Valuation,a.FinPercentage,a.Introduction,a.UserId,a.Income,a.Revenue,a.Avatar,a.BrokerageOptions,b.Tags,b.Visible FROM Projects a INNER JOIN ProjectVisibleRules b on a.Id = b.ProjectId WHERE a.Id = @projectId";

                var result = await conn.QueryAsync<decimal>(sql, new { projectId });

                return result;
            }
        }
    }
}
