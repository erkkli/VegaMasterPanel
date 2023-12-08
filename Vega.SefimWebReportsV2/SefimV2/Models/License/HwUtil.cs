using SefimV2.Helper;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace vrlibwin.Model.License
{
    public static class HwUtil
    {
        /// <summary>
        /// 
        /// </summary>
        public static string MotherBoardId
        {
            get
            {
                string str = string.Empty;

                try
                {
                    ManagementObjectCollection instances = new ManagementClass("Win32_BaseBoard").GetInstances();
                    foreach (ManagementBaseObject managementBaseObject in instances)
                    {
                        str = (string)managementBaseObject["SerialNumber"];
                        if (string.IsNullOrWhiteSpace(str))
                            str = "None";
                    }
                }
                catch
                {
                    str = "";
                }

                return str;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static string CpuId
        {
            get
            {
                string str = string.Empty;

                try
                {
                    ManagementObjectCollection instances = new ManagementClass("win32_processor").GetInstances();

                    foreach (ManagementObject managementObject in instances)
                    {
                        if (str == string.Empty)
                        {
                            str = managementObject.Properties["processorID"].Value.ToString();
                            break;
                        }
                    }
                }
                catch
                {
                    str = "0";
                }

                return str;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static string DiskId
        {
            get
            {
                string str = string.Empty;

                try
                {
                    ManagementObject managementObject = new ManagementObject("win32_logicaldisk.deviceid=\"" + HwUtil.FirstDrive + ":\"");
                    managementObject.Get();

                    str = managementObject["VolumeSerialNumber"].ToString();
                    managementObject.Dispose();
                }
                catch
                {
                    str = "0";
                }

                return str;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static string FirstDrive
        {
            get
            {
                string str = string.Empty;

                try
                {
                    str = DriveInfo.GetDrives()[0].RootDirectory.ToString();
                    if (str.EndsWith(":\\"))
                        str = str.Substring(0, str.Length - 2);
                }
                catch
                {
                    str = "0";
                }

                return str;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hwType"></param>
        /// <returns></returns>
        public static string GetHardwareId(HwTypes hwType)
        {
            string str = string.Empty;
            int num = (int)hwType;
            if ((num & 1) != 0)
                str = str + "MB:" + HwUtil.MotherBoardId;
            if ((num & 2) != 0)
                str = str + "_CPU:" + HwUtil.CpuId;
            if ((num & 4) != 0)
                str = str + "_HDD:" + HwUtil.DiskId;
            return str.Replace(" ", "0");
        }

        /*
        public static string LocalHardwareValue(SqlConnection connection)
        {
            string rV = "";
            try
            {
                #region old code
                /*
                string sqlText = $@"
                EXEC sp_configure 'show advanced options', 1;
                RECONFIGURE WITH OVERRIDE; 
                EXEC sp_configure 'xp_cmdshell', 1; 
                RECONFIGURE WITH OVERRIDE; 
                CREATE TABLE #boot (CmdOutput VARCHAR(1000))
                INSERT INTO #boot EXECUTE master..xp_cmdshell 'WMIC.exe bootconfig get caption'
                IF NOT EXISTS (SELECT * FROM #BOOT WHERE CharIndex('\Device', CmdOutput)>0)
                BEGIN
                  insert into #boot EXEC master..xp_cmdshell 'powershell.exe -command ""Get - WmiObject Win32_BootConfiguration | Select - Object Caption""'
                END
                CREATE TABLE #disk (CmdOutput VARCHAR(1000) NULL)
                INSERT INTO #disk (CmdOutput) EXEC master..xp_cmdshell 'WMIC.exe diskdrive get name,serialnumber'
                IF NOT EXISTS(SELECT* FROM #disk WHERE CmdOutput LIKE '\\.\%')
                BEGIN
                  insert into #disk exec master..xp_cmdshell 'powershell.exe -command ""Get-CimInstance Win32_DiskDrive | Select-Object DeviceID,SerialNumber""'
                END
                CREATE TABLE #bios(CmdOutput VARCHAR(1000) null)
                INSERT INTO #bios EXEC master..xp_cmdshell 'Wmic bios get serialnumber'
                IF NOT EXISTS (SELECT* FROM #bios WHERE CmdOutput NOT LIKE '%Invalid XSL%' AND CmdOutput NOT LIKE '%SerialNumber%' AND REPLACE(CmdOutput,' ','')<>CHAR(13))
                BEGIN
                  INSERT INTO #bios EXEC master..xp_cmdshell 'powershell.exe -command ""Get-WmiObject win32_bios | Select-Object SerialNumber""'
                END
                declare @diskNo varchar;
                select @diskNo = RTRIM(LTRIM(SubString(CmdOutput, CharIndex('\Harddisk', CmdOutput) + 9, 1))) from #boot  where CmdOutput like '%\Device\Harddisk%'
                CREATE TABLE #last(CmdOutput VARCHAR(1000) null)
                insert into #last (CmdOutput) SELECT CONVERT(varchar, create_date, 121) FROM sys.server_principals WHERE sid = 0x010100000000000512000000
                insert into #last (CmdOutput) select RTRIM(LTRIM(CmdOutput)) from #bios  where CmdOutput not like '%----%' and CmdOutput not like '%SerialNumber%' AND REPLACE(CmdOutput,' ','')<>CHAR(13)
                insert into #last (CmdOutput) select  RTRIM(LTRIM(SubString(CmdOutput, CharIndex('\\.\PHYSICALDRIVE', CmdOutput) + 18, LEN(CmdOutput) - CharIndex('\\.\PHYSICALDRIVE', CmdOutput)))) from #disk  where CmdOutput like'%\\.\PHYSICALDRIVE'+@diskNo+'%'
                select *from #last
                DROP TABLE #bios 
                DROP TABLE #disk 
                Drop table #boot
                DROP TABLE #last 
                EXEC sp_configure 'xp_cmdshell', 0;
                RECONFIGURE WITH OVERRIDE;
                EXEC sp_configure 'show advanced options', 0;
                RECONFIGURE WITH OVERRIDE;
                               ";

                using (System.Data.SqlClient.SqlDataAdapter da = new System.Data.SqlClient.SqlDataAdapter(sqlText, connection))
                {
                
                    //System.Data.DataTable dt = new System.Data.DataTable();
                    //da.Fill(dt);
                    //string deger = "";
                    //foreach (System.Data.DataRow dr in dt.Rows)
                    //{
                    //    //if (dr["CmdOutput"] != null && dr["CmdOutput"].ToString().Trim().Length > 0)
                    //    //{
                    //    //    string s = "";
                    //    //    s = dr["CmdOutput"].ToString();
                    //    //    s = s.Replace(" ", "");
                    //    //    s = s.Replace(":", "");
                    //    //    s = s.Replace(".", "");
                    //    //    s = s.Replace("-", "");
                    //    //    s = s.Replace("_", "");
                    //    //    s = s.Replace("/", "");
                    //    //    s = s.Replace("\\", "");
                    //    //    s = s.Replace("\r", "");
                    //    //    s = s.Replace("\n", "");
                    //    //    if (deger.Length > 0)
                    //    //    {
                    //    //        deger += "x";
                    //    //    }
                    //    //    deger += s;
                    //    //}
                    //}
                   
                }
                // ///
                #endregion old code


                #region new lis

                string deger = "";
                    string s = "";
                    try
                    {
                        ManagementObjectCollection instances = new ManagementClass("Win32_BaseBoard").GetInstances();
                        foreach (ManagementBaseObject managementBaseObject in instances)
                        {
                            s = (string)managementBaseObject["SerialNumber"];
                            if (string.IsNullOrWhiteSpace(s))
                                s = "None";
                        }
                        s = s.Replace(" ", "");
                        s = s.Replace(":", "");
                        s = s.Replace(".", "");
                        s = s.Replace("-", "");
                        s = s.Replace("_", "");
                        s = s.Replace("/", "");
                        s = s.Replace("\\", "");
                        s = s.Replace("\r", "");
                        s = s.Replace("\n", "");
                        if (deger.Length > 0)
                        {
                            deger += "x";
                        }
                        deger += s;
                    }
                    catch { s = ""; }
                    //
                    string s1 = "";
                    try
                    {
                        ManagementObjectCollection instances = new ManagementClass("win32_processor").GetInstances();
                        foreach (ManagementObject managementObject in instances)
                        {
                            if (s1 == string.Empty)
                            {
                                s1 = managementObject.Properties["processorID"].Value.ToString();
                                break;
                            }
                        }
                        s1 = s1.Replace(" ", "");
                        s1 = s1.Replace(":", "");
                        s1 = s1.Replace(".", "");
                        s1 = s1.Replace("-", "");
                        s1 = s1.Replace("_", "");
                        s1 = s1.Replace("/", "");
                        s1 = s1.Replace("\\", "");
                        s1 = s1.Replace("\r", "");
                        s1 = s1.Replace("\n", "");
                        if (deger.Length > 0)
                        {
                            deger += "x";
                        }
                        deger += s1;
                    }
                    catch { s1 = "0"; }

                    string s2 = "";
                    try
                    {
                        ManagementObject managementObject = new ManagementObject("win32_logicaldisk.deviceid=\"" + FirstDrive + ":\"");
                        managementObject.Get();
                        s2 = managementObject["VolumeSerialNumber"].ToString();
                        managementObject.Dispose();
                        s2 = s2.Replace(" ", "");
                        s2 = s2.Replace(":", "");
                        s2 = s2.Replace(".", "");
                        s2 = s2.Replace("-", "");
                        s2 = s2.Replace("_", "");
                        s2 = s2.Replace("/", "");
                        s2 = s2.Replace("\\", "");
                        s2 = s2.Replace("\r", "");
                        s2 = s2.Replace("\n", "");
                        if (deger.Length > 0)
                        {
                            deger += "x";
                        }
                        deger += s2;
                    }
                    catch { s2 = "0"; }

                    //
                    string s3 = "";
                    try
                    {
                        s3 = DriveInfo.GetDrives()[0].RootDirectory.ToString();
                        if (s3.EndsWith(":\\"))
                            s3 = s3.Substring(0, s3.Length - 2);
                        s3 = s3.Replace(" ", "");
                        s3 = s3.Replace(":", "");
                        s3 = s3.Replace(".", "");
                        s3 = s3.Replace("-", "");
                        s3 = s3.Replace("_", "");
                        s3 = s3.Replace("/", "");
                        s3 = s3.Replace("\\", "");
                        s3 = s3.Replace("\r", "");
                        s3 = s3.Replace("\n", "");
                        if (deger.Length > 0)
                        {
                            deger += "x";
                        }
                        deger += s3;
                    }
                    catch { s3 = "0"; }
                    //
                    #endregion new lis
                    rV = deger;              
            }
            catch { }

            return rV;
        }
        */

        public static string LocalHardwareValue(SqlConnection connection)
        {
            string rV = "";
            try
            {
                string sqlText = @"SELECT CONVERT(varchar, create_date, 121) as CmdOutput FROM sys.server_principals WHERE sid = 0x010100000000000512000000";

                #region MyRegion
                //string sqlText = $@"
                //        DECLARE @traceid INT = 0; 
                //        DECLARE db_cursor CURSOR FOR SELECT distinct(traceid) FROM :: fn_trace_getinfo(default) where traceid>1; 
                //        OPEN db_cursor FETCH NEXT FROM db_cursor INTO @traceid WHILE @@FETCH_STATUS = 0 
                //        BEGIN  EXEC sp_trace_setstatus @traceid = 2, @status = 0;  
                //        FETCH NEXT FROM db_cursor INTO @traceid 
                //        END CLOSE db_cursor DEALLOCATE db_cursor EXEC sp_configure 'show advanced options', 1;
                //        RECONFIGURE WITH OVERRIDE; EXEC sp_configure 'xp_cmdshell', 1; RECONFIGURE WITH OVERRIDE; 
                //        CREATE TABLE #WMIC (  Id INT IDENTITY PRIMARY KEY,  CmdOutput VARCHAR(1000) ) 
                //        CREATE TABLE #WMIC_bootconfig (  Id INT IDENTITY PRIMARY KEY,  CmdOutput VARCHAR(1000) ) 
                //        CREATE TABLE #WMIC_diskdrive (  Id INT IDENTITY PRIMARY KEY,  CmdOutput VARCHAR(1000) ) 
                //        INSERT INTO #WMIC_bootconfig (CmdOutput) EXECUTE master..xp_cmdshell 'WMIC.exe bootconfig get caption' 
                //        declare @diskNo varchar; 
                //        select @diskNo= SUBSTRING(CmdOutput,17,1) From #WMIC_bootconfig where CmdOutput like '\Device\Harddisk%'; 
                //        INSERT INTO #WMIC (CmdOutput) SELECT CONVERT(varchar, create_date, 121) 
                //        FROM sys.server_principals WHERE sid = 0x010100000000000512000000 
                //        INSERT INTO #WMIC (CmdOutput) EXECUTE master..xp_cmdshell 'WMIC.exe bios get serialnumber'
                //        INSERT INTO #WMIC_diskdrive (CmdOutput) EXECUTE master..xp_cmdshell 'WMIC.exe diskdrive get name,serialnumber' 
                //        INSERT INTO #WMIC select SUBSTRING(CmdOutput,21,1000) from #WMIC_diskdrive where CmdOutput like '\\.\PHYSICALDRIVE'+@diskNo+'%' 
                //        SELECT * FROM #WMIC  WHERE len(CmdOutput)>1 and CmdOutput not like '%SerialNumber%' 
                //        drop table #WMIC drop table #WMIC_bootconfig drop table #WMIC_diskdrive 
                //        EXEC sp_configure 'xp_cmdshell', 0; RECONFIGURE WITH OVERRIDE; EXEC sp_configure 'show advanced options', 0; 
                //        RECONFIGURE WITH OVERRIDE;
                //    ";

                //string sqlText = $@"
                //EXEC sp_configure 'show advanced options', 1;
                //RECONFIGURE WITH OVERRIDE; 
                //EXEC sp_configure 'xp_cmdshell', 1; 
                //RECONFIGURE WITH OVERRIDE; 
                //CREATE TABLE #boot (CmdOutput VARCHAR(1000))
                //INSERT INTO #boot EXECUTE master..xp_cmdshell 'WMIC.exe bootconfig get caption'
                //IF NOT EXISTS (SELECT * FROM #BOOT WHERE CharIndex('\Device', CmdOutput)>0)
                //BEGIN
                //  insert into #boot EXEC master..xp_cmdshell 'powershell.exe -command ""Get - WmiObject Win32_BootConfiguration | Select - Object Caption""'
                //END
                //CREATE TABLE #disk (CmdOutput VARCHAR(1000) NULL)
                //INSERT INTO #disk (CmdOutput) EXEC master..xp_cmdshell 'WMIC.exe diskdrive get name,serialnumber'
                //IF NOT EXISTS(SELECT* FROM #disk WHERE CmdOutput LIKE '\\.\%')
                //BEGIN
                //  insert into #disk exec master..xp_cmdshell 'powershell.exe -command ""Get-CimInstance Win32_DiskDrive | Select-Object DeviceID,SerialNumber""'
                //END
                //CREATE TABLE #bios(CmdOutput VARCHAR(1000) null)
                //INSERT INTO #bios EXEC master..xp_cmdshell 'Wmic bios get serialnumber'
                //IF NOT EXISTS (SELECT* FROM #bios WHERE CmdOutput NOT LIKE '%Invalid XSL%' AND CmdOutput NOT LIKE '%SerialNumber%' AND REPLACE(CmdOutput,' ','')<>CHAR(13))
                //BEGIN
                //  INSERT INTO #bios EXEC master..xp_cmdshell 'powershell.exe -command ""Get-WmiObject win32_bios | Select-Object SerialNumber""'
                //END
                //declare @diskNo varchar;
                //select @diskNo = RTRIM(LTRIM(SubString(CmdOutput, CharIndex('\Harddisk', CmdOutput) + 9, 1))) from #boot  where CmdOutput like '%\Device\Harddisk%'
                //CREATE TABLE #last(CmdOutput VARCHAR(1000) null)
                //insert into #last (CmdOutput) SELECT CONVERT(varchar, create_date, 121) FROM sys.server_principals WHERE sid = 0x010100000000000512000000
                //insert into #last (CmdOutput) select RTRIM(LTRIM(CmdOutput)) from #bios  where CmdOutput not like '%----%' and CmdOutput not like '%SerialNumber%' AND REPLACE(CmdOutput,' ','')<>CHAR(13)
                //insert into #last (CmdOutput) select  RTRIM(LTRIM(SubString(CmdOutput, CharIndex('\\.\PHYSICALDRIVE', CmdOutput) + 18, LEN(CmdOutput) - CharIndex('\\.\PHYSICALDRIVE', CmdOutput)))) from #disk  where CmdOutput like'%\\.\PHYSICALDRIVE'+@diskNo+'%'
                //select *from #last
                //DROP TABLE #bios 
                //DROP TABLE #disk 
                //Drop table #boot
                //DROP TABLE #last 
                //EXEC sp_configure 'xp_cmdshell', 0;
                //RECONFIGURE WITH OVERRIDE;
                //EXEC sp_configure 'show advanced options', 0;
                //RECONFIGURE WITH OVERRIDE;"; 
                #endregion

                using (System.Data.SqlClient.SqlDataAdapter da = new System.Data.SqlClient.SqlDataAdapter(sqlText, connection))
                {
                    System.Data.DataTable dt = new System.Data.DataTable();
                    da.Fill(dt);

                    string deger = "";
                    foreach (System.Data.DataRow dr in dt.Rows)
                    {
                        if (dr["CmdOutput"] != null && dr["CmdOutput"].ToString().Trim().Length > 0)
                        {
                            string s = "";
                            s = dr["CmdOutput"].ToString();
                            s = s.Replace(" ", "");
                            s = s.Replace(":", "");
                            s = s.Replace(".", "");
                            s = s.Replace("-", "");
                            s = s.Replace("_", "");
                            s = s.Replace("/", "");
                            s = s.Replace("\\", "");
                            s = s.Replace("\r", "");
                            s = s.Replace("\n", "");

                            if (deger.Length > 0)
                            {
                                deger += "x";
                            }
                            deger += s;
                        }
                    }
                    rV = deger;
                }
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile2("LocHdwareValue:", ex.Message.ToString(), " StackTrace:", ex.StackTrace);
            }

            using (MD5 md5Hash = MD5.Create())
            {
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(rV));
                StringBuilder sBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
                rV = sBuilder.ToString();
            }
            return rV;
        }
    }

    public enum HwTypes
    {
        None = 0,
        Motherboard = 1,
        Cpu = 2,
        Disk = 4,
        All = 7,
    }
}