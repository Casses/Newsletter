using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newsletter.CompositionRoot;
using System.Text.Json;

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

    [Function("SampleHttpFunction")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        var result = new
        {
            DatabaseConnectionString = _options.Database.ConnectionString,
            EmailFrom = _options.Notifications.Email.FromAddress,
            SmsProvider = _options.Notifications.Sms.Provider
        };

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        
        var jsonResponse = JsonSerializer.Serialize(result);
        await response.WriteStringAsync(jsonResponse);
        
        return response;
    }
} 