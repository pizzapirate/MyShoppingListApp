using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoppingListApp.Models
{

    public class ShoppingListItem
    {
        [PrimaryKey, AutoIncrement, Column("Id")]
        public int Id { get; set; }
        public string ItemName { get; set; }
        public bool BasketStatus { get; set; }
        public int TableId { get; set; }

    }

}
 