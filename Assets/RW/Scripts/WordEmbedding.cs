using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// helper class
public class WordEmbedding
{
    string word;
    double[] vectors;
    WordEmbeddingDistance[] nearestNeighbours;

    double[] pca;

    // Constructor
    public WordEmbedding(string w, double[] v)
    {
        word = w;
        vectors = v;
    }
    
    // Instance Method
    public override String ToString()
    {
        string output = word;
        if (nearestNeighbours != null)
        {
            for (int i = 0; i < nearestNeighbours.Length; i++)
            {
                if (nearestNeighbours[i] != null)
                {
                    output += " ";
                    output += nearestNeighbours[i].ToString();
                }
            }
        }
        else
        {
            for (int i = 0; i < vectors.Length; i++)
            {
                output += " ";
                output += vectors[i];
            }
        }
        return output;
    }

    public String GetWord()
    {
        return word;
    }

    public double[] GetVectors()
    {
        return vectors;
    }

    public void SetPCAVectors(double[] pca)
    {
        this.pca = pca;
    }

    public double[] GetPCAVectors()
    {
        return pca;
    }

    public void FindNN(WordEmbedding[] neighbours, int k)
    {
        nearestNeighbours = new WordEmbeddingDistance[k];
        for (int i = 0; i < neighbours.Length; i++)
        {
            if (word != neighbours[i].GetWord())
            {
                double distance = Distance(neighbours[i]);
                double[] pcadistance = new double[3];

                for (int j = 0; j < 3; j++)
                {
                    pcadistance[j] = pca[j] - neighbours[i].GetPCAVectors()[j];
                }

                WordEmbeddingDistance wed = new WordEmbeddingDistance(neighbours[i], distance, pcadistance);

                for (int j = 0; j < k; j++)
                {
                    if (nearestNeighbours[j] == null)
                    {
                        nearestNeighbours[j] = wed;
                        j = k;
                    }
                    else if (nearestNeighbours[j].GetDistance() > wed.GetDistance())
                    {
                        WordEmbeddingDistance holder = nearestNeighbours[j];
                        nearestNeighbours[j] = wed;
                        wed = holder;
                    }
                }
            }
        }
    }

    public WordEmbeddingDistance[] GetNN()
    {
        return nearestNeighbours;
    }

    public double Distance(WordEmbedding target)
    {
        if (target.vectors.Length != vectors.Length)
        {
            throw new ArgumentOutOfRangeException("Both WordEmbeddings must be of same dimensionality.");
        }
        double sum = 0;
        for (int i = 0; i < vectors.Length; i++)
        {
            sum += ((vectors[i] - target.vectors[i]) * (vectors[i] - target.vectors[i]));
        }
        return Math.Sqrt(sum);
    }

    // Destructor
    ~WordEmbedding()
    {
        // Some resource cleanup routines
    }
}
