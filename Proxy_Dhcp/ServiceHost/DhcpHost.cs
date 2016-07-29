using System.ServiceProcess;
using CloneDeploy_Proxy_Dhcp.Config;

namespace CloneDeploy_Proxy_Dhcp.ServiceHost
{
    public partial class DhcpHost : ServiceBase
    {
        private readonly DHCPServer.DHCPServer _server;
        private readonly DHCPServer.DHCPServer _proxy;
        
        public DhcpHost(DHCPServer.DHCPServer server, DHCPServer.DHCPServer proxy)
        {
            InitializeComponent();
            _server = server;
            _proxy = proxy;
        }

        public void ManualStart(string[] args)
        {
            OnStart(args);
        }

        public void ManualStop()
        {
            OnStop();
        }

        protected override void OnStart(string[] args)
        {
            if (Settings.ListenDiscover)
                _server.Start();
            if (Settings.ListenProxy)
                _proxy.Start();
        }

        protected override void OnStop()
        {
            if (Settings.ListenDiscover)
                _server.Dispose();
            if (Settings.ListenProxy)
                _proxy.Dispose();
        }
    }
}
