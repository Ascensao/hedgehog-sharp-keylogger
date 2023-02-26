using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;



namespace Hedgehog
{
    class classFTP
    {

        public string ftpurl = @"xxxxxxx";
        const string ftpusername = "xxxxxxx";
        const string ftppassword = "xxxxxxxxx";
        classDatabase db = new classDatabase();
        classBeta beta = new classBeta();

        public void CaptureImage(bool showCursor, string _MACAddress, string _folder)
        {
            try
            {
                Point curPos = new Point(Cursor.Position.X, Cursor.Position.Y);
                Size curSize = new Size();
                curSize.Height = Cursor.Current.Size.Height;
                curSize.Width = Cursor.Current.Size.Width;

                Rectangle SelectionRectangle = Screen.GetBounds(Screen.GetBounds(Point.Empty));

                DateTime dt = DateTime.Now;
                string FileName = _MACAddress + "-" + String.Format("{0:s}", dt).Replace(":", "-") + ".jpg";
                string FilePath = _folder + FileName;

                using (Bitmap bitmap = new Bitmap(SelectionRectangle.Width, SelectionRectangle.Height))
                {

                    using (Graphics g = Graphics.FromImage(bitmap))
                    {

                        g.CopyFromScreen(Point.Empty, Point.Empty, SelectionRectangle.Size);

                        if (showCursor)
                        {

                            Rectangle cursorBounds = new Rectangle(curPos, curSize);
                            Cursors.Default.Draw(g, cursorBounds);
                        }

                    }
                    bitmap.Save(FilePath, ImageFormat.Jpeg);
                }
            }
            catch { beta.writeBeta("CATCH ERRROR: captureImage"); }
        }

        public bool UploadFile(string _FilePath, string _UploadPath)
        {
            System.IO.FileInfo _FileInfo = new System.IO.FileInfo(_FilePath);

            // Create FtpWebRequest object from the Uri provided
            System.Net.FtpWebRequest _FtpWebRequest = (System.Net.FtpWebRequest)System.Net.FtpWebRequest.Create(new Uri(_UploadPath));

            // Provide the WebPermission Credintials
            _FtpWebRequest.Credentials = new System.Net.NetworkCredential(ftpusername, ftppassword);

            // By default KeepAlive is true, where the control connection is not closed

            // after a command is executed.
            _FtpWebRequest.KeepAlive = false;

            // set timeout for 20 seconds
            _FtpWebRequest.Timeout = 20000;

            // Specify the command to be executed.
            _FtpWebRequest.Method = System.Net.WebRequestMethods.Ftp.UploadFile;

            // Specify the data transfer type.
            _FtpWebRequest.UseBinary = true;

            // Notify the server about the size of the uploaded file
            _FtpWebRequest.ContentLength = _FileInfo.Length;

            // The buffer size is set to 2kb
            int buffLength = 2048;

            byte[] buff = new byte[buffLength];

            // Opens a file stream (System.IO.FileStream) to read the file to be uploaded
            System.IO.FileStream _FileStream = _FileInfo.OpenRead();

            try
            {
                // Stream to which the file to be upload is written
                System.IO.Stream _Stream = _FtpWebRequest.GetRequestStream();

                // Read from the file stream 2kb at a time
                int contentLen = _FileStream.Read(buff, 0, buffLength);

                // Till Stream content ends

                while (contentLen != 0)
                {
                    // Write Content from the file stream to the FTP Upload Stream

                    _Stream.Write(buff, 0, contentLen);

                    contentLen = _FileStream.Read(buff, 0, buffLength);
                }

                // Close the file stream and the Request Stream

                _Stream.Close();

                _Stream.Dispose();

                _FileStream.Close();

                _FileStream.Dispose();
                return true;
            }
            catch
            {
                beta.writeBeta("CATCH ERRROR: uploadFile");
                return false; 
            }
        }

    }
}
