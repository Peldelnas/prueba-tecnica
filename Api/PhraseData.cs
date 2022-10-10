using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data;

namespace Api;

public interface IPhraseData
{
    Task<Phrase> AddPhrase(Phrase phrase);
    Task<bool> DeletePhrase(int id);
    Task<IEnumerable<Phrase>> GetPhrases();
    Task<Phrase> UpdatePhrase(Phrase phrase);
}

public class PhraseData : IPhraseData
{
    private readonly List<Phrase> phrases = new List<Phrase>
        {
            new Phrase
            {
                Id = 10,
                Text = "Esto es una prueba",
                Classification= "classif",
                Entities = "entities",
                Language = "Español",
                IsTranslated = true,
                KeyWords = "clave",
                Sentiment = 4

            }
        };

    private int GetRandomInt()
    {
        var random = new Random();
        return random.Next(100, 1000);
    }

    public Task<Phrase> AddPhrase(Phrase phrase)
    {
        phrase.Id = GetRandomInt();
        phrases.Add(phrase);
        return Task.FromResult(phrase);
    }

    public Task<Phrase> UpdatePhrase(Phrase phrase)
    {
        var index = phrases.FindIndex(p => p.Id == phrase.Id);
        phrases[index] = phrase;
        return Task.FromResult(phrase);
    }

    public Task<bool> DeletePhrase(int id)
    {
        var index = phrases.FindIndex(p => p.Id == id);
        phrases.RemoveAt(index);
        return Task.FromResult(true);
    }

    public Task<IEnumerable<Phrase>> GetPhrases()
    {
        return Task.FromResult(phrases.AsEnumerable());
    }
}
