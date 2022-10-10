using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Data;

namespace Api;

public class PhrasesPost
{
    private readonly IPhraseData phraseData;

    public PhrasesPost(IPhraseData phraseData)
    {
        this.phraseData = phraseData;
    }

    [FunctionName("PhrasesPost")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "phrases")] HttpRequest req,
        ILogger log)
    {
        var body = await new StreamReader(req.Body).ReadToEndAsync();
        var phrase = JsonSerializer.Deserialize<Phrase>(body, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        var newPhrase = await phraseData.AddPhrase(phrase);
        return new OkObjectResult(newPhrase);
    }
}
