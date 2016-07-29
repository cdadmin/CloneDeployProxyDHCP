using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using CloneDeploy_Proxy_Dhcp.Config;
using CloneDeploy_Proxy_Dhcp.Helpers;

namespace CloneDeploy_Proxy_Dhcp.DHCPServer
{
    public class DHCPDataReceived
    {
        public void Process(DHCPRequest dhcpRequest)
        {
            var requestType = dhcpRequest.GetMsgType();
            Trace.WriteLine(requestType + " Request From " + Utility.ByteArrayToString(dhcpRequest.GetChaddr(), true) + " " +
                              dhcpRequest.GetSourceIP() + ":" + dhcpRequest.GetSourcePort());

            var clientHardwareAddress = new PhysicalAddress(dhcpRequest.GetChaddr());
            if (DHCPServer.AclList.ContainsKey(clientHardwareAddress) && !DHCPServer.AclList[clientHardwareAddress] ||
                !DHCPServer.AclList.ContainsKey(clientHardwareAddress) && !Settings.AllowAll)
            {
                Trace.WriteLine("Request Denied By ACL - Ignoring");
                return;
            }

            var vendorId = dhcpRequest.GetVendorOptionData();
            if (vendorId != null)
            {
                var strVendorId = Utility.ByteArrayToString(vendorId, true);
                Trace.WriteLine("Vendor Class Id " + strVendorId + " " + Encoding.Default.GetString(vendorId));

                if (requestType == DHCPMsgType.DHCPDISCOVER && strVendorId.StartsWith("505845436C69656E74"))
                //Expected Format: 505845436C69656E743A417263683A30303030303A554E44493A303032303031 (PXEClient:Arch:00000:UNDI:002001)
                    ProcessPXERequest(dhcpRequest);
                else if (requestType == DHCPMsgType.DHCPINFORM && strVendorId.StartsWith("4141504C4253445043") && Settings.ListenBSDP)
                //Expected Format: 4141504C42534450432F693338362F694D616331342C33 (AAPLBSDPC/i386/iMac14,3)
                {
                    var vendorSpecificInformation = dhcpRequest.GetVendorSpecificInformation();
                    if (vendorSpecificInformation != null)
                    {
                        var strVendorInformation = Utility.ByteArrayToString(vendorSpecificInformation,true);
                        if (strVendorInformation.Length >= 6)
                        {
                            switch (strVendorInformation.Substring(0, 6))
                            {
                                case "010101":
                                    SendAppleBootList(dhcpRequest);
                                    break;
                                case "010102":
                                    var interfaceHex =
                                        Utility.ByteArrayToString(
                                            string.IsNullOrEmpty(Settings.ServerIdentifierOverride)
                                                ? IPAddress.Parse(Settings.Nic).GetAddressBytes()
                                                : IPAddress.Parse(Settings.ServerIdentifierOverride).GetAddressBytes(),
                                            true);
                                    
                                    if (strVendorInformation.Contains(interfaceHex))
                                        SendSelectedNetBoot(dhcpRequest);
                                    else
                                        Trace.WriteLine("Different BSDP Server Targeted - Ignoring");
                                    break;
                                default:
                                    Trace.WriteLine("Not An Apple BSDP Request, Vendor Specific Information Mismatch - Ignoring");
                                    break;
                            }
                        }
                        else
                            Trace.WriteLine("Unexpected Vendor Specific Information - Ignoring");               
                    }
                    else
                        Trace.WriteLine("No Vendor Specific Information Supplied - Ignoring");                   
                }
                else
                    Trace.WriteLine("Request Is Not A PXE or NetBoot Request - Ignoring");
            }
            else
                Trace.WriteLine("No Vendor Class Id Supplied - Ignoring");

            Trace.WriteLine("");
        }

