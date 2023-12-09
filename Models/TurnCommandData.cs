namespace nateisthename.Govee;

public class TurnCommandData(int value) : IGoveeData
{
  public int Value = value;

  public string GetTypeLabel() => "turn";
}
