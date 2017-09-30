using CloneDeploy_ApiCalls;
using CloneDeploy_Proxy_Dhcp.Dtos;
using RestSharp;

namespace CloneDeploy_Proxy_Dhcp.ApiCalls
{
    public class ProxyDhcpApi
    {
        private readonly RestRequest _request;
        private readonly string _resource;

        public ProxyDhcpApi(string resource)
        {
            _request = new RestRequest();
            _resource = resource;
        }

        public ProxyReservationDTO GetProxyReservation(string mac)
        {
            _request.Method = Method.GET;
            _request.Resource = string.Format("api/{0}/GetProxyReservation/", _resource);
            _request.AddParameter("mac", mac);
            return new ApiRequest().Execute<ProxyReservationDTO>(_request);
        }

        public TftpServerDTO GetComputerTftpServers(string mac)
        {
            _request.Method = Method.GET;
            _request.Resource = string.Format("api/{0}/GetComputerTftpServers/", _resource);
            _request.AddParameter("mac", mac);
            return new ApiRequest().Execute<TftpServerDTO>(_request);
        }

        public TftpServerDTO GetAllTftpServers()
        {
            _request.Method = Method.GET;
            _request.Resource = string.Format("api/{0}/GetAllTftpServers/", _resource);
            return new ApiRequest().Execute<TftpServerDTO>(_request);
        }

        public AppleVendorDTO GetAppleVendorString(string ip)
        {
            _request.Method = Method.GET;
            _request.Resource = string.Format("api/{0}/GetAppleVendorString/", _resource);
            _request.AddParameter("ip", ip);
            var response = new ApiRequest().Execute<AppleVendorDTO>(_request);
            return response;
        }

        public bool Test()
        {
            _request.Method = Method.GET;
            _request.Resource = string.Format("api/{0}/Test/", _resource);
            var response = new ApiRequest().Execute<ApiBoolResponseDTO>(_request);
            return response != null && response.Value;
        }
    }
}
