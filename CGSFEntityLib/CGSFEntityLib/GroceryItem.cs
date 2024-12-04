using Microsoft.ApplicationBlocks.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGSFEntityLib
{    
    public class GroceryItem : IOrderableItem
    {
        private int _ItemID;
        public int ItemID
        {
            get { return _ItemID; }
        }

        private string _SKU;
        public string SKU
        {
            get { return _SKU; }
            set { _SKU = value; }
        }

        private string _Brand;
        public string Brand
        {
            get { return _Brand; }
            set { _Brand = value; }
        }

        private string _Description;
        public string Description
        {
            get { return _Description; }
            set { _Description = value; }
        }

        private int _CurrentQty = 0;
        public int CurrentQty
        {
            get { return _CurrentQty; }
            set { _CurrentQty = value; }
        }

        private int _OnOrderQty = 0;
        public int OnOrderQty
        {
            get { return _OnOrderQty; }
            set { _OnOrderQty = value; }
        }

        private int _MaxQty = 0;
        public int MaxQty
        {
            get { return _MaxQty; }
            set { _MaxQty = value; }
        }

        private int _OrderThreshold = 0;
        public int OrderThreshold
        {
            get { return _OrderThreshold; }
            set { _OrderThreshold = value; }
        }

        private bool _IsSelected = true;
        public bool IsSelected
        {
            get { return _IsSelected; }
            set { _IsSelected = value; }
        }

        private DateTime _DateLastReceived = DateTime.Now;
        public DateTime DateLastReceived
        {
            get { return _DateLastReceived; }
            set { _DateLastReceived = value; }
        }

        private int _QtyLastReceived = 0;
        public int QtyLastReceived
        {
            get { return _QtyLastReceived; }
            set { _QtyLastReceived = value; }
        }

        private double _UnitCost = 0.00;
        public double UnitCost
        {
            get { return _UnitCost; }
            set { _UnitCost = value; }
        }

        private double _RetailPrice = 0.00;
        public double RetailPrice
        {
            get { return _RetailPrice; }
            set { _RetailPrice = value; }
        }

        private DateTime _DateCreated = DateTime.Now;
        public DateTime DateCreated
        {
            get { return _DateCreated; }
        }

        private User _CreatedBy;
        public string CreatedBy
        {
            get { return _CreatedBy.UserName; }
        }

        private DateTime _DateLastModified = DateTime.Now;
        public DateTime DateLastModified
        {
            get { return _DateLastModified; }
        }

        private User _LastModifiedBy;
        public string LastModifiedBy
        {
            get { return _LastModifiedBy.UserName; }
        }

        private string _ConnString;

        public GroceryItem(string connString)
        {
            _ConnString = connString;
            _CreatedBy = new User(connString);
            _LastModifiedBy = new User(connString);
        }

        public GroceryItem(string connString, int itemID)
        {
            _ConnString = connString;
            _CreatedBy = new User(connString);
            _LastModifiedBy = new User(connString);
            LoadItem(itemID);
        }

        public GroceryItem(string connString, int itemID, int updateUserID)
        {
            _ConnString = connString;
            _CreatedBy = new User(connString);
            _LastModifiedBy = new User(connString);
            _LastModifiedBy.LoadUserDetails(updateUserID);
            LoadItem(itemID);
        }

        public GroceryItem(string sku, string brand, string description, int qty, int orderThreshold, int maxQty, double unitCost, double retailPrice, int createdBy, int lastModifiedBy, string connString)
        {
            _SKU = sku;
            _Brand = brand;
            _Description = description;
            _CurrentQty = qty;
            _OrderThreshold = orderThreshold;
            _MaxQty = maxQty;
            _UnitCost = unitCost;
            _RetailPrice = retailPrice;
            _ConnString = connString;
            _CreatedBy = new User(connString);
            _LastModifiedBy = new User(connString);
            _CreatedBy.LoadUserDetails(createdBy);
            _LastModifiedBy.LoadUserDetails(lastModifiedBy);
        }

        public GroceryItem(int itemID, string sku, string brand, string description, int qty, int orderThreshold, int maxQty, double unitCost, double retailPrice, int createdBy, int lastModifiedBy, string connString)
        {
            _ItemID = itemID;
            _SKU = sku;
            _Brand = brand;
            _Description = description;
            _CurrentQty = qty;
            _OrderThreshold = orderThreshold;
            _MaxQty = maxQty;
            _UnitCost = unitCost;
            _RetailPrice = retailPrice;
            _ConnString = connString;
            _CreatedBy = new User(connString);
            _LastModifiedBy = new User(connString);
            _CreatedBy.LoadUserDetails(createdBy);
            _LastModifiedBy.LoadUserDetails(lastModifiedBy);
        }

        public bool LoadItem(int itemID)
        {
            try
            {
                string sql = "SELECT * FROM GroceryItems WHERE ItemID = @itemid";
                SqlParameter[] p = new SqlParameter[] {
                    new SqlParameter("@itemid", itemID)
                };

                DataRow dr = SqlHelper.ExecuteDataset(_ConnString, CommandType.Text, sql, p).Tables[0].Rows[0];

                _ItemID = (int)dr["ItemID"];
                _SKU = dr["SKU"].ToString();
                _Brand = dr["Brand"].ToString();
                _Description = dr["Description"].ToString();
                _CurrentQty = (int)dr["CurrentQty"];
                _OnOrderQty = (int)dr["OnOrderQty"];
                _MaxQty = (int)dr["MaxQty"];
                _OrderThreshold = (int)dr["OrderThreshold"];
                _DateLastReceived = (DateTime)dr["DateLastReceived"];               
                _QtyLastReceived = (int)dr["QtyLastReceived"];
                _UnitCost = (double)dr["UnitCost"];
                _RetailPrice = (double)dr["RetailPrice"];
                _DateCreated = (DateTime)dr["DateLastCreated"];
                User createdBy = new User(_ConnString);
                User lastModifiedBy = new User(_ConnString);
                createdBy.LoadUserDetails((int)dr["CreatedBy"]);
                lastModifiedBy.LoadUserDetails((int)dr["LastModifiedBy"]);
                _CreatedBy = createdBy;
                _LastModifiedBy = lastModifiedBy;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool AlreadyExists()
        {
            string sql = "SELECT ItemID FROM GroceryItems WHERE SKU = @sku";
            SqlParameter[] p = new SqlParameter[] {
                new SqlParameter("@sku", _SKU)
            };

            return SqlHelper.ExecuteScalar(_ConnString, CommandType.Text, sql, p) != null;
        }

        public bool Save(bool isNew, out string resultMsg)
        {

            string sql = "";

            if (isNew)
            {
                if (AlreadyExists())
                {
                    resultMsg = "The item already exists";
                    return false;
                }

                sql = @"INSERT INTO GroceryItems (SKU, Brand, Description, CurrentQty, OnOrderQty, MaxQty, OrderThreshold, DateLastReceived,
                                                   QtyLastReceived, UnitCost, RetailPrice, DateCreated, CreatedBy, DateLastModified, LastModifiedBy)
                        VALUES (@sku, @brand, @descr, @currqty, @onorderqty, @maxqty, @orderthreshold, @datelastreceived,
                                @qtylastreceived, @unitcost, @retailprice, @datecreated, @createdby, @datelastmodified, @lastmodifiedby)";
            }
            else
            {
                sql = @"UPDATE GroceryItems SET SKU = @sku, Brand = @brand, Description = @descr, CurrentQty = @currqty,
                            OnOrderQty = @onorderqty, MaxQty = @maxqty, OrderThreshold = @orderthreshold, DateLastReceived = @datelastreceived,
                            QtyLastReceived = @qtylastreceived, UnitCost = @unitcost, RetailPrice = @retailprice, DateLastModified = @datecreated,
                            LastModifiedBy = @lastmodifiedby WHERE ItemID = @id";
            }

            SqlParameter[] p = new SqlParameter[] {
                new SqlParameter("@id", _ItemID),
                new SqlParameter("@sku", _SKU),
                new SqlParameter("@brand", _Brand),
                new SqlParameter("@descr", _Description),
                new SqlParameter("@currqty", _CurrentQty),
                new SqlParameter("@onorderqty", _OnOrderQty),
                new SqlParameter("@maxqty", _MaxQty),
                new SqlParameter("@orderthreshold", _OrderThreshold),
                new SqlParameter("@datelastreceived", _DateLastReceived),                
                new SqlParameter("@qtylastreceived", _QtyLastReceived),
                new SqlParameter("@unitcost", _UnitCost),
                new SqlParameter("@retailprice", _RetailPrice),
                new SqlParameter("@datecreated", _DateCreated),
                new SqlParameter("@createdby", _CreatedBy.UserID),
                new SqlParameter("@datelastmodified", _DateLastModified),
                new SqlParameter("@lastmodifiedby", _LastModifiedBy.UserID)
            };

            try
            {
                SqlHelper.ExecuteNonQuery(_ConnString, CommandType.Text, sql, p);
            }
            catch (Exception ex)
            {
                resultMsg = ex.Message;
                return false;
            }

            resultMsg = "Save Successful";
            return true;
        }

        public bool DeleteItem(out string resultMsg)
        {
            try
            {
                string sql = "DELETE FROM GroceryItems WHERE ItemID = @itemid";
                SqlParameter[] p = new SqlParameter[] {
                    new SqlParameter("@itemid", _ItemID)
                };

                SqlHelper.ExecuteNonQuery(_ConnString, CommandType.Text, sql, p);
                resultMsg = "Item has been deleted successfully";
                return true;
            }
            catch (Exception ex)
            {
                resultMsg = ex.Message;
                return false;
            }
        }

        public bool UpdateCurrentQty(int qtyChange)
        {
            int newQty = _CurrentQty + qtyChange;
            if (newQty < 0)
            {
                throw new InvalidOperationException("Unable to update to a quantity that is less than zero");                    
            }

            _CurrentQty = newQty;
            return true;
        }

        public static List<GroceryItem> GetAllItemsAsList(string connString)
        {
            List<GroceryItem> items = new List<GroceryItem>();

            items = GetAllItemsAsDataTable(connString).AsEnumerable()
                        .Select(row => new GroceryItem(row.Field<int>("ItemID"), row.Field<string>("SKU"), row.Field<string>("Brand"), row.Field<string>("Description"),
                                            row.Field<int>("CurrentQty"), row.Field<int>("OrderThreshold"), row.Field<int>("MaxQty"),
                                            row.Field<double>("UnitCost"), row.Field<double>("RetailPrice"), row.Field<int>("CreatedBy"),
                                            row.Field<int>("LastModifiedBy"), connString)).ToList();
            return items;
        }

        public static List<GroceryItem> GetAllItemsAsList(string connString, DateTime date)
        {
            List<GroceryItem> items = new List<GroceryItem>();

            items = GetAllItemsAsDataTable(connString, date).AsEnumerable()
                        .Select(row => new GroceryItem(row.Field<int>("ItemID"), row.Field<string>("SKU"), row.Field<string>("Brand"), row.Field<string>("Description"),
                                            row.Field<int>("CurrentQty"), row.Field<int>("OrderThreshold"), row.Field<int>("MaxQty"),
                                            row.Field<double>("UnitCost"), row.Field<double>("RetailPrice"), row.Field<int>("CreatedBy"),
                                            row.Field<int>("LastModifiedBy"), connString)).ToList();
            return items;
        }

        public static DataTable GetAllItemsAsDataTable(string connString)
        {
            try
            {
                string sql = "SELECT * FROM GroceryItems";
                DataTable items = SqlHelper.ExecuteDataset(connString, CommandType.Text, sql).Tables[0];
                return items;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static DataTable GetAllItemsAsDataTable(string connString, DateTime date)
        {
            try
            {
                string sql = "SELECT * FROM GroceryItems WHERE DateLastReceived >= @date";
                SqlParameter[] p = new SqlParameter[] {
                    new SqlParameter("@date", date)
                };
                DataTable items = SqlHelper.ExecuteDataset(connString, CommandType.Text, sql, p).Tables[0];
                return items;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
