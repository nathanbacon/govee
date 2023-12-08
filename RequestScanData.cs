namespace nateisthename.Govee;

public class RequestScanData : IGoveeData
{
  public string AccountTopic => "reserve";

  public string GetTypeLabel() => "scan";
}
