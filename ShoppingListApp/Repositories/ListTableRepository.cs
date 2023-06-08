using ShoppingListApp.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoppingListApp.Repositories
{
    public class ListTableRepository
    {
        private static string _dbPath;
        private static SQLiteConnection conn;
        public ListTableRepository(string dbPath)
        {
            _dbPath = dbPath;
            Init();
        }
        public void Init()
        {
            conn = new SQLiteConnection(_dbPath);
            conn.CreateTable<ListTableEntry>();
        }
        public List<ListTableEntry> GetAll()
        {
            return conn.Table<ListTableEntry>().ToList();
        }
        public void Add(ListTableEntry item)
        {
            conn.Insert(item);
        }
        public void Delete(ListTableEntry item)
        {
            App.ShoppingListRepository.DeleteEntireList(item.Id);
            conn.Delete(item);
        }
        public void DeleteForReorder() // Only deletes all of the items in the selected table, DOES NOT DELETE TABLE ID from LISTS TABLE
        {
            App.ShoppingListRepository.DeleteEntireList(App.SelectedTable);
        }
        public string GetTableName(int tableId)
        {
            return conn.Table<ListTableEntry>().Where(x => x.Id == tableId).First().Name;
        }
    }
}
