using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordEmbeddingDistance
{
    private WordEmbedding wordEmbed;
    private double distance;

    // Constructor
    public WordEmbeddingDistance(WordEmbedding w, double d)
    {
        wordEmbed = w;
        distance = d;
    }

    public string GetWord()
    {
        return wordEmbed.GetWord();
    }

    public WordEmbedding getWordEmbedding()
    {
        return wordEmbed;
    }

    public double GetDistance()
    {
        return distance;
    }

    // Instance Method
    public override string ToString()
    {
        return "(" + wordEmbed.GetWord() + ", " + distance + ")";
    }

    // Destructor
    ~WordEmbeddingDistance()
    {
        // Some resource cleanup routines
    }
}