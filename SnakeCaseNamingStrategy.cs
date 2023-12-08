using Newtonsoft.Json.Serialization;


public class SnakeCaseNamingStrategy : NamingStrategy
{
  protected override string ResolvePropertyName(string name)
  {
    return ToSnakeCase(name);
  }

  private static string ToSnakeCase(string str)
  {
    if (string.IsNullOrEmpty(str))
      return str;

    var result = char.ToLower(str[0]).ToString();

    for (int i = 1; i < str.Length; i++)
    {
      if (char.IsUpper(str[i]))
      {
        result += "_" + char.ToLower(str[i]);
      }
      else
      {
        result += str[i];
      }
    }

    return result;
  }
}
