using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Android.Util;
using SQLite;

namespace Familia.Helpers {
	public sealed class SqlHelper<T> where T : new() {
		private static readonly SQLiteAsyncConnection Db;

		private async Task<SqlHelper<T>> InitializeAsync() {
			await Db.CreateTableAsync<T>();
			return this;
		}

		public static Task<SqlHelper<T>> CreateAsync() {
			return new SqlHelper<T>().InitializeAsync();
		}

		~SqlHelper() {
			Db.CloseAsync();
		}

		static SqlHelper() {
			string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			const string dbName = "devices_data.db";
			Db = new SQLiteAsyncConnection(Path.Combine(path, dbName));
		}

		public async Task<IEnumerable<T>> QueryValuations(string query) {
			try {
				return await Db.QueryAsync<T>(query);
			} catch (Exception e) {
				Log.Error("InsertionError", e.Message);
				return null;
			}
		}

		public async Task Insert(T dataToInsert) {
			try {
				await Db.InsertAsync(dataToInsert);
			} catch (Java.Lang.Exception e) {
				Log.Error("InsertionError", e.Message);
			}
		}

		public async Task Delete(T dataToDelete) {
			try {
				await Db.DeleteAsync(dataToDelete);
			} catch (Java.Lang.Exception e) {
				Log.Error("InsertionError", e.Message);
			}
		}

		public async Task DeleteAll() {
			try {
				await Db.DeleteAllAsync<T>();
			} catch (Java.Lang.Exception e) {
				Log.Error("DeleteError", e.Message);
			}
		}

		public async Task DropTable() {
			try {
				await Db.DropTableAsync<T>();
			} catch (Java.Lang.Exception e) {
				Log.Error("IDropTableError", e.Message);
			}
		}

		public async void DropTables(params Type[] tables) {
			try {
				foreach (Type table in tables) {
					await Db.ExecuteAsync($"DELETE FROM {table.Name}");
				}
			} catch (Java.Lang.Exception e) {
				Log.Error("IDropTableError", e.Message);
			}
		}

    
    }
}