using Microsoft.ApplicationBlocks.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace CGSFEntityLib
{
    public class Order
    {
        private int _OrderID;
        public int OrderID
        {
            get { return _OrderID; }
        }

        private double _OrderTotal;
        public double OrderTotal
        {
            get { return _OrderTotal; }
            set { _OrderTotal = value; }
        }

        private string _OrderNotes;
        public string OrderNotes
        {
            get { return _OrderNotes; }
            set { _OrderNotes = value; }
        }

        private DateTime _OrderDate;
        public DateTime OrderDate
        {
            get { return _OrderDate; }
            set { _OrderDate = value; }
        }

        private bool _IsOpen;
        public bool IsOpen
        {
            get { return _IsOpen; }
            set { _IsOpen = value; }
        }

        private List<IOrderableItem> _OrderItems = new List<IOrderableItem>();
        public List<IOrderableItem> OrderItems
        {
            get { return _OrderItems; }
            set { _OrderItems = value; }
        }

        private string _ConnString;

        private const string _CustomerNumber = "12345678-900XX";
        private const string _BusinessNameAddress = "Come Get Some Food\n123 Any Street\nCary, NC  27511";
        private const string _BusinessPhone = "(336) 555 - 1234";

        public Order(string connString)
        {
            _ConnString = connString;
        }

        public Order(int orderID, string connString)
        {
            _OrderID = orderID;
            _ConnString = connString;
            LoadOrder(orderID);
        }

        public Order(double orderTotal, string notes, DateTime orderDate, bool isOpen)
        {
            _OrderTotal = orderTotal;
            _OrderNotes = notes;
            _OrderDate = orderDate;
            _IsOpen = isOpen;
            _OrderItems = new List<IOrderableItem>();
        }

        public bool LoadOrder(int orderID)
        {
            try
            {
                string sql = "SELECT * FROM Orders WHERE OrderID = @orderid";
                SqlParameter[] p = new SqlParameter[] {
                    new SqlParameter("@orderid", orderID)
                };

                DataRow dr = SqlHelper.ExecuteDataset(_ConnString, CommandType.Text, sql, p).Tables[0].Rows[0];

                _OrderID = orderID;
                _OrderTotal = (double)dr["OrderTotal"];
                _OrderNotes = dr["OrderNotes"].ToString();
                _OrderDate = (DateTime)dr["OrderDate"];
                _IsOpen = (bool)dr["IsOpen"];

                sql = "SELECT * FROM OrderItems WHERE OrderID = @orderid";

                DataTable dt = SqlHelper.ExecuteDataset(_ConnString, CommandType.Text, sql, p).Tables[0];

                foreach (DataRow row in dt.Rows)
                {
                    _OrderItems.Add(new GroceryItem(_ConnString, (int)row["ItemID"]));
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public List<IOrderableItem> CalculateOrder()
        {
            string sql = "SELECT * FROM GroceryItems WHERE (CurrentQty + OnOrderQty) <= OrderThreshold";

            _OrderItems = (SqlHelper.ExecuteDataset(_ConnString, CommandType.Text, sql, null).Tables[0]).AsEnumerable()
                            .Select(row => new GroceryItem(_ConnString, row.Field<int>("ItemID"))
                                {
                                    OnOrderQty = row.Field<int>("MaxQty") - row.Field<int>("CurrentQty") - row.Field<int>("OnOrderQty")
                                }).ToList<IOrderableItem>();

            return _OrderItems;
        }

        public byte[] ProcessOrder(out string resultMsg)
        {

            using (SqlConnection conn = new SqlConnection(_ConnString))
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();
                try
                {
                    
                    _OrderDate = DateTime.Now;

                    string sql = @"INSERT INTO Orders (OrderTotal, OrderNotes, OrderDate, IsOpen)
                            VALUES (@ordertotal, @ordernotes, @orderdate, @isopen);
                            SELECT SCOPE_IDENTITY()";
                    SqlParameter[] p = new SqlParameter[] { 
                new SqlParameter("@ordertotal", _OrderTotal),
                new SqlParameter("@ordernotes", _OrderNotes),
                new SqlParameter("@orderdate", _OrderDate),
                new SqlParameter("@isopen", _IsOpen)
            };

                    _OrderID = Convert.ToInt32(SqlHelper.ExecuteScalar(trans, CommandType.Text, sql, p));

                    string orderFile = String.Format(@"{0}\order.txt", Path.GetTempPath());
                    string headerSKU = "SKU";
                    string headerDesc = "Description";
                    string headerUnitCost = "Unit Cost";
                    string headerQty = "Quantity";
                    string headerLineTotal = "Line Total";
                    const int skuWidth = 15;
                    const int descWidth = 105;
                    const int unitCostWidth = 13;
                    const int qtyWidth = 10;
                    const int lineTotalWidth = 13;
                    int textLength = skuWidth + descWidth + unitCostWidth + qtyWidth + lineTotalWidth;
                    string sectionDivider = "*".PadRight(textLength,'*');
                    string headerDivider = "-".PadRight(textLength, '-');

                    using (TextWriter tw = File.CreateText(orderFile))
                    {
                        tw.WriteLine();
                        tw.WriteLine(sectionDivider);
                        tw.WriteLine("Customer Number: {0}{1}", _CustomerNumber, Environment.NewLine);
                        tw.WriteLine();
                        tw.WriteLine(_BusinessNameAddress);
                        tw.WriteLine(_BusinessPhone);
                        tw.WriteLine(sectionDivider);
                        tw.WriteLine();
                        tw.WriteLine("Order Date: {0:MM/dd/yyyy}{1}", _OrderDate, Environment.NewLine);
                        tw.WriteLine("Total Items: {0}   Order Total: {1:C}{2}", _OrderItems.Count, _OrderTotal, Environment.NewLine);
                        tw.WriteLine("Order Items: ");
                        tw.WriteLine();
                        tw.WriteLine(headerDivider);
                        tw.WriteLine("{0}{1}{2}{3}{4}", headerSKU.PadRight(skuWidth), headerDesc.PadRight(descWidth), headerUnitCost.PadRight(unitCostWidth), headerQty.PadRight(qtyWidth), headerLineTotal.PadRight(lineTotalWidth));
                        tw.WriteLine(headerDivider);

                        foreach (IOrderableItem item in _OrderItems)
                        {
                            double lineTotal = (item.OnOrderQty * item.UnitCost);
                            double unitCost = item.UnitCost;
                            tw.Write("{0}", item.SKU.PadRight(skuWidth));
                            tw.Write("{0}", item.Description.PadRight(descWidth));
                            tw.Write("{0}", item.UnitCost.ToString("C").PadRight(unitCostWidth));
                            tw.Write("{0}", item.OnOrderQty.ToString().PadRight(qtyWidth));
                            tw.WriteLine("{0}", lineTotal.ToString("C").PadRight(lineTotalWidth));
                            string sql2 = "INSERT INTO OrderItems VALUES (@orderid, @itemid, @orderqty, @linetotal)";
                            string sql3 = "UPDATE GroceryItems SET OnOrderQty = @orderqty WHERE ItemID = @itemid";
                            SqlParameter[] p2 = new SqlParameter[] {
                        new SqlParameter("@orderid", _OrderID),
                        new SqlParameter("@itemid", item.ItemID),
                        new SqlParameter("@orderqty", item.OnOrderQty),
                        new SqlParameter("@linetotal", lineTotal)
                    };
                            SqlHelper.ExecuteNonQuery(trans, CommandType.Text, sql2, p2);
                            SqlHelper.ExecuteNonQuery(trans, CommandType.Text, sql3, p2);
                        }
                    }

                    trans.Commit();
                    resultMsg = "Order Sent Successfully";
                    return File.ReadAllBytes(orderFile);
                }

                catch (Exception ex)
                {
                    resultMsg = ex.Message;
                    trans.Rollback();
                    return null;
                }

            }
 

        }

        public bool Close(bool closeOnly, out string resultMsg)
        {
            using (SqlConnection conn = new SqlConnection(_ConnString))
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();
                try
                {
                    string sql = "UPDATE Orders SET IsOpen = 0 WHERE OrderID = @orderid";
                    SqlParameter[] p = new SqlParameter[] {
                        new SqlParameter("@orderid", _OrderID)
                    };

                    SqlHelper.ExecuteNonQuery(trans, CommandType.Text, sql, p);

                    if (!closeOnly)
                    {
                        foreach (IOrderableItem item in _OrderItems)
                        {
                            string sql2 = "UPDATE GroceryItems SET CurrentQty = CurrentQty + @orderqty, OnOrderQty = OnOrderQty - @orderqty WHERE ItemID = @itemid";
                            SqlParameter[] p2 = new SqlParameter[] {
                                new SqlParameter("@orderqty", item.OnOrderQty),
                                new SqlParameter("@itemid", item.ItemID)
                            };

                            SqlHelper.ExecuteNonQuery(trans, CommandType.Text, sql2, p2);
                        }
                    }
                    else
                    {
                        foreach (IOrderableItem item in _OrderItems)
                        {
                            string sql2 = "UPDATE GroceryItems SET OnOrderQty = 0 WHERE ItemID = @itemid";
                            SqlParameter[] p2 = new SqlParameter[] {
                                new SqlParameter("@itemid", item.ItemID)
                            };

                            SqlHelper.ExecuteNonQuery(trans, CommandType.Text, sql2, p2);
                        }
                    }

                    trans.Commit();
                    resultMsg = "Order has been successfully closed";
                    return true;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    resultMsg = ex.Message;
                    return false;
                }
            }
        }

        public static List<Order> GetOrderHistory(string connString)
        {
            string sql = "SELECT * FROM Orders";
            return (SqlHelper.ExecuteDataset(connString, CommandType.Text, sql).Tables[0]).AsEnumerable()
                          .Select(row => new Order(connString) { _OrderID = row.Field<int>("OrderID"),
                                                                 _OrderDate = row.Field<DateTime>("OrderDate"),
                                                                 _OrderTotal = row.Field<double>("OrderTotal"),
                                                                 _OrderNotes = row.Field<string>("OrderNotes"),
                                                                 _IsOpen = row.Field<bool>("IsOpen") })
                                                                 .OrderByDescending(o => o.OrderDate).ToList<Order>();
        }
    }
}
