namespace nateisthename.Govee;

public class GoveeMessageWrapper<T>(T data) where T : IGoveeData
{
  public GoveeMessage<T> Msg = new GoveeMessage<T>(data);
}
