using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using Accord.Statistics.Analysis;

public class Scoreboard : Selectable
{
    public GameObject scoreboard;
    public TextMeshPro text;
    public Camera cam;

    private WordEmbedding[] embeddings;
    private int counter;

    private int count;
    private int dimensionality;

    private bool preview;

    private WordEmbedding Target;

    public GameObject optionPrefab;
    private GameObject[] options;

    private readonly int k = 10;


    // Start is called before the first frame update
    void Start()
    {
        counter = 0;
        string line;
        preview = true;

            
        // Read the file and display it line by line.  
        System.IO.StreamReader file = new System.IO.StreamReader(@"C:\UNI\skipgram.txt");
        line = file.ReadLine();

        string[] subStrings = line.Split(' ');
        count = System.Convert.ToInt32(subStrings[0]);
        dimensionality = System.Convert.ToInt32(subStrings[1]);

        count = 1000;

        embeddings = new WordEmbedding[count];
        double[][] data = new double[count][];

        for (int j = 0; j < count; j++)
        {
            line = file.ReadLine();
            subStrings = line.Split(' ');
            string currentword = subStrings[0];

            double[] currentVectors = new double[dimensionality];
            for (int i = 0; i < dimensionality; i++)
            {
                currentVectors[i] = System.Convert.ToDouble(subStrings[(i + 1)]);
            }
            data[counter] = currentVectors;

            embeddings[counter] = new WordEmbedding(currentword, currentVectors);

            counter++;
        }
        file.Close();

        var pca = new PrincipalComponentAnalysis();
        pca.Learn(data);
        double[][] finalData = pca.Transform(data);

        counter = 0;

        for (int j = 0; j < count; j++)
        {
            WordEmbedding Target = embeddings[j];

            double[] pcavectors = new double[3];
            for (int k = 0; k < 3; k++)
            {
                pcavectors[k] = finalData[j][k];
            }
            Target.SetPCAVectors(pcavectors);
            embeddings[j] = Target;
        }


        UpdateTarget();
    }

    // Update is called once per frame
    void Update()
    {
        scoreboard.transform.rotation = Quaternion.LookRotation(scoreboard.GetComponent<Renderer>().bounds.center - cam.transform.position);
        if (preview && (counter%100 == 0))
        {
            UpdateTarget();
        }
        counter++;
    }

    public void SetTarget(WordEmbedding we)
    {
        Target = we;
        Target.FindNN(embeddings, k);
        text.text = Target.GetWord();
    }
    
    private void UpdateTarget()
    {
        Target = embeddings[(counter / 100) % count];
        Target.FindNN(embeddings, k);
        text.text = Target.GetWord();
    }

    public override void Select()
    {
        WordEmbeddingDistance[] NN = Target.GetNN();

        if (preview)
        {
            preview = false;
            //scoreboard.SetActive(false);

            options = new GameObject[k];

            for (int i = 0; i < k; i++)
            {
                WordEmbeddingDistance neighbour = NN[i];

                GameObject option = Instantiate(optionPrefab);
                Option o = option.GetComponent<Option>();
                o.cam = cam;
                o.SetParent(this);
                o.SetWordEmbedding(neighbour.getWordEmbedding());

                option.transform.position = transform.position + new Vector3(
                        (float)(5 * neighbour.getPCADistance()[0]),
                        (float)(5 * neighbour.getPCADistance()[1]),
                        (float)(5 * neighbour.getPCADistance()[2]));

                options[i] = option;
            }
        } else
        {
            for (int i = 0; i < k; i++)
            {
                WordEmbeddingDistance neighbour = NN[i];
                GameObject option = options[i];
                Option o = option.GetComponent<Option>();
                o.SetWordEmbedding(NN[i].getWordEmbedding());


                o.setTargetPosition(transform.position + new Vector3(
                        (float)(5 * neighbour.getPCADistance()[0]),
                        (float)(5 * neighbour.getPCADistance()[1]),
                        (float)(5 * neighbour.getPCADistance()[2])));
            }
        }
    }
}
