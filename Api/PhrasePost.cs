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
using Azure.Core;
using System;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Api;

public class PhrasesPost
{
    private readonly IPhraseData phraseData;

    // habría que checkearlo por temas de seguridad
    private string endpoint = "https://retoaideveloperlanguageresource.cognitiveservices.azure.com/";
    private string apiKey = "c0b9af3ec25147cd86fff1032d14d755";
    private TextAnalyticsClient client;


    public PhrasesPost(IPhraseData phraseData)
    {
        this.phraseData = phraseData;
        client = new TextAnalyticsClient(new Uri(endpoint), new Azure.AzureKeyCredential(apiKey));
    }

    [FunctionName("PhrasesPost")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "phrases")] HttpRequest req,
        ILogger log)
    {
        var body = await new StreamReader(req.Body).ReadToEndAsync();
        var phrase = JsonSerializer.Deserialize<Phrase>(body, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }); 
        var lan = "es";       

        try
        {
            Azure.Response<DetectedLanguage> response = client.DetectLanguage(phrase.Text);
            DetectedLanguage language = response.Value;
            phrase.Language = language.Name;
            // habría que checkear een caso de errores
            if(language.Name != "Spanish") {
                lan = await TranslatorHandler.Detect(phrase.Text);
                string texttt = await TranslatorHandler.Translate(phrase.Text, lan);               
                var details = JArray.Parse(texttt);
                phrase.TextTranslated = details[0]["translations"][0]["text"].ToString();
            }
            Azure.Response<CategorizedEntityCollection> entities = client.RecognizeEntities(phrase.Text, lan);

            Console.WriteLine("lenguage: " + lan);
            Console.WriteLine(entities.GetRawResponse());

            
        }
        catch (Azure.RequestFailedException exception)
        {
            Console.WriteLine($"Error Code: {exception.ErrorCode}");
            Console.WriteLine($"Message: {exception.Message}");
        }

        var newPhrase = await phraseData.AddPhrase(phrase);



        return new OkObjectResult(newPhrase);


    }
}
