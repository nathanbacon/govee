namespace nateisthename.Govee;

public class GoveeMessageWrapper<T>(GoveeMessage<T> msg) where T : IGoveeData
{
  public GoveeMessage<T> Msg = msg;
}
