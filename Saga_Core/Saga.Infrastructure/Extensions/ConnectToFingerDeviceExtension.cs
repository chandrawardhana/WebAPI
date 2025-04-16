using Saga.Domain.Enums;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Text;
using zkemkeeper;

namespace Saga.Infrastructure.Extensions;

public static class ConnectToFingerDeviceExtension
{
    public static bool ConnectToDevice(CZKEMClass zkemKeeper, ConnectionMethod? Method = null, string? IPAddress = null, int? Port = null, string? Comm = null, int? Baudrate = null, HttpClient httpClient = null, string? serialNumber = null)
    {
        const int machineNumber = 1;
        bool connected = false;

        switch (Method)
        {
            case ConnectionMethod.Web:

                if (string.IsNullOrEmpty(IPAddress) || string.IsNullOrEmpty(serialNumber))
                    return false;

                try
                {
                    // First try to connect with sdk
                    connected = zkemKeeper.Connect_Net(IPAddress, Port ?? 4370);

                    // Set Security Protocol
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

                    // Configure HttpClientHandler with specific SSL/TLS settings
                    var handler = new HttpClientHandler
                    {
                        SslProtocols = SslProtocols.Tls12,
                        // Configure proxy settings for PPTP
                        UseProxy = true,
                        UseDefaultCredentials = true,

                        // Allow automatic redirection
                        AllowAutoRedirect = true,

                        // Configure authentication
                        PreAuthenticate = true,

                        // Configure client certificate handling
                        ClientCertificateOptions = ClientCertificateOption.Manual,
                        ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true,

                        // Maximum redirects
                        MaxAutomaticRedirections = 10,

                        // Enable automatic decompression
                        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                    };

                    // Use the configured handler for the request
                    using (var secureHttpClient = new HttpClient(handler))
                    {
                        secureHttpClient.Timeout = TimeSpan.FromMinutes(2);

                        var soapRequest = $@"<GetAttLog>
                            <ArgComKey xsi:type=""xsd:integer"">{serialNumber}</ArgComKey>
                            <Arg>
                                <PIN xsi:type=""xsd:integer"">All</PIN>
                            </Arg>
                        </GetAttLog>";

                        var endpoint = $"http://{IPAddress}:{Port ?? 4370}/iWsService";

                        var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
                        request.Content = new StringContent(soapRequest, Encoding.UTF8, "text/xml");
                        request.Headers.Add("SOAPAction", "GetAttLog");

                        // Add additional headers that might help with connection stability
                        request.Headers.Add("Connection", "Keep-Alive");
                        request.Headers.Add("Keep-Alive", "timeout=600");

                        var response = secureHttpClient.SendAsync(request).GetAwaiter().GetResult();
                        response.EnsureSuccessStatusCode();

                        connected = true;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                    connected = false;
                }
                break;
            case ConnectionMethod.Serial:
                if (int.TryParse(Comm?.Replace("COM", ""), out int portNumber))
                {
                    connected = zkemKeeper.Connect_Com(portNumber, machineNumber, Baudrate ?? 115200);
                }
                break;
            case ConnectionMethod.Port:
                var usbComSearch = new SearchforUSBCom();
                var comm = Comm ?? "";
                if (usbComSearch.SearchforCom(ref comm))
                {
                    if (int.TryParse(Comm?.Replace("COM", ""), out int usbPort))
                    {
                        connected = zkemKeeper.Connect_Com(usbPort, machineNumber, Baudrate ?? 115200);
                    }
                }
                break;
        }

        if (connected)
        {
            zkemKeeper.RegEvent(machineNumber, 65535);
        }

        return connected;
    }
}
