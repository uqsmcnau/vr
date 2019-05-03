using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordEmbeddingDistance
{
    WordEmbedding word;
    double distance;
    double[] pcadistance;

    // Constructor
    public WordEmbeddingDistance(WordEmbedding w, double d, double[] p)
    {
        word = w;
        distance = d;
        pcadistance = p;
    }

    public WordEmbedding getWordEmbedding()
    {
        return word;
    }

    public double GetDistance()
    {
        return distance;
    }

    public double[] getPCADistance()
    {
        return pcadistance;
    }

    // Instance Method
    public override string ToString()
    {
        string outputString = "(" + word + ", " + distance + " [";
        for (int i = 0; i < pcadistance.Length; i++)
        {
            outputString += pcadistance[i];
            if ((i + 1) < pcadistance.Length)
            {
                outputString += ", ";
            }
            else
            {
                outputString += "]";
            }
        }
        return outputString;
    }

    // Destructor
    ~WordEmbeddingDistance()
    {
        // Some resource cleanup routines
    }
}