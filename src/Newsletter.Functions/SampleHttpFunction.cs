using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newsletter.CompositionRoot;

namespace Newsletter.Functions;

public class SampleHttpFunction
{
    private readonly ILogger _logger;
    private readonly NewsletterOptions _options;

    public SampleHttpFunction(ILoggerFactory loggerFactory, IOptions<NewsletterOptions> options)
    {
        _logger = loggerFactory.CreateLogger<SampleHttpFunction>();
        _options = options.Value;
    }

    [Function("SampleHttpFunction")]
    public HttpResponseData Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json");
        response.WriteString(System.Text.Json.JsonSerializer.Serialize(new
        {
            DatabaseConnectionString = _options.Database.ConnectionString,
            EmailFrom = _options.Notifications.Email.FromAddress,
            SmsProvider = _options.Notifications.Sms.Provider
        }));
        return response;
    }
} 