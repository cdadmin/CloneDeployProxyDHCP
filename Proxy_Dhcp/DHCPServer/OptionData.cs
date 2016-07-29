﻿using System;

namespace CloneDeploy_Proxy_Dhcp.DHCPServer
{
    public class OptionData
    {
        /// <summary>
        /// Returns option content
        /// </summary>
        /// <param name="option">Option to retrieve</param>
        /// <param name="requestData">requested Data</param>
        /// <returns>Option content</returns>
        public static byte[] GetOptionData(DHCPOption option, DHCPMessage.Packet requestData)
        {
            int DHCPId = 0;
            byte DDataID, DataLength = 0;
            byte[] dumpData;

            DHCPId = (int)option;
            for (int i = 0; i < requestData.options.Length; i++)
            {
                DDataID = requestData.options[i];
                if (DDataID == (byte)DHCPOption.END_Option) break;
                if (DDataID == DHCPId)
                {
                    DataLength = requestData.options[i + 1];
                    dumpData = new byte[DataLength];
                    Array.Copy(requestData.options, i + 2, dumpData, 0, DataLength);
                    return dumpData;
                }
                else
                {
                    DataLength = requestData.options[i + 1];
                    i += 1 + DataLength;
                }
            }

            return null;
        }        
    }

    /// <summary>DHCP option enum</summary>
    public enum DHCPOption
    {
        /// <summary>Option 1</summary>
        SubnetMask = 1,
        /// <summary>Option 2</summary>
        TimeOffset = 2,
        /// <summary>Option 3</summary>
        Router = 3,
        /// <summary>Option 4</summary>
        TimeServer = 4,
        /// <summary>Option 5</summary>
        NameServer = 5,
        /// <summary>Option 6</summary>
        DomainNameServers = 6,
        /// <summary>Option 7</summary>
        LogServer = 7,
        /// <summary>Option 8</summary>
        CookieServer = 8,
        /// <summary>Option 9</summary>
        LPRServer = 9,
        /// <summary>Option 10</summary>
        ImpressServer = 10,
        /// <summary>Option 11</summary>
        ResourceLocServer = 11,
        /// <summary>Option 12</summary>
        HostName = 12,
        /// <summary>Option 13</summary>
        BootFileSize = 13,
        /// <summary>Option 14</summary>
        MeritDump = 14,
        /// <summary>Option 15</summary>
        DomainName = 15,
        /// <summary>Option 16</summary>
        SwapServer = 16,
        /// <summary>Option 17</summary>
        RootPath = 17,
        /// <summary>Option 18</summary>
        ExtensionsPath = 18,
        /// <summary>Option 19</summary>
        IpForwarding = 19,
        /// <summary>Option 20</summary>
        NonLocalSourceRouting = 20,
        /// <summary>Option 21</summary>
        PolicyFilter = 21,
        /// <summary>Option 22</summary>
        MaximumDatagramReAssemblySize = 22,
        /// <summary>Option 23</summary>
        DefaultIPTimeToLive = 23,
        /// <summary>Option 24</summary>
        PathMTUAgingTimeout = 24,
        /// <summary>Option 25</summary>
        PathMTUPlateauTable = 25,
        /// <summary>Option 26</summary>
        InterfaceMTU = 26,
        /// <summary>Option 27</summary>
        AllSubnetsAreLocal = 27,
        /// <summary>Option 28</summary>
        BroadcastAddress = 28,
        /// <summary>Option 29</summary>
        PerformMaskDiscovery = 29,
        /// <summary>Option 30</summary>
        MaskSupplier = 30,
        /// <summary>Option 31</summary>
        PerformRouterDiscovery = 31,
        /// <summary>Option 32</summary>
        RouterSolicitationAddress = 32,
        /// <summary>Option 33</summary>
        StaticRoute = 33,
        /// <summary>Option 34</summary>
        TrailerEncapsulation = 34,
        /// <summary>Option 35</summary>
        ARPCacheTimeout = 35,
        /// <summary>Option 36</summary>
        EthernetEncapsulation = 36,
        /// <summary>Option 37</summary>
        TCPDefaultTTL = 37,
        /// <summary>Option 38</summary>
        TCPKeepaliveInterval = 38,
        /// <summary>Option 39</summary>
        TCPKeepaliveGarbage = 39,
        /// <summary>Option 40</summary>
        NetworkInformationServiceDomain = 40,
        /// <summary>Option 41</summary>
        NetworkInformationServers = 41,
        /// <summary>Option 42</summary>
        NetworkTimeProtocolServers = 42,
        /// <summary>Option 43</summary>
        VendorSpecificInformation = 43,
        /// <summary>Option 44</summary>
        NetBIOSoverTCPIPNameServer = 44,
        /// <summary>Option 45</summary>
        NetBIOSoverTCPIPDatagramDistributionServer = 45,
        /// <summary>Option 46</summary>
        NetBIOSoverTCPIPNodeType = 46,
        /// <summary>Option 47</summary>
        NetBIOSoverTCPIPScope = 47,
        /// <summary>Option 48</summary>
        XWindowSystemFontServer = 48,
        /// <summary>Option 49</summary>
        XWindowSystemDisplayManager = 49,
        /// <summary>Option 50</summary>
        RequestedIPAddress = 50,
        /// <summary>Option 51</summary>
        IPAddressLeaseTime = 51,
        /// <summary>Option 52</summary>
        OptionOverload = 52,
        /// <summary>Option 53</summary>
        DHCPMessageTYPE = 53,
        /// <summary>Option 54</summary>
        ServerIdentifier = 54,
        /// <summary>Option 55</summary>
        ParameterRequestList = 55,
        /// <summary>Option 56</summary>
        Message = 56,
        /// <summary>Option 57</summary>
        MaximumDHCPMessageSize = 57,
        /// <summary>Option 58</summary>
        RenewalTimeValue_T1 = 58,
        /// <summary>Option 59</summary>
        RebindingTimeValue_T2 = 59,
        /// <summary>Option 60</summary>
        Vendorclassidentifier = 60,
        /// <summary>Option 61</summary>
        ClientIdentifier = 61,
        /// <summary>Option 62</summary>
        NetWateIPDomainName = 62,
        /// <summary>Option 63</summary>
        NetWateIPInformation = 63,
        /// <summary>Option 64</summary>
        NetworkInformationServicePlusDomain = 64,
        /// <summary>Option 65</summary>
        NetworkInformationServicePlusServers = 65,
        /// <summary>Option 66</summary>
        TFTPServerName = 66,
        /// <summary>Option 67</summary>
        BootfileName = 67,
        /// <summary>Option 68</summary>
        MobileIPHomeAgent = 68,
        /// <summary>Option 69</summary>
        SMTPServer = 69,
        /// <summary>Option 70</summary>
        POP3Server = 70,
        /// <summary>Option 71</summary>
        NNTPServer = 71,
        /// <summary>Option 72</summary>
        DefaultWWWServer = 72,
        /// <summary>Option 73</summary>
        DefaultFingerServer = 73,
        /// <summary>Option 74</summary>
        DefaultIRCServer = 74,
        /// <summary>Option 75</summary>
        StreetTalkServer = 75,
        /// <summary>Option 76</summary>
        STDAServer = 76,
        /// <summary>Option 82</summary>
        RelayInfo = 82,
        /// <summary>Option 93</summary>
        ClientSystemArchitecture = 93,
        /// <summary>Option 121</summary>
        StaticRoutes = 121,
        /// <summary>Option 249</summary>
        StaticRoutesWin = 249,
        /// <summary>Option 252</summary>
        Wpad = 252,
        /// <summary>Option 255 (END option)</summary>
        END_Option = 255
    }
}
