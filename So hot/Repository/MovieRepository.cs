using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace So_hot.Repository
{
    public class MovieRepository
    {
        public IDbConnection connection;
        public MovieRepository(IDbConnection db)
        {
            connection = db;
        }
        public async Task<List<Movies>> GetAllMovie()
        {
            var result = await connection.QueryAsync<Movies>("select * from Movies");
            if (result == null)
                return null;
            return result.ToList();
        }
        public async Task DeleteByFullPath(string fullPath)
        {
            await connection.ExecuteAsync($"delete from Movies where FullPath = \"{fullPath}\"" );
        }
        public async Task Insert(Movies mv)
        {
            string update = $"insert into Movies(Name,FullPath,Image) values(\"{mv.Name}\",\"{mv.FullPath}\",\"{mv.Image}\")";
            await connection.ExecuteAsync(update);
        }
        public async Task<List<Movies>> Search(string query)
        {
            string select = $"select * from Movies where Name like \"%{query}%\"";
            var result = await connection.QueryAsync<Movies>(select);
            if (result == null)
                return null;
            return result.ToList();
        }
        public async Task<List<Movies>> GetByFullPath(string fullPath)
        {
            string select = $"select * from Movies where FullPath = \"{fullPath}\"";
            var result = await connection.QueryAsync<Movies>(select);
            if (result == null)
                return null;
            return result.ToList();
        }
        public async Task<bool> DeleteAllMovie()
        {
            if (!UserManagement.UserSession.Type)
                return false;
            await connection.ExecuteAsync("delete from Movies");
            return true;
        }
    }
}
