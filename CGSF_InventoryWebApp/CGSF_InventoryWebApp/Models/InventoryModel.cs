using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CGSFEntityLib;
using System.Configuration;
using GridMvc;

namespace CGSF_InventoryWebApp.Models
{
    public class InventoryModel : BaseModel
    {
        
        public List<GroceryItem> AllItems;
        public GroceryItem NewItem;
        public Grid<GroceryItem> ItemGrid;
        public Grid<IOrderableItem> OrderGrid;
        public Grid<Order> OrderHistoryGrid;
        public Order CurrentOrder;
        public bool ShowPrintView = false;
        public string PrintFileString = String.Empty;

        public InventoryModel()
        {
            AllItems = GroceryItem.GetAllItemsAsList(ConnString);
            CurrentUser = new User(ConnString);
            NewItem = new GroceryItem(ConnString);
            ItemGrid = new Grid<GroceryItem>(AllItems);
            CurrentOrder = new Order(ConnString);
            OrderGrid = new Grid<IOrderableItem>(CurrentOrder.OrderItems);
            OrderHistoryGrid = new Grid<Order>(Order.GetOrderHistory(ConnString));
        }

    }
}