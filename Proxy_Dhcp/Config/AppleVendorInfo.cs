using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace CloneDeploy_Proxy_Dhcp.Config
{
    class AppleVendorInfo
    {
        private class Nbi
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }

    /*BSDP Options
     *Code      Length      Values                              Name                        Client Or Server
    -1-         1           1-List, 2-Select,3-Failed           Message Type                Both
    -2-         2           uint16 encoding                     Version                     Client
    -3-         4           ipaddr encoding                     Server Identifier           Server
    -4-         2           uint16 0-65535                      Server Priority             Server
    -5-         2           uint16 < 1024                       Reply Port                  Client
    -7-         4           single:1-4095 cluster:4096-65535    Default Image Id            Server
    -8-         4                                               Selected Image Id           Both
    -9-         255 Max                                         Image List                  Server
    */

        /*Example Vendor Option String Serving two netboot images. 1 named net and 1 named boot
        01:01:01:03:04:A2:33:4B:A0:04:02:FF:FF:07:04:01:00:00:89:09:11:01:00:00:89:03:6E:65:74:01:00:00:88:04:62:6F:6F:74
    
        01:01:                                        option 1
              01:                                     List
        03:04:                                        option 3
              A2:33:4B:A0:                            Ip address
        04:02:                                        option 4
              FF:FF:                                  Max Value
        07:04:                                        Option 7
              01:00:00:89:                            default image id - always starts with 01:00 for Netboot OSX
        09:11:                                        Option 9 Length = 5 * [number of images] + [sum of image names]
              01:00:00:89:                            [image id]
                              03:                     [image name length]
                                    6E:65:74:         [image name]
              01:00:00:88:                            [image id]
                              04:                     [image name length]
                                    62:6f:6f:74       [image name]
        */

        public void Generate()
        {
            Console.WriteLine();
            string output = string.Empty;
            var listNbis = new List<Nbi>();
            var reader = new IniReader();
            var vendorOptions = new StringBuilder();

            //Read in the config.ini nbi ids
            var netBootServer = reader.ReadConfig("vendor-specific-info-generator", "netboot-server-ip");
            for (int i = 1; i <= 5; i++)
            {
                var tmpId = reader.ReadConfig("vendor-specific-info-generator", "apple_nbi_id_" + i);
                var tmpName = reader.ReadConfig("vendor-specific-info-generator", "apple_nbi_name_" + i);
                if (!string.IsNullOrEmpty(tmpId) && !string.IsNullOrEmpty(tmpName))
                {
                    var nbi = new Nbi();
                    nbi.Id = tmpId;
                    nbi.Name = tmpName;
                    listNbis.Add(nbi);
                }              
            }
            if (listNbis.Count == 0) return;

            //Set the target netboot server ip address
            vendorOptions.Append("01:01:01:03:04:");
            IPAddress ip;
            try
            {
                ip = IPAddress.Parse(netBootServer);
            }
            catch
            {
                Console.WriteLine("Could Not Parse netboot-server-ip");
                return;
            }

            foreach (byte i in ip.GetAddressBytes())
                vendorOptions.Append(i.ToString("X2") + ":");

            //Set the default nbi and determine the total nbi list length
            vendorOptions.Append("04:02:FF:FF:07:04:");
            int rowCount = 0;
            int totalNameLength = 0;
            foreach (var nbi in listNbis)
            {
                rowCount++;
                if (rowCount == 1)
                {
                    vendorOptions.Append("01:00:");
                    vendorOptions.Append(Helpers.Utility.AddHexColons(Convert.ToInt32(nbi.Id).ToString("X4")));
                    vendorOptions.Append(":");
                }
                totalNameLength += nbi.Name.Length;
            }

            vendorOptions.Append("09:" + (5*rowCount + totalNameLength).ToString("X2"));
            vendorOptions.Append(":");

            //Add the nbis
            List<string> listIds = new List<string>();
            var counter = 1;
            foreach (var nbi in listNbis)
            {
                vendorOptions.Append("01:00:");
                var nbiIdHex = Convert.ToInt32(nbi.Id).ToString("X4");
                vendorOptions.Append(Helpers.Utility.AddHexColons(nbiIdHex));
                vendorOptions.Append(":");

                vendorOptions.Append(nbi.Name.Length.ToString("X2"));
                vendorOptions.Append(":");
                vendorOptions.Append(Helpers.Utility.StringToHex(nbi.Name));
                if(counter != listNbis.Count)
                    vendorOptions.Append(":");
                listIds.Add(nbiIdHex);

                if (nbiIdHex != "0F49" && nbiIdHex != "98DB")
                {
                    output += "Place The " + nbi.Name + " NBI NetBoot.dmg In A Folder Named " + nbiIdHex + " On Your Web Server" + Environment.NewLine;
                    output += "Place The " + nbi.Name + " NBI i386 Folder In A Folder Named " + nbiIdHex + " On Your TFTP Server" + Environment.NewLine;
                }
                counter++;
            }

            var duplicateIds = listIds.GroupBy(x => x)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key);

            if (duplicateIds.Any())
            {
                Console.WriteLine("The list cannot contain duplicate ids");
                return;
            }

            output += Environment.NewLine + "Copy The Following String To Your config.ini For The apple-vendor-specific-information Key" +
                      Environment.NewLine;
            output += vendorOptions.ToString();
            Console.WriteLine(output);

        }
    }
}
