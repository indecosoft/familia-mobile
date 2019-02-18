using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Android.Util;
using Java.Lang;
using SQLite;

namespace FamiliaXamarin.Helpers
{
    public sealed class SqlHelper<T>
        where T : new()
    {
        private static readonly SQLiteAsyncConnection Db;

        private async Task<SqlHelper<T>> InitializeAsync() 
        {
            await Db.CreateTableAsync<T>();
            return this;
        }
        public static Task<SqlHelper<T>> CreateAsync() 
        {
            return new SqlHelper<T>().InitializeAsync();
        }
        
        static SqlHelper()
        {
            var path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            const string dbName = "devices_data.db";
            Db = new SQLiteAsyncConnection(Path.Combine(path, dbName));
        }
        
        public async Task<IEnumerable<T>> QueryValuations(string query)
        {
            try
            {
                return await Db.QueryAsync<T>(query);
            }
            catch (System.Exception e)
            {
                Log.Error("InsertionError", e.Message);
                return null;
            }  
        }
        
        public async Task Insert(T dataToInsert)
        {
            try
            {
                await Db.InsertAsync(dataToInsert);
            }
            catch (Exception e)
            {
                Log.Error("InsertionError", e.Message);
            }
            
        }
        public async Task DropTable()
        {
            try
            {
                await Db.DropTableAsync<T>();
            }
            catch (Exception e)
            {
                Log.Error("IDropTableError", e.Message);
            }
            
        }
    }
}