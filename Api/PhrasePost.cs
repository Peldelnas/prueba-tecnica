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

        string document = @"Este documento está escrito en un idioma diferente al Inglés. Tiene como objetivo demostrar
                    cómo invocar el método de Detección de idioma del servicio de Text Analytics en Microsoft Azure.
                    También muestra cómo acceder a la información retornada por el servicio. Esta capacidad es útil
                    para los sistemas de contenido que recopilan texto arbitrario, donde el idioma es desconocido.
                    La característica Detección de idioma puede detectar una amplia gama de idiomas, variantes,
                    dialectos y algunos idiomas regionales o culturales.";

        try
        {
            Azure.Response<DetectedLanguage> response = client.DetectLanguage(document);

            DetectedLanguage language = response.Value;
            Console.WriteLine($"Detected language {language.Name} with confidence score {language.ConfidenceScore}.");
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
