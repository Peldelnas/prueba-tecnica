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
using System.Linq;

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
            Console.WriteLine("try start");
            Azure.Response<DetectedLanguage> response = client.DetectLanguage(phrase.Text);
            DetectedLanguage language = response.Value;
            phrase.Language = language.Name;
            // habría que checkear een caso de errores
            if(language.Name != "Spanish") {
                lan = await TranslatorHandler.Detect(phrase.Text);
                string texttt = await TranslatorHandler.Translate(phrase.Text, lan);               
                var details = JArray.Parse(texttt);
                phrase.TextTranslated = details[0]["translations"][0]["text"].ToString();
                Console.WriteLine("different to spanish done");
            }
            var responseEntities = client.RecognizeEntities(phrase.Text, lan);
            foreach(var entity in responseEntities.Value)
            {
                phrase.Entities += $"Palabra: {entity.Text},\r\nCategoría: {entity.Category},\r\nSub-Categoría: {entity.SubCategory}\r\n";
            }
            Console.WriteLine("entities done");
            var responseKey = client.ExtractKeyPhrases(phrase.Text,lan);
            phrase.KeyWords = "";
            foreach(var key in responseKey.Value)
            {
                phrase.KeyWords += key+ ", ";
            }
            if(phrase.KeyWords != "") 
            {
                phrase.KeyWords = phrase.KeyWords.Trim().Substring(0,phrase.KeyWords.Length - 2);
            }
            
            Console.WriteLine("keywords done");

            var reviewsResponse = client.AnalyzeSentiment(phrase.Text, lan, options: new AnalyzeSentimentOptions(){
                IncludeOpinionMining = true
            });
            Azure.AI.TextAnalytics.DocumentSentiment review = reviewsResponse.Value;
                phrase.Sentiment = "";

                phrase.Sentiment += $"Document sentiment: "+ review.Sentiment.ToString() +"\n";
                phrase.Sentiment += $"\tPositive score: {review.ConfidenceScores.Positive.ToString()}";
                phrase.Sentiment += $"\tNegative score: {review.ConfidenceScores.Negative.ToString()}";
                phrase.Sentiment += $"\tNeutral score: {review.ConfidenceScores.Neutral.ToString()}\n";
                foreach (SentenceSentiment sentence in review.Sentences)
                {
                    phrase.Sentiment += $"\tText: \"{sentence.Text}\"";
                    phrase.Sentiment += $"\tSentence sentiment: {sentence.Sentiment}";
                    phrase.Sentiment += $"\tSentence positive score: {sentence.ConfidenceScores.Positive:0.00}";
                    phrase.Sentiment += $"\tSentence negative score: {sentence.ConfidenceScores.Negative:0.00}";
                    phrase.Sentiment += $"\tSentence neutral score: {sentence.ConfidenceScores.Neutral:0.00}\n";
                    foreach (SentenceOpinion sentenceOpinion in sentence.Opinions)
                    {
                        phrase.Sentiment += $"\tTarget: {sentenceOpinion.Target.Text}, Value: {sentenceOpinion.Target.Sentiment}";
                        phrase.Sentiment += $"\tTarget positive score: {sentenceOpinion.Target.ConfidenceScores.Positive:0.00}";
                        phrase.Sentiment += $"\tTarget negative score: {sentenceOpinion.Target.ConfidenceScores.Negative:0.00}";
                        foreach (AssessmentSentiment assessment in sentenceOpinion.Assessments)
                        {
                            phrase.Sentiment += $"\t\tRelated Assessment: {assessment.Text}, Value: {assessment.Sentiment}";
                            phrase.Sentiment += $"\t\tRelated Assessment positive score: {assessment.ConfidenceScores.Positive:0.00}";
                            phrase.Sentiment += $"\t\tRelated Assessment negative score: {assessment.ConfidenceScores.Negative:0.00}";
                        }
                    }
                }                
                phrase.Sentiment += $"\n";
                Console.WriteLine("sentiment done");



            System.Threading.Thread.Sleep(1000);
            
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