        static void ProcessPXERequest(DHCPRequest dhcpRequest)
        {
            Trace.WriteLine("Request Is A PXE Boot");
            var replyOptions = new DHCPReplyOptions();
            replyOptions.OtherOptions.Add(DHCPOption.Vendorclassidentifier, Encoding.UTF8.GetBytes("PXEClient"));
            var reply = new DHCPReply(dhcpRequest);
            reply.Send(DHCPMsgType.DHCPOFFER, replyOptions, 68);
        }

        static void SendAppleBootList(DHCPRequest dhcpRequest)
        {
            Trace.WriteLine("Request Is An Apple NetBoot");
            int bsdpPort = 68;
            var vendorSpecificInformation = OptionData.GetOptionData(DHCPOption.VendorSpecificInformation, dhcpRequest.requestData);
            var strVendorInformation = Utility.ByteArrayToString(vendorSpecificInformation, true);
            if (strVendorInformation.Length >= 21)
            {
                var isReturnPort = strVendorInformation.Substring(14, 4);
                if (isReturnPort == "0502")
                {
                    var returnPort = strVendorInformation.Substring(18, 4);
                    bsdpPort = Convert.ToInt32(returnPort, 16);
                }
            }

            var replyOptions = new DHCPReplyOptions();
            replyOptions.OtherOptions.Add(DHCPOption.Vendorclassidentifier, Encoding.UTF8.GetBytes("AAPLBSDPC"));
            replyOptions.OtherOptions.Add(DHCPOption.VendorSpecificInformation, Utility.StringToByteArray(Settings.VendorInfo));
            var reply = new DHCPReply(dhcpRequest);
            reply.Send(DHCPMsgType.DHCPACK, replyOptions, bsdpPort);
        }

        static void SendSelectedNetBoot(DHCPRequest dhcpRequest)
        {
            //This Reply is the client selecting which image they want to boot from
            Trace.WriteLine("Request Is An Apple NetBoot Selection");
            
            var vendorSpecificInformation = dhcpRequest.GetVendorSpecificInformation();
            var strVendorSpecificInformation = Utility.ByteArrayToString(vendorSpecificInformation,true);
            var imageIdHex = strVendorSpecificInformation.Substring(strVendorSpecificInformation.Length - 4);
            var targetNbi = Settings.RootPath.Replace("[nbi_id]", imageIdHex);
            var targetAppleBootFile = Settings.AppleBootFile.Replace("[nbi_id]", imageIdHex);
            var clientHardwareAddress = new PhysicalAddress(dhcpRequest.GetChaddr());

            var replyOptions = new DHCPReplyOptions();
            replyOptions.NextServer = IPAddress.Parse(Settings.NextServer);
            replyOptions.OtherOptions.Add(DHCPOption.Vendorclassidentifier, Encoding.UTF8.GetBytes("AAPLBSDPC"));
            replyOptions.OtherOptions.Add(DHCPOption.VendorSpecificInformation, Utility.StringToByteArray(Settings.VendorInfo));
            replyOptions.OtherOptions.Add(DHCPOption.RootPath, Encoding.UTF8.GetBytes(targetNbi));

            //Modification to allow both a clonedeploy linux and osx imaging environment to work simultaneously without two proxy dhcp servers running
            if (imageIdHex == "0F49" || imageIdHex == "98DB") //image ids of 3913 or 39131
                replyOptions.BootFileName = Settings.AppleEFIBootFile;
            else
                replyOptions.BootFileName = targetAppleBootFile;
            
            if (DHCPServer.Reservations.ContainsKey(clientHardwareAddress))
            {
                replyOptions.NextServer =
                    IPAddress.Parse(DHCPServer.Reservations[clientHardwareAddress].ReserveNextServer);
                replyOptions.BootFileName = DHCPServer.Reservations[clientHardwareAddress].ReserveBootFile;
            }

            var reply = new DHCPReply(dhcpRequest);
            reply.Send(DHCPMsgType.DHCPACK, replyOptions, 68);     
        } 
    }
}
