using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Newsletter.CompositionRoot;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration((context, config) =>
    {
        config.SetBasePath(context.HostingEnvironment.ContentRootPath)
              .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
              .AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        services.AddNewsletterServices(context.Configuration);
    })
    .Build();

host.Run(); 