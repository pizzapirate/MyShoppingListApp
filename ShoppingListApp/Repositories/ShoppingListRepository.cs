using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShoppingListApp.Models;
using System.Collections.ObjectModel;

namespace ShoppingListApp.Repositories
{
    public class ShoppingListRepository
    {
        private static string _dbPath;
        private static SQLiteConnection conn;
        public ShoppingListRepository(string dbPath)
        {   
            _dbPath = dbPath;
            Init();
        }
        public void Init()
        {
            conn = new SQLiteConnection(_dbPath);
            conn.CreateTable<ShoppingListItem>();
        }
        public List<ShoppingListItem> GetAll()
        {
            return conn.Table<ShoppingListItem>().ToList();
        }
        public async Task<List<ShoppingListItem>> GetAllAsync()
        {
            return await Task.Run(() => conn.Table<ShoppingListItem>().Where(x => x.TableId == App.SelectedTable).ToList());
        }
        public void Add(ShoppingListItem item)
        {
            conn.Insert(item);
        }
        public void AddRange(ObservableCollection<ShoppingListItem> items) 
        {
            conn.InsertAll(items);
        }
        public void Delete(ShoppingListItem item)
        {
            conn.Delete(item);
        }
        public void Update(ShoppingListItem item)
        {
            conn.Update(item);
        }
        public List<string> GetTableNames()
        {
            var tableNames = new List<string>();
            foreach (var con in conn.TableMappings)
            {
                if (con.TableName != "ColumnInfo")
                {
                    tableNames.Add(con.TableName);
                }
            }
            return tableNames;
        }
        public void DeleteEntireList(int tableId)
        {
            foreach (var con in conn.Table<ShoppingListItem>())
            {
                if (con.TableId == tableId){
                    conn.Delete(con);
                }
            }
        }
        public void DeleteBasketStatusTrueItems(int tabledId)
        {
            foreach (var con in conn.Table<ShoppingListItem>())
            {
                if (con.TableId == tabledId && con.BasketStatus == true)
                {
                    conn.Delete(con);
                }
            }
        }
    }

}
