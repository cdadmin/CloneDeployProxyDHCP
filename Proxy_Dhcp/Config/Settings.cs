using System;
using System.IO;
using System.Net;
using System.Text;
using CloneDeploy_Proxy_Dhcp.Helpers;

namespace CloneDeploy_Proxy_Dhcp.Config
{
    class Settings
    {
        public static string Nic { get; private set; }
        public static string NextServer { get; private set; }
        public static bool ListenDiscover { get; private set; }
        public static bool ListenProxy { get; private set; }
        public static bool AllowAll { get; private set; }
        public static string CloneDeployServiceURL { get; private set; }
        public static string BiosBootFile { get; private set; }
        public static string Efi32BootFile { get; private set; }
        public static string Efi64BootFile { get; private set; }
        public static string AppleBootFile { get; private set; }
        public static string RootPath { get; private set; }
        public static string VendorInfo { get; private set; }
        public static bool ListenBSDP { get; private set; }
        public static string AppleEFIBootFile { get; private set; }
        public static string ServerIdentifierOverride { get; private set; }
        public static byte[] PXEClient { get; set; }
        public static byte[] AAPLBSDPC { get; set; }

        public void SetAll()
        {
            var isError = false;
           
            var reader = new IniReader();
            reader.CheckForConfig();

            Nic = reader.ReadConfig("settings","interface");
            NextServer = reader.ReadConfig("settings", "next-server");

            try
            {
                ListenDiscover = Convert.ToBoolean(reader.ReadConfig("settings", "listen-dhcp"));
            }
            catch (Exception)
            {
                Console.WriteLine("listen-dhcp Is Not Valid.  Valid Entries Include true or false");
                isError = true;
               
            }

            try
            {
                ListenProxy = Convert.ToBoolean(reader.ReadConfig("settings", "listen-proxy"));
            }
            catch (Exception)
            {
                Console.WriteLine("listen-proxy Is Not Valid.  Valid Entries Include true or false");
                isError = true;
            }

            try
            {
                AllowAll = Convert.ToBoolean(reader.ReadConfig("settings", "allow-all-mac"));
            }
            catch (Exception)
            {
                Console.WriteLine("allow-all-mac Is Not Valid.  Valid Entries Include true or false");
                isError = true;
            }

            PXEClient = Encoding.UTF8.GetBytes("PXEClient");
            AAPLBSDPC = Encoding.UTF8.GetBytes("AAPLBSDPC");

            CloneDeployServiceURL = reader.ReadConfig("settings", "clonedeploy-service-url");

            BiosBootFile = reader.ReadConfig("settings", "bios-bootfile");
            Efi32BootFile = reader.ReadConfig("settings", "efi32-bootfile");
            Efi64BootFile = reader.ReadConfig("settings", "efi64-bootfile");

            AppleBootFile = reader.ReadConfig("settings", "apple-boot-file");
            RootPath = reader.ReadConfig("settings", "apple-root-path");
            VendorInfo = reader.ReadConfig("settings", "apple-vendor-specific-information");
            ServerIdentifierOverride = reader.ReadConfig("settings", "server-identifier-override");
            try
            {
                ListenBSDP = Convert.ToBoolean(reader.ReadConfig("settings", "listen-apple-bsdp"));
            }
            catch (Exception)
            {
                Console.WriteLine("listen-apple-bsdp Is Not Valid.  Valid Entries Include true or false");
                isError = true;
            }


            AppleEFIBootFile = reader.ReadConfig("settings", "apple-efi-boot-file");
           
            if (!string.IsNullOrEmpty(Settings.CloneDeployServiceURL))
            {
                using (var client = new WebClient())
                {
                    try
                    {
                        client.DownloadString(Settings.CloneDeployServiceURL + "Test");
                    }
                    catch
                    {
                        Console.WriteLine("CloneDeploy Web Service Test Failed.  Web Reservations Will Not Be Processed");
                        CloneDeployServiceURL = null;
                    }
                }
            }
            else
            {
                Console.WriteLine("CloneDeploy Web Service Is Not Populated.  Web Reservations Will Not Be Processed");
            }

            Console.WriteLine();

            
            try
            {
                IPAddress.Parse(Nic);
            }
            catch
            {
                Console.WriteLine("Interface Is Not Valid.  Ensure The Interface Is A Valid IPv4 Address");
                isError = true;
            }

            try
            {
                IPAddress.Parse(NextServer);
            }
            catch
            {
                Console.WriteLine("Next-Server Is Not Valid.  Ensure The Next-Server Is A Valid IPv4 Address");
                isError = true;
            }

            if (ListenBSDP && !ListenDiscover)
            {
                Console.WriteLine("Cannot Listen For Apple BSDP Requests.  listen-dhcp Must Be true");
                isError = true;
            }

            if (Nic == "0.0.0.0" && string.IsNullOrEmpty(ServerIdentifierOverride))
            {
                Console.WriteLine("When the interface Is Set To Listen For Requests On Any IP, server-identifier-override Must Have An Address");
                isError = true;
            }

            if (Type.GetType("Mono.Runtime") != null)
            {
                if (Nic != "0.0.0.0")
                {
                    Console.WriteLine("When running on Mono the interface must be set to 0.0.0.0");
                    isError = true;
                }
                if (string.IsNullOrEmpty(ServerIdentifierOverride))
                {
                    Console.WriteLine("When running on Mono the server-identifier-override must have a value");
                    isError = true;
                }
            }

            if (isError)
            {
                Console.WriteLine("CloneDeploy Proxy DHCP Could Not Be Started");
                Console.WriteLine("Press [Enter] to Exit");
                Console.Read();
                Environment.Exit(1);
            }

            var rdr = new FileReader();
            if (!AllowAll)
            {
                if(File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"allow"))
                {
                    foreach (var mac in rdr.ReadFile("allow"))
                        DHCPServer.DHCPServer.AddAcl(PhysicalAddress.Parse(mac), false);
                }
            }

            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"deny"))
            {
                foreach (var mac in rdr.ReadFile("deny"))
                    DHCPServer.DHCPServer.AddAcl(PhysicalAddress.Parse(mac), true);
            }

            try
            {
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"reservations"))
                {
                    foreach (var reservation in rdr.ReadFile("reservations"))
                    {
                        var arrayReservation = reservation.Split(',');

                        var serveroptions = new DHCPServer.DHCPServer.ReservationOptions();
                        if (arrayReservation.Length == 4)
                            serveroptions.ReserveBCDFile = arrayReservation[3];
                        serveroptions.ReserveBootFile = arrayReservation[2];
                        serveroptions.ReserveNextServer = arrayReservation[1];
                        DHCPServer.DHCPServer.Reservations.Add(PhysicalAddress.Parse(arrayReservation[0]), serveroptions);
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Could Not Parse Local Reservations File.  Local Reservations Will Not Be Processed");
                DHCPServer.DHCPServer.Reservations.Clear();
            }
          
          
        }  
    }
}
