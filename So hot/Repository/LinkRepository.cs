using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace So_hot.Repository
{
    public class LinkRepository
    {
        public IDbConnection connection;
        public LinkRepository(IDbConnection db)
        {
            connection = db;
        }
        public async Task<List<tblLink>> GetLinkList( bool isRealData)
        {
            if (connection == null || connection.State == ConnectionState.Closed)
                return null;
            string select = string.Empty;
            if(isRealData)
                select = string.Format("select * from tblLink where (Type is null or Type = 1)"); 
            else
                select = string.Format("select * from tblLink where Type = 0");
            var result = await connection.QueryAsync<tblLink>(select);
            if (result != null)
                return result.ToList();
            return null;
        }
        public async Task Insert(tblLink model)
        {
            string update = string.Format("insert into tblLink(Link,LinkDown,Dienvien,Note,Status,Type) values(@Link,@LinkDown,@Dienvien,@Note,@Status,@Type)");
            await connection.ExecuteAsync(update, model);
        }
        public async Task Update(tblLink model)
        {
            string update = string.Format("update tblLink set Link =@Link,LinkDown=@LinkDown,Dienvien=@Dienvien,Note=@Note,Status=@Status,Type=@Type where Id =@Id");
            await connection.ExecuteAsync(update, model);
        }
        public async Task<List<tblLink>> Search(string query, bool isRealData=false)
        {
            string select = $"select * from tblLink where (Link like '%{query}%' or LinkDown like '%{query}%' or Dienvien like '%{query}%' or Note like '%{query}%')";
            if (isRealData == false)
                select = string.Format(select + " and Type = 0");
            else
                select = string.Format(select + " and (Type is null or Type = 1)");
            var result = await connection.QueryAsync<tblLink>(select);
            if (result == null)
                return null;
            return result.ToList();
        }
        public async Task DeleteById(int Id)
        {
            await connection.ExecuteAsync($"delete from tblLink where Id = {Id}");
        }
    }
}
