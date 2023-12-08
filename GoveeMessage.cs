namespace nateisthename.Govee;

public class GoveeMessage<T>(T data) where T : IGoveeData
{
  public string Cmd = data.GetTypeLabel();
  public T Data { get; } = data;
}
