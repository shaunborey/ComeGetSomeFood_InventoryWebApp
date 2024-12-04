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
    public class User
    {
        private int _UserID;
        public int UserID
        {
            get { return _UserID; }
        }

        private UserType _UserType;
        public UserType Type
        {
            get { return _UserType; }
            set { _UserType = value; }
        }

        private string _UserName;
        public string UserName
        {
            get { return _UserName; }
            set { _UserName = value; }
        }

        private string _Password;
        public string Password
        {
            get { return _Password; }
        }

        private string _FirstName;
        public string FirstName
        {
            get { return _FirstName; }
            set { _FirstName = value; }
        }

        private string _LastName;
        public string LastName
        {
            get { return _LastName; }
            set { _LastName = value; }
        }

        private string _EmailAddress;
        public string EmailAddress
        {
            get { return _EmailAddress; }
            set { _EmailAddress = value; }
        }

        private bool _IsAuthenticated;
        public bool IsAuthenticated
        {
            get { return _IsAuthenticated; }
        }

        private string _ConnString;

        public enum UserType { Owner, Employee }

        public User(string connString)
        {
            _ConnString = connString;
        }

        public User(string userName, string pw, string connString)
        {
            _UserName = userName;
            _Password = Crypto.Encrypt(pw);
            _ConnString = connString;
            _IsAuthenticated = false;
        }

        public User(UserType type, string userName, string pw, string fName, string lName, string email, string connString)
        {
            _UserType = type;
            _UserName = userName;
            _Password = Crypto.Encrypt(pw);
            _FirstName = fName;
            _LastName = lName;
            _EmailAddress = email;
            _ConnString = connString;
            _IsAuthenticated = false;
        }

        public bool AddUser(out string resultMsg)
        {
            try
            {
                if (AlreadyExists())
                {
                    resultMsg = "The username already exists";
                    return false;
                }
                else
                {
                    string sql = @"INSERT INTO Users (UserType, UserName, Password, FirstName, LastName, EMailAddress) 
                                   VALUES (@type, @username, @pw, @fname, @lname, @email)";

                    SqlParameter[] p = new SqlParameter[] {
                    new SqlParameter("@type", (int)_UserType),
                    new SqlParameter("@username", _UserName),
                    new SqlParameter("@pw", _Password),
                    new SqlParameter("@fname", _FirstName),
                    new SqlParameter("@lname", _LastName),
                    new SqlParameter("@email", _EmailAddress)
                };

                    SqlHelper.ExecuteNonQuery(_ConnString, CommandType.Text, sql, p);
                    resultMsg = "User has been added successfully";
                    return true;
                }
            }
            catch (Exception ex)
            {
                resultMsg = ex.Message;
                return false;
            }
        
        }

        public bool DeleteUser(out string resultMsg)
        {
            try
            {
                string sql = "DELETE FROM Users WHERE UserID = @userid";
                SqlParameter[] p = new SqlParameter[] {
                    new SqlParameter("@userid", _UserID)
                };

                SqlHelper.ExecuteNonQuery(_ConnString, CommandType.Text, sql, p);
                resultMsg = "User account has been deleted successfully";
                return true;
            }
            catch (Exception ex)
            {
                resultMsg = ex.Message;
                return false;
            }
        
        }

        public bool SetPassword(string oldPW, string newPW, out string resultMsg)
        {
            try
            {
                if (Crypto.Encrypt(oldPW) != _Password)
                {
                    resultMsg = "Password provided is incorrect";
                    return false;
                }
                else
                {
                    _Password = Crypto.Encrypt(newPW);
                    if (Save(false, out resultMsg))
                    {
                        resultMsg = "Password updated successfully";
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

            }
            catch (Exception ex)
            {
                resultMsg = ex.Message;
                return false;
            }
        }

        public bool AlreadyExists()
        {
            string sql = "SELECT UserID FROM Users WHERE UserName = @user";
            SqlParameter[] p = new SqlParameter[] {
                new SqlParameter("@user", _UserName)
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
                    resultMsg = "The username is already in use";
                    return false;
                }

                sql = @"INSERT INTO Users (UserType, UserName, Password, FirstName, LastName, EmailAddress)
                        VALUES (@usertype, @username, @password, @firstname, @lastname, @email)";
            }
            else
            {
                sql = @"UPDATE Users SET UserName = @username, Password = @password, FirstName = @firstname, 
                        LastName = @lastname, EMailAddress = @email WHERE UserID = @id";
            }

            SqlParameter[] p = new SqlParameter[] {
                new SqlParameter("@username", _UserName),
                new SqlParameter("@password", _Password),
                new SqlParameter("@firstname", _FirstName),
                new SqlParameter("@lastname", _LastName),
                new SqlParameter("@email", _EmailAddress)
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

        public bool Login(out string resultMsg)
        {
            try
            {
                string sql = "SELECT * FROM Users WHERE UserName = @username AND Password = @pw";
                SqlParameter[] p = new SqlParameter[] {
                new SqlParameter("@username", _UserName),
                new SqlParameter("@pw", _Password)
                };

                DataTable dt = SqlHelper.ExecuteDataset(_ConnString, CommandType.Text, sql, p).Tables[0];

                    if (dt.Rows.Count == 0)
                    {
                        resultMsg = "Login Failed: The provided Username or Password is incorrect";
                        _Password = String.Empty;
                        return false;
                    }
                    else
                    {
                        DataRow dr = dt.Rows[0];
                        _UserID = (int)dr["UserID"];
                        _UserType = (UserType)dr["UserType"];
                        _FirstName = dr["FirstName"].ToString();
                        _LastName = dr["LastName"].ToString();
                        _EmailAddress = dr["EMailAddress"].ToString();
                        _IsAuthenticated = true;
                    }

                    resultMsg = "Login Successful";
                    return true;
                
            }
            catch (Exception ex)
            {
                resultMsg = ex.Message;
                return false;
            }
        }

        public bool LoadUserDetails(int userID)
        {
            try
            {
                string sql = "SELECT * FROM Users WHERE UserID = @userid";
                SqlParameter[] p = new SqlParameter[] {
                new SqlParameter("@userid", userID)
                };

                DataRow dr = SqlHelper.ExecuteDataset(_ConnString, CommandType.Text, sql, p).Tables[0].Rows[0];
                _UserID = userID;
                _UserName = dr["UserName"].ToString();
                _UserType = (UserType)dr["UserType"];
                _Password = dr["Password"].ToString();
                _FirstName = dr["FirstName"].ToString();
                _LastName = dr["LastName"].ToString();
                _EmailAddress = dr["EMailAddress"].ToString();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
