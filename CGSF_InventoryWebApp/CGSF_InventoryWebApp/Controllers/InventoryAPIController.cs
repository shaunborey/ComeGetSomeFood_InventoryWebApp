using CGSF_InventoryWebApp.Models;
using CGSFEntityLib;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Routing;
using Newtonsoft.Json;

namespace CGSF_InventoryWebApp.Controllers
{
    public class InventoryAPIController : ApiController
    {
        // GET api/InventoryItems
        /// <summary>
        /// Method to get all inventory items
        /// </summary>
        /// <returns>List of all inventory items</returns>
        [Route("api/GetInventory")]
        public string GetInventory()
        {
            List<GroceryItem> items = GroceryItem.GetAllItemsAsList(ConfigurationManager.ConnectionStrings["CGSFConnString"].ConnectionString);
            string retval = JsonConvert.SerializeObject(items);
            return retval;
        }

        // GET api/GetInventory/5/5/5
        /// <summary>
        /// Method to get all inventory items
        /// </summary>       
        /// <param name="day">Day of the date filter</param>
        /// <param name="month">Month of the date filter</param>
        /// <param name="year">Year of the date filter</param>
        /// <returns>List of all inventory items</returns>
        [Route("api/GetInventory/{month}/{day}/{year}")]
        public string GetInventory(int month, int day, int year)
        {
            DateTime date = new DateTime(year, month, day);
            List<GroceryItem> items = GroceryItem.GetAllItemsAsList(ConfigurationManager.ConnectionStrings["CGSFConnString"].ConnectionString, date);
            string retval = JsonConvert.SerializeObject(items);

            return retval;
        }

    }
}
