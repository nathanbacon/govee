using Microsoft.Extensions.DependencyInjection;

namespace nateisthename.Govee;

public static class GoveeServiceExtensions
{
  public static IServiceCollection UseGoveeService(this IServiceCollection services, Action<GoveeServiceOptions> configure)
  {

    services.AddHostedService<GoveeListeningService>(provider =>
    {
      var options = new GoveeServiceOptions();
      configure?.Invoke(options);

      var deviceResponseHandler = provider.GetRequiredService<IDeviceResponseHandler>();
      return new GoveeListeningService(deviceResponseHandler, options.TimeLimit);
    });

    return services;
  }
}

public class GoveeServiceOptions
{
  public TimeSpan? TimeLimit;
}
