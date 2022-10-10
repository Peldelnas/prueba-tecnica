using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Data;

using Azure.AI.TextAnalytics;
namespace Api;

public class PhrasesPut
{
    private readonly IPhraseData phraseData;

    public PhrasesPut(IPhraseData phraseData)
    {
        this.phraseData = phraseData;
    }

    [FunctionName("PhrasesPut")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "phrases")] HttpRequest req,
        ILogger log)
    {
        var body = await new StreamReader(req.Body).ReadToEndAsync();
        var phrase = JsonSerializer.Deserialize<Phrase>(body, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        var updatedPhrase = await phraseData.UpdatePhrase(phrase);
        return new OkObjectResult(updatedPhrase);
        

    }
}
