using System.Net;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newsletter.CompositionRoot;

namespace Newsletter.Functions;

public class SampleHttpFunction
{
    private readonly ILogger<SampleHttpFunction> _logger;
    private readonly NewsletterOptions _options;

    public SampleHttpFunction(ILogger<SampleHttpFunction> logger, IOptions<NewsletterOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    [FunctionName("SampleHttpFunction")]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "SampleHttpFunction")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        var result = new
        {
            DatabaseConnectionString = _options.Database.ConnectionString,
            EmailFrom = _options.Notifications.Email.FromAddress,
            SmsProvider = _options.Notifications.Sms.Provider
        };

        return new OkObjectResult(result);
    }
} 