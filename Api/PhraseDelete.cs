using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Api;

public class PhrasesDelete
{
    private readonly IPhraseData phraseData;

    public PhrasesDelete(IPhraseData phraseData)
    {
        this.phraseData = phraseData;
    }

    [FunctionName("PhrasesDelete")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "phrases/{phraseId:int}")] HttpRequest req,
        int phraseId,
        ILogger log)
    {
        var result = await phraseData.DeletePhrase(phraseId);

        if (result)
        {
            return new OkResult();
        }
        else
        {
            return new BadRequestResult();
        }
    }
}
