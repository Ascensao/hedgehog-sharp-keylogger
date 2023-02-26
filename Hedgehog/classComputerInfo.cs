using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.NetworkInformation;
using System.Net;

namespace Hedgehog
{
    class classComputerInfo
    {
        public string getMACAddress()
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            String sMacAddress = string.Empty;
            foreach (NetworkInterface adapter in nics)
            {
                if (sMacAddress == String.Empty)// only return MAC Address from first card  
                {
                    IPInterfaceProperties properties = adapter.GetIPProperties();
                    sMacAddress = adapter.GetPhysicalAddress().ToString();
                }
            } return sMacAddress;
        }

        public string getUser()
        {
            /*
             * Environment.UserName
               Will Display format : 'Username'
             * 
             * System.Security.Principal.WindowsIdentity.GetCurrent().Name
               Will Display format : 'NetworkName\Username' or 'ComputerName\Username'
             */
            return Environment.UserName;
        }

        public string getRandomPassword()
        {
            Random random = new Random();
            int password = random.Next(1000, 9999);
            return password.ToString();
        }

        public string getComputerName()
        {
            return Environment.MachineName;
        }

        public string getExternalIp()
        {
            try
            {
                string externalIP;
                externalIP = (new WebClient()).DownloadString("http://whosip.com/getip.php/");
                return externalIP;
            }
            catch { return null; }
        }

    }
}
