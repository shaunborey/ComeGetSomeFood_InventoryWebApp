using CGSF_InventoryWebApp.Models;
using CGSFEntityLib;
using GridMvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CGSF_InventoryWebApp.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(UserModel model)
        {
            if (TempData["UserModel"] != null)
            {
                model = TempData["UserModel"] as UserModel;                
            }

            if (Session["UID"] != null)
            {
                InventoryModel invModel = new InventoryModel();
                invModel.IsSuccess = model.IsSuccess;
                invModel.IsError = model.IsError;
                invModel.Message = model.Message;
                invModel.CurrentUser.LoadUserDetails((int)Session["UID"]);
                TempData["InventoryModel"] = invModel;
                return RedirectToAction("Inventory");                
            }

            return View(model);
        }

        [Route("ProcessUser/{actionType}")]
        public ActionResult ProcessUser(UserModel model, string actionType)
        {

            switch (actionType)
            {
                case "Logout":
                    Session["UID"] = null;                    
                    TempData["UserModel"] = null;
                    return RedirectToAction("Index");

                case "AddUser":

                    if (Request.Form["NewUser.Password"] != Request.Form["ConfirmPassword"])
                    {
                        model.IsError = true;
                        model.ShowErrorModal = false;
                        model.ModalType = "addUser";
                        model.Message = "Passwords do not match";

                    }
                    else
                    {
                        model.NewUser = new User(CGSFEntityLib.User.UserType.Employee, Request.Form["NewUser.UserName"],
                                                    Request.Form["NewUser.Password"], Request.Form["NewUser.FirstName"],
                                                    Request.Form["NewUser.LastName"], Request.Form["NewUser.EmailAddress"], UserModel.ConnString);
                        if (model.NewUser.AddUser(out model.Message))
                        {
                            model.IsSuccess = true;
                            model.Message = "New user created successfully";
                        }
                        else
                        {
                            model.IsError = true;
                            model.ShowErrorModal = false;
                            model.ModalType = "addUser";
                        }
                    }

                    TempData["UserModel"] = model;
                    return RedirectToAction("Index");

                case "Login":
                    model.CurrentUser = new User(Request.Form["CurrentUser.UserName"], Request.Form["CurrentUser.Password"], UserModel.ConnString);
                    if (model.CurrentUser.Login(out model.Message))
                    {
                        Session["UID"] = model.CurrentUser.UserID;
                        return RedirectToAction("Inventory");
                    }
                    else
                    {
                        Session["UID"] = null;
                        model.IsError = true;
                        TempData["UserModel"] = model;
                        return RedirectToAction("Index");
                    }

                default:
                    return RedirectToAction("Index");                                     
             }            
        }

        [Route("Orders/{orderAction?}")]
        public ActionResult Orders(InventoryModel model, string orderAction)
        {
            int uid;
            bool validSession = int.TryParse(Session["UID"].ToString(), out uid);
            if (validSession)
            {
                if (!model.CurrentUser.LoadUserDetails(uid))
                {
                    return RedirectToAction("Index");
                }
            }
            else
            {
                return RedirectToAction("Index");
            }
            
            if (TempData["InventoryModel"] != null)
            {
                model = TempData["InventoryModel"] as InventoryModel;
            }

            model.OrderHistoryGrid = new Grid<Order>(Order.GetOrderHistory(InventoryModel.ConnString));
            //Handle any user modifications for items in posted page
            if (Request.QueryString["grid-page"] != null || Request.QueryString["grid-column"] != null || !String.IsNullOrEmpty(orderAction))
            {
                model.CurrentOrder.OrderItems = model.CurrentOrder.CalculateOrder();
                model.OrderGrid = new Grid<IOrderableItem>(model.CurrentOrder.OrderItems);
                foreach (IOrderableItem item in model.CurrentOrder.OrderItems)
                {

                    item.IsSelected = !String.IsNullOrEmpty(Request.Form["hdnIsSelected." + item.ItemID]);
                    int qty;
                    if (int.TryParse(Request.Form["hdnOrderQty." + item.ItemID], out qty))
                    {
                        item.OnOrderQty = qty;
                    }

                    if (!String.IsNullOrEmpty(Request.Form["ckbxIsSelected." + item.ItemID]))
                    {
                        item.IsSelected = Request.Form["ckbxIsSelected." + item.ItemID].Contains("true");
                    }
                }

                model.CurrentOrder.OrderNotes = Request.Form["CurrentOrder.OrderNotes"];

                if (!String.IsNullOrEmpty(orderAction))
                {
                    switch (orderAction)
                    {
                        case "GetOrder":
                            model.CurrentOrder.OrderItems = model.CurrentOrder.CalculateOrder();
                            model.OrderGrid = new Grid<IOrderableItem>(model.CurrentOrder.OrderItems);
                            if (model.CurrentOrder.OrderItems.Count == 0)
                            {
                                model.IsError = true;
                                model.Message = "There is nothing to order at this time";
                                model.ShowErrorModal = true;
                            }
                            else
                            {
                                model.ShowErrorModal = false;
                            }
                            return View(model);

                        case "EditOrderItem":
                            try
                            {
                                if (model.CurrentUser.LoadUserDetails((int)Session["UID"]))
                                {                                    
                                    IOrderableItem item = model.CurrentOrder.OrderItems.Find(v => v.ItemID == int.Parse(Request.Form["editItemID"]));
                                    int qty;
                                    if (int.TryParse(Request.Form["editOrderQty"], out qty))
                                    {
                                        item.OnOrderQty = qty;
                                    }
                                }
                                else
                                {
                                    Session["UID"] = null;
                                    UserModel newModel = new UserModel();
                                    newModel.Message = "SESSION HAS EXPIRED.<BR />PLEASE LOG IN AGAIN";
                                    TempData["Model"] = newModel;
                                    return RedirectToAction("Index");
                                }
                            }
                            catch (Exception ex)
                            {
                                model.IsError = true;
                                model.ModalType = "editModal";
                                model.Message = ex.Message;
                                model.ShowErrorModal = false;
                                return View(model);
                            }
                            model.OrderGrid = new Grid<IOrderableItem>(model.CurrentOrder.OrderItems);
                            return View(model);

                        case "SubmitOrder":
                            try
                            {
                                model.CurrentOrder.OrderTotal = model.CurrentOrder.OrderItems.Sum(item => item.OnOrderQty * item.UnitCost);
                                model.CurrentOrder.IsOpen = true;
                                model.PrintFileString = System.Text.Encoding.Default.GetString(model.CurrentOrder.ProcessOrder(out model.Message));
                                model.ShowPrintView = true;
                                return View(model);
                            }
                            catch (Exception ex)
                            {
                                model.IsError = true;
                                model.Message = ex.Message;
                                return View(model);
                            }

                        case "CloseOrder":
                            int orderID = int.Parse(Request.Form["hdnCloseOrderID"]);
                            bool closeOnly = Request.Form["chbxCloseOnly"].Contains("true");
                            Order order = new Order(orderID, InventoryModel.ConnString);

                            if (order.Close(closeOnly, out model.Message))
                            {
                                model.IsSuccess = true;
                                return View(model);
                            }
                            else
                            {
                                model.IsError = true;
                                model.ShowErrorModal = false;
                                model.ModalType = "orderHistoryModal";
                                return View(model);
                            }
                    }
                }
            }

            model.OrderGrid = new Grid<IOrderableItem>(model.CurrentOrder.OrderItems);
            return View(model);
        }

        [Route("Inventory/{inventoryAction?}")]
        public ActionResult Inventory(InventoryModel model, string inventoryAction)
        {
            if (Session["UID"] != null)
            {
                if (model.CurrentUser == null)
                {
                    model.CurrentUser = new User(InventoryModel.ConnString);
                }

                if (!model.CurrentUser.LoadUserDetails((int)Session["UID"]))
                {
                    return RedirectToAction("Index");
                }
            }
            else
            {
                return RedirectToAction("Index");
            }

            if (TempData["InventoryModel"] != null)
            {
                model = TempData["InventoryModel"] as InventoryModel;
            }

       
            if (!String.IsNullOrEmpty(inventoryAction))
            {
                switch (inventoryAction)
                {
                    case "AddItem":
                        try
                        {
                            model.NewItem = new GroceryItem(Request.Form["NewItem.SKU"], Request.Form["NewItem.Brand"], Request.Form["NewItem.Description"],
                                                       int.Parse(Request.Form["NewItem.CurrentQty"]), int.Parse(Request.Form["NewItem.OrderThreshold"]), int.Parse(Request.Form["NewItem.MaxQty"]),
                                                       double.Parse(Request.Form["NewItem.UnitCost"]), double.Parse(Request.Form["NewItem.RetailPrice"]), model.CurrentUser.UserID,
                                                       model.CurrentUser.UserID, InventoryModel.ConnString);

                            if (model.CurrentUser.LoadUserDetails((int)Session["UID"]))
                            {
                                if (model.NewItem.Save(true, out model.Message))
                                {
                                    model.IsSuccess = true;
                                    model.AllItems = GroceryItem.GetAllItemsAsList(InventoryModel.ConnString);
                                    return View(model);
                                }
                                else
                                {
                                    model.IsError = true;
                                    model.ModalType = "newItemModal";
                                    model.ShowErrorModal = false;
                                    return View(model);
                                }
                            }
                            else
                            {
                                Session["UID"] = null;
                                UserModel newModel = new UserModel();
                                newModel.Message = "SESSION HAS EXPIRED.<BR />PLEASE LOG IN AGAIN";
                                TempData["Model"] = newModel;
                                return RedirectToAction("Index");
                            }
                        }
                        catch (Exception ex)
                        {
                            model.IsError = true;
                            model.ModalType = "newItemModal";
                            model.Message = ex.Message;
                            model.ShowErrorModal = false;
                            return View(model);
                        }
                        
                    case "EditItem":
                        try
                        {
                            if (model.CurrentUser.LoadUserDetails((int)Session["UID"]))
                            {
                                GroceryItem item = new GroceryItem(InventoryModel.ConnString, int.Parse(Request.Form["editItemID"]), model.CurrentUser.UserID);
                                item.SKU = Request.Form["editSKU"];
                                item.Brand = Request.Form["editBrand"];
                                item.Description = Request.Form["editDescription"];
                                item.CurrentQty = int.Parse(Request.Form["editCurrentQty"]);
                                item.MaxQty = int.Parse(Request.Form["editMaxQty"]);
                                item.OrderThreshold = int.Parse(Request.Form["editOrderThreshold"]);
                                item.UnitCost = double.Parse(Request.Form["editUnitCost"]);
                                item.RetailPrice = double.Parse(Request.Form["editRetailPrice"]);
                                                                
                                if (item.Save(false, out model.Message))
                                {
                                    model.IsSuccess = true;
                                    model.AllItems = GroceryItem.GetAllItemsAsList(InventoryModel.ConnString);
                                    model.ItemGrid = new Grid<GroceryItem>(model.AllItems);
                                    return View(model);
                                }
                                else
                                {
                                    model.IsError = true;
                                    model.ModalType = "editModal";
                                    model.ShowErrorModal = false;
                                    return View(model);
                                }
                            }
                            else
                            {
                                Session["UID"] = null;
                                UserModel newModel = new UserModel();
                                newModel.Message = "SESSION HAS EXPIRED.<BR />PLEASE LOG IN AGAIN";
                                TempData["Model"] = newModel;
                                return RedirectToAction("Index");
                            }
                        }
                        catch (Exception ex)
                        {
                            model.IsError = true;
                            model.ModalType = "editModal";
                            model.Message = ex.Message;
                            model.ShowErrorModal = false;
                            return View(model);
                        }

                    case "DeleteItem":                           

                        try
                        {
                            if (model.CurrentUser.LoadUserDetails((int)Session["UID"]))
                            {
                                GroceryItem deleteItem = new GroceryItem(InventoryModel.ConnString, int.Parse(Request.Form["deleteItemID"]), model.CurrentUser.UserID);
                                
                                if (deleteItem.DeleteItem(out model.Message))
                                {
                                    model.IsSuccess = true;
                                    model.AllItems = GroceryItem.GetAllItemsAsList(InventoryModel.ConnString);
                                    return View(model);
                                }
                                else
                                {
                                    model.IsError = true;
                                    model.ModalType = "deleteModal";
                                    model.ShowErrorModal = false;
                                    return View(model);
                                }
                            }
                            else
                            {
                                Session["UID"] = null;
                                UserModel newModel = new UserModel();
                                newModel.Message = "SESSION HAS EXPIRED.<BR />PLEASE LOG IN AGAIN";
                                TempData["Model"] = newModel;
                                return RedirectToAction("Index");
                            }
                        }
                        catch (Exception ex)
                        {
                            model.IsError = true;
                            model.ModalType = "deleteModal";
                            model.Message = ex.Message;
                            model.ShowErrorModal = false;
                            return View(model);
                        }
                }
            }           

            model.ItemGrid = new Grid<GroceryItem>(model.AllItems);
            
            return View(model);
        }
    }
}