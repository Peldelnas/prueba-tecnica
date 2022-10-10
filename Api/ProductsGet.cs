using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Threading.Tasks;

namespace Api;

public class PhrasesGet
{
    private readonly IPhraseData phraseData;

    public PhrasesGet(IPhraseData phraseData)
    {
        this.phraseData = phraseData;
    }

    [FunctionName("PhrasesGet")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "phrases")] HttpRequest req)
    {
        var phrases = await phraseData.GetPhrases();
        return new OkObjectResult(phrases);
    }
}