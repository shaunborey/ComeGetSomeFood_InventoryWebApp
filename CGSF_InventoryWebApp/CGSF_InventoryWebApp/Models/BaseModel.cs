using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CGSFEntityLib;
using System.Configuration;

namespace CGSF_InventoryWebApp.Models
{
    public class BaseModel
    {
        public static string ConnString = ConfigurationManager.ConnectionStrings["CGSFConnString"].ConnectionString;
        public User CurrentUser;

        public string Message;
        public bool IsSuccess = false;
        public bool IsError = false;
        public bool ShowErrorModal = false;
        public string ModalType;

        public BaseModel()
        {
            CurrentUser = new User(ConnString);
        }
    }
}