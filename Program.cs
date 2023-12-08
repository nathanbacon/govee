// See https://aka.ms/new-console-template for more information
using System.Net;
using System.Net.Sockets;
using System.Text;
using nateisthename.Govee;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

Console.WriteLine("Hello, World!");

var jsonSerializerSettings = new JsonSerializerSettings
{
  ContractResolver = new DefaultContractResolver
  {
    NamingStrategy = new SnakeCaseNamingStrategy()
  },
  Formatting = Formatting.Indented
};

static async Task RepeatTaskEvery(Func<Task> action, TimeSpan interval, CancellationToken cancellationToken)
{
  while (!cancellationToken.IsCancellationRequested)
  {
    await action();

    try
    {
      await Task.Delay(interval, cancellationToken);
    }
    catch (TaskCanceledException)
    {

    }
  }
}

List<IPAddress> iPAddresses = new();

int onState = 1;

async Task ToggleOn(CancellationToken cancellationToken)
{
  int sendPort = 4003;
  using var udpClient = new UdpClient();
  onState = (onState + 1) % 2;
  var turnCommand = new TurnCommandData(onState);
  var goveeMessage = new GoveeMessage<TurnCommandData>(turnCommand);
  var messageWrapper = new GoveeMessageWrapper<TurnCommandData>(goveeMessage);
  var message = JsonConvert.SerializeObject(messageWrapper, jsonSerializerSettings);
  Console.WriteLine(message);
  var messageBytes = Encoding.UTF8.GetBytes(message);

  foreach (IPAddress iPAddress in iPAddresses)
  {
    Console.WriteLine("Sending message to: " + iPAddress.ToString());
    await udpClient.SendAsync(messageBytes, new IPEndPoint(iPAddress, sendPort), cancellationToken);
  }
}

var udpListener = Task.Run(async () =>
{
  int listenPort = 4002;
  using var udpClient = new UdpClient(listenPort);

  while (true)
  {
    Console.WriteLine("Listening for message");
    try
    {
      IPEndPoint clientEndpoint = new IPEndPoint(IPAddress.Any, 0);
      var receivedResults = await udpClient.ReceiveAsync();
      byte[] bytes = receivedResults.Buffer;
      string message = Encoding.UTF8.GetString(bytes);
      var responseScanRoot = JsonConvert.DeserializeObject<GoveeMessageWrapper<ResponseScanData>>(message, jsonSerializerSettings);
      if (responseScanRoot == null) continue;

      iPAddresses.Add(IPAddress.Parse(responseScanRoot.Msg.Data.Ip));

      Console.WriteLine(message);
    }
    catch (Exception ex)
    {
      Console.WriteLine(ex.Message);
    }
  }
});

var multicastListener = Task.Run(async () =>
{
  string multicastAddress = "239.255.255.250";
  var ipEndpoint = IPAddress.Parse(multicastAddress);
  int multicastPort = 4001;
  using var udpClient = new UdpClient();

  udpClient.JoinMulticastGroup(ipEndpoint, 25);

  RequestScanData requestScan = new();
  var goveeMessage = new GoveeMessage<RequestScanData>(requestScan);
  var requestScanWrapper = new GoveeMessageWrapper<RequestScanData>(goveeMessage);
  string message = JsonConvert.SerializeObject(requestScanWrapper, jsonSerializerSettings);
  var messageBytes = Encoding.UTF8.GetBytes(message);

  await udpClient.SendAsync(messageBytes, messageBytes.Length, new IPEndPoint(ipEndpoint, multicastPort));
  Console.WriteLine(message);

  Console.WriteLine($"Message sent to {multicastAddress}");
});

var onOffTask = Task.Run(async () =>
{
  var cancellationTokenSource = new CancellationTokenSource();
  var repeatingTask = RepeatTaskEvery(() => ToggleOn(cancellationTokenSource.Token), TimeSpan.FromSeconds(5), cancellationTokenSource.Token);
});

await Task.WhenAll([multicastListener, udpListener, onOffTask]);
