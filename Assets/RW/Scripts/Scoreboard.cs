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
    private WordEmbedding OldTarget;

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
        OldTarget = Target;
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
            // Setup Initial state based on sample word
            preview = false;

            // Create game options
            options = new GameObject[k + 1];

            //No longer display initial option
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.enabled = false;
            meshRenderer = text.GetComponent<MeshRenderer>();
            meshRenderer.enabled = false;
            BoxCollider boxCollider = GetComponent<BoxCollider>();
            boxCollider.enabled = false;

            // Create target option
            GameObject targetOption = Instantiate(optionPrefab);
            Option to = targetOption.GetComponent<Option>();
            to.cam = cam;
            to.SetParent(this);
            to.SetWordEmbedding(Target);
            targetOption.transform.position = transform.position;
            options[0] = targetOption;

            // Create k Nearest Neighbour Objects
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

                options[i + 1] = option;
            }
        } else
        {
            // New word selected so update the model
            WordEmbeddingDistance[] newWED = new WordEmbeddingDistance[k + 1];
            WordEmbeddingDistance[] exisitingWED = new WordEmbeddingDistance[k + 1];
            int countOfNewWords = 0;

            // First sort the kNN into new words and words already existing in the previous model
            for (int i = 0; i < k; i++)
            {
                WordEmbeddingDistance neighbour = NN[i];
                Boolean flag = false;
                for (int j = 0; j < k; j++)
                {
                    // Word already in model
                    if (options[j].GetComponent<Option>().getWord() == NN[i].getWordEmbedding().GetWord())
                    {
                        flag = true;
                        exisitingWED[i - countOfNewWords] = neighbour;
                    }
                }
                if (!flag)
                {
                    // Word is not in model
                    newWED[countOfNewWords] = neighbour;
                    countOfNewWords += 1;
                }
            }

            Vector3 targetCurrentPosition = transform.position;

            // For each option update and move or recreate in model
            for (int i = 0; i < k + 1; i++)
            {
                GameObject option = options[i];
                Option o = option.GetComponent<Option>();

                // If the word is the target, move towards the centre
                if (o.getWord() == Target.GetWord())
                {
                    o.SetWordEmbedding(Target);
                    o.SetTargetPosition(transform.position);
                    targetCurrentPosition = o.transform.position;
                }
            }



            // For each option update and move or recreate in model
            for (int i = 0; i < k + 1; i++)
            {
                GameObject option = options[i];
                Option o = option.GetComponent<Option>();

                if (o.getWord() != Target.GetWord()) {
                    // Check if the option represents one of the words to be retained
                    Boolean flag = false;
                    for (int j = 0; j < k + 1; j++)
                    {
                        // If so move it towards its new location relative to the target word
                        if (exisitingWED[j] != null && exisitingWED[j].getWordEmbedding().GetWord() == o.getWord())
                        {
                            flag = true;
                            o.SetWordEmbedding(exisitingWED[j].getWordEmbedding());
                            o.SetTargetPosition(transform.position + new Vector3(
                                (float)(5 * exisitingWED[j].getPCADistance()[0]),
                                (float)(5 * exisitingWED[j].getPCADistance()[1]),
                                (float)(5 * exisitingWED[j].getPCADistance()[2])));
                            o.SetVisible();
                        }
                    }
                    if (!flag)
                    {
                        // If not, replace it with one of the new words to be modelled
                        countOfNewWords -= 1;

                        GameObject tempGameObject = Instantiate(optionPrefab);
                        Option tempOption = tempGameObject.GetComponent<Option>();
                        tempOption.cam = cam;
                        tempOption.SetWordEmbedding(o.GetWordEmbedding());
                        tempOption.SetPosition(o.transform.position);
                        tempOption.SetTargetPosition(tempOption.transform.position + transform.position - targetCurrentPosition);
                        tempOption.StartFadeOut();
                        
                        o.SetWordEmbedding(newWED[countOfNewWords].getWordEmbedding());
                        o.SetPosition(targetCurrentPosition + new Vector3(
                                (float)(5 * newWED[countOfNewWords].getPCADistance()[0]),
                                (float)(5 * newWED[countOfNewWords].getPCADistance()[1]),
                                (float)(5 * newWED[countOfNewWords].getPCADistance()[2])));
                        o.SetTargetPosition(transform.position + new Vector3(
                                (float)(5 * newWED[countOfNewWords].getPCADistance()[0]),
                                (float)(5 * newWED[countOfNewWords].getPCADistance()[1]),
                                (float)(5 * newWED[countOfNewWords].getPCADistance()[2])));
                        o.StartFadeIn();
                    }
                }
            }
        }
    }
}
