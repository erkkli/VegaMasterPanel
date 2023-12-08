using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Web.Configuration;

namespace vrlibwin.Data
{
    public static class ConnectionProvider
    {
        private static SqlConnection licCon;
        private static SqlConnection masterCon;
        private static System.Timers.Timer contimer;

        public static void ConnectionOpen()
        {
            contimer = new System.Timers.Timer();
            contimer.Enabled = true;
            contimer.Interval = 840000;
            contimer.Elapsed += Contimer_Elapsed;

            var macAddr =
            (
                from nic in NetworkInterface.GetAllNetworkInterfaces()
                where nic.OperationalStatus == OperationalStatus.Up
                select nic.GetPhysicalAddress().ToString()
            ).FirstOrDefault();


            licCon = new SqlConnection(getLocalConnectionString() + "Application Name=SefimReport;");
            licCon.Open();
        }

        public static SqlConnection MasterConOpen()
        {
            var dataSource = getLocalConnectionString().Split(';');
            var masterConString = "";
            foreach (var item in dataSource)
            {
                if (item.Contains("Initial Catalog"))
                {
                    masterConString += "Initial Catalog=master;";
                    continue;
                }

                masterConString += item + ";";
            }

            masterConString = masterConString.Substring(0, masterConString.Length - 1);
            masterCon = new SqlConnection(masterConString);
            masterCon.Open();
            return masterCon;
        }

        private static void Contimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (licCon.State == ConnectionState.Open)
            {
                licCon.Close();
                licCon.Open();
            }
            else
            {
                licCon.Open();
            }

            if (masterCon.State == ConnectionState.Open)
            {
                masterCon.Close();
                masterCon.Open();
            }
            else
            {
                masterCon.Open();
            }

        }

        private static string getLocalConnectionString()
        {
            var SqlConnString = "Provider=SQLOLEDB;Data Source=" + WebConfigurationManager.AppSettings["Server"] + ";Initial Catalog=" + WebConfigurationManager.AppSettings["DBName"] + ";Persist Security Info=True;User Id=" + WebConfigurationManager.AppSettings["User"] + ";Password=" + WebConfigurationManager.AppSettings["Password"] + "; MultipleActiveResultSets=true;";
            return SqlConnString;
            //if (File.Exists(getConnectionStringFileName()))
            //    return File.ReadAllText(getConnectionStringFileName());
            //throw new ApplicationException("Connection string sağlanmamış. Dosya yolu:" + getConnectionStringFileName());
        }

        //        private static string getConnectionStringFileName()
        //        {
        //            string retval = Path.GetDirectoryName(Application.ExecutablePath);
        //            if (!retval.EndsWith(Path.DirectorySeparatorChar.ToString()))
        //                retval += Path.DirectorySeparatorChar;
        //            string fileName = "connectionstring.txt";
        //#if DEBUG
        //            fileName = "connectionstring.debug.txt";
        //#endif
        //            return retval + fileName;
        //        }
    }
}
