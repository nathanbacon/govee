using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace nateisthename.Govee;

public class GoveeClient(IDeviceResponseHandler? deviceResponseHandler = null)
{
  private readonly string _MulticastAddress = "239.255.255.250";
  private readonly int _MulticastPort = 4001;
  private readonly int _ResponseListenPort = 4002;
  private readonly IDeviceResponseHandler? _DeviceResponseHandler = deviceResponseHandler;

  private static JsonSerializerSettings MyJsonSerializerSettings =>
    new()
    {
      ContractResolver = new DefaultContractResolver
      {
        NamingStrategy = new SnakeCaseNamingStrategy()
      },
      Formatting = Formatting.Indented
    };

  public async Task ListenForRequestScanResponses(CancellationToken cancellationToken)
  {
    using var udpClient = new UdpClient(_ResponseListenPort);

    while (!cancellationToken.IsCancellationRequested)
    {
      IPEndPoint clientEndpoint = new(IPAddress.Any, 0);
      var receivedResults = await udpClient.ReceiveAsync(cancellationToken);
      byte[] bytes = receivedResults.Buffer;
      string message = Encoding.UTF8.GetString(bytes);
      var goveeMessageWrapper = JsonConvert.DeserializeObject<GoveeMessageWrapper<ResponseScanData>>(message, MyJsonSerializerSettings);

      if (goveeMessageWrapper == null) continue;

      var responseScanData = goveeMessageWrapper.Msg.Data;

      _DeviceResponseHandler?.HandleDeviceResponse(responseScanData);
    }
  }

  public async Task BroadcastIntroduction(CancellationToken cancellationToken)
  {
    var ipEndpoint = IPAddress.Parse(_MulticastAddress);
    using var udpClient = new UdpClient();

    udpClient.JoinMulticastGroup(ipEndpoint, 25);

    RequestScanData requestScanData = new();
    var goveeMessage = new GoveeMessageWrapper<RequestScanData>(requestScanData);
    string message = JsonConvert.SerializeObject(goveeMessage, MyJsonSerializerSettings);
    var messageBytes = Encoding.UTF8.GetBytes(message);

    await udpClient.SendAsync(messageBytes, new IPEndPoint(ipEndpoint, _MulticastPort), cancellationToken: cancellationToken);
  }
}
