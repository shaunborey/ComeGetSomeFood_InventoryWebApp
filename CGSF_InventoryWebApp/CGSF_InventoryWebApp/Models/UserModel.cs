using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CGSFEntityLib;
using System.Configuration;
using System.ComponentModel.DataAnnotations;


namespace CGSF_InventoryWebApp.Models
{
    public class UserModel :BaseModel
    {
        public User NewUser;
        public string ConfirmPassword;

        public UserModel()
        {
            NewUser = new User(ConnString);
            ConfirmPassword = String.Empty;
        }

        public UserModel(User user)
        {
            CurrentUser = user;
        }
    }
}