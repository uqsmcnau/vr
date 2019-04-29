using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// helper class
class WordEmbeddingDistance
{
    string word;
    double distance;

    // Constructor
    public WordEmbeddingDistance(string w, double d)
    {
        word = w;
        distance = d;
    }

    public string GetWord()
    {
        return word;
    }

    public double GetDistance()
    {
        return distance;
    }

    // Instance Method
    public override string ToString()
    {
        return "(" + word + ", " + distance + ")";
    }

    // Destructor
    ~WordEmbeddingDistance()
    {
        // Some resource cleanup routines
    }
}

// helper class
class WordEmbedding
{
    string word;
    double[] vectors;
    WordEmbeddingDistance[] nearestNeighbours;

    // Constructor
    public WordEmbedding(string w, double[] v)
    {
        word = w;
        vectors = v;
    }

    // Instance Method
    public override string ToString()
    {
        string output = word;
        if (nearestNeighbours != null)
        {
            for (int i = 0; i < nearestNeighbours.Length; i++)
            {
                if (nearestNeighbours[i] != null)
                {
                    output += "\n";
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

    public string GetWord()
    {
        return word;
    }

    public double[] GetVectors()
    {
        return vectors;
    }

    public void FindNN(WordEmbedding[] neighbours, int k)
    {
        nearestNeighbours = new WordEmbeddingDistance[k];
        for (int i = 0; i < neighbours.Length; i++)
        {
            double distance = Distance(neighbours[i]);
            WordEmbeddingDistance wed = new WordEmbeddingDistance(neighbours[i].GetWord(), distance);

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

public class Scoreboard : MonoBehaviour
{
    public TextMeshPro scoreboard;
    public Camera cam;

    private WordEmbedding[] embeddings;
    private int counter;

    private int count;
    private int dimensionality;

    private WordEmbedding Target;

    // Start is called before the first frame update
    void Start()
    {
        counter = 0;
        string line;

        // Read the file and display it line by line.  
        System.IO.StreamReader file = new System.IO.StreamReader(@"C:\UNI\skipgram.txt");
        line = file.ReadLine();

        string[] subStrings = line.Split(' ');
        count = System.Convert.ToInt32(subStrings[0]);
        dimensionality = System.Convert.ToInt32(subStrings[1]);

        count = 1000;

        embeddings = new WordEmbedding[count];

        for (int j = 0; j < count; j++)
        //while ((line = file.ReadLine()) != null)
        {
            line = file.ReadLine();
            subStrings = line.Split(' ');
            string currentword = subStrings[0];

            double[] currentVectors = new double[dimensionality];
            for (int i = 0; i < dimensionality; i++)
            {
                currentVectors[i] = System.Convert.ToDouble(subStrings[(i + 1)]);
            }

            embeddings[counter] = new WordEmbedding(currentword, currentVectors);
            //embeddings[counter].PrintString();
            counter++;
        }
        file.Close();

        counter = 0;
    }

    // Update is called once per frame
    void Update()
    {
        scoreboard.transform.rotation = Quaternion.LookRotation(scoreboard.transform.position - cam.transform.position);
        if (counter%100 == 0)
        {
            Target = embeddings[(counter / 100) % count];
            Target.FindNN(embeddings, 10);
            scoreboard.text = Target.ToString();
        }
        counter++;
    }
}
