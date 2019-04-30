using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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
            scoreboard.SetActive(false);

            options = new GameObject[k];

            for (int i = 0; i < k; i++)
            {
                GameObject option = Instantiate(optionPrefab);
                Option o = option.GetComponent<Option>();
                o.cam = cam;
                o.SetParent(this);
                o.SetWordEmbedding(NN[i].getWordEmbedding());

                option.transform.position = transform.position + new Vector3(
                        (float)(5 * Math.Sin(((float)i) / ((float)k) * ((float)2 * Math.PI))),
                        0.0f,
                        (float)(5 * Math.Cos(((float)i) / ((float)k) * ((float)2 * Math.PI))));

                options[i] = option;
            }
        } else
        {
            for (int i = 0; i < k; i++)
            {
                GameObject option = options[i];
                Option o = option.GetComponent<Option>();
                o.SetWordEmbedding(NN[i].getWordEmbedding());
            }
        }
    }
}
