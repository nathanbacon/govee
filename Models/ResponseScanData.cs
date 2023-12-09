namespace nateisthename.Govee;

public class ResponseScanData : IGoveeData
{
  public required string Ip { get; set; }
  public required string Device { get; set; }
  public required string Sku { get; set; }
  public required string BleVersionHard { get; set; }
  public required string BleVersionSoft { get; set; }
  public required string WifiVersionHard { get; set; }
  public required string WifiVersionSoft { get; set; }

  public string GetTypeLabel() => "scan";
}
