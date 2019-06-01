using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace So_hot.Repository
{
    public class InfoRepository
    {
        public IDbConnection connection;
        public InfoRepository(IDbConnection db)
        {
            connection = db;
        }
        public async Task<List<Info>> GetInfoList()
        {
            var result = await connection.QueryAsync<Info>("select * from tblInfo");
            if (result != null)
                return result.ToList();
            return null;
        }
    }
}
