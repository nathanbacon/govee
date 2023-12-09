using Microsoft.Extensions.Hosting;

namespace nateisthename.Govee;

public class GoveeListeningService(IDeviceResponseHandler deviceResponseHandler, TimeSpan? timeLimit = null) : BackgroundService
{
  private GoveeClient? _GoveeClient;
  public readonly TimeSpan? _TimeLimit = timeLimit;
  private IDeviceResponseHandler _DeviceResponseHandler = deviceResponseHandler;

  protected override async Task ExecuteAsync(CancellationToken cancellationToken)
  {
    _GoveeClient = new GoveeClient(_DeviceResponseHandler);
    var listenTaskForResponsesTask = _GoveeClient.ListenForRequestScanResponses(cancellationToken);
    var broadcastIntroTask = _GoveeClient.BroadcastIntroduction(cancellationToken);
    List<Task> myTasks = [Task.WhenAll(listenTaskForResponsesTask, broadcastIntroTask)];
    if (_TimeLimit != null)
    {
      myTasks.Add(Task.Delay(_TimeLimit.Value.Milliseconds, cancellationToken));
    }

    await Task.WhenAny(myTasks);
  }

  public override async Task StopAsync(CancellationToken cancellationToken)
  {
    await base.StopAsync(cancellationToken);
  }
}
