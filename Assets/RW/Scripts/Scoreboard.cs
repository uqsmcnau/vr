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

            options = new GameObject[k + 1];

            //No longer display initial option
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.enabled = false;
            meshRenderer = text.GetComponent<MeshRenderer>();
            meshRenderer.enabled = false;
            BoxCollider boxCollider = GetComponent<BoxCollider>();
            boxCollider.enabled = false;

            GameObject targetOption = Instantiate(optionPrefab);
            Option to = targetOption.GetComponent<Option>();
            to.cam = cam;
            to.SetParent(this);
            to.SetWordEmbedding(Target);
            targetOption.transform.position = transform.position;
            options[0] = targetOption;


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
            WordEmbeddingDistance[] newWED = new WordEmbeddingDistance[k + 1];
            WordEmbeddingDistance[] exisitingWED = new WordEmbeddingDistance[k + 1];
            int countOfNewWords = 0;

            for (int i = 0; i < k; i++)
            {
                WordEmbeddingDistance neighbour = NN[i];
                Boolean flag = false;
                for (int j = 0; j < k; j++)
                {
                    if (options[j].GetComponent<Option>().getWord() == NN[i].getWordEmbedding().GetWord())
                    {
                        flag = true;
                        exisitingWED[i - countOfNewWords] = neighbour;
                    }
                }
                if (!flag)
                {
                    newWED[countOfNewWords] = neighbour;
                    countOfNewWords += 1;
                }
            }

            Debug.Log("New Words");
            for (int i = 0; i < k + 1; i++)
            {
                if (newWED[i] != null)
                {
                    Debug.Log(newWED[i].getWordEmbedding().GetWord());
                }
            }
            Debug.Log("Existing Words");
            for (int i = 0; i < k + 1; i++)
            {
                if (exisitingWED[i] != null)
                {
                    Debug.Log(exisitingWED[i].getWordEmbedding().GetWord());
                }
            }

            for (int i = 0; i < k + 1; i++)
            {
                GameObject option = options[i];
                Option o = option.GetComponent<Option>();
            
                if (o.getWord() == Target.GetWord())
                {
                    o.SetWordEmbedding(Target);
                    o.setTargetPosition(transform.position);
                }
                else
                {
                    Boolean flag = false;
                    for (int j = 0; j < k + 1; j++)
                    {
                        if (exisitingWED[j] != null && exisitingWED[j].getWordEmbedding().GetWord() == o.getWord())
                        {
                            flag = true;
                            o.SetWordEmbedding(exisitingWED[j].getWordEmbedding());
                            o.setTargetPosition(transform.position + new Vector3(
                                (float)(5 * exisitingWED[j].getPCADistance()[0]),
                                (float)(5 * exisitingWED[j].getPCADistance()[1]),
                                (float)(5 * exisitingWED[j].getPCADistance()[2])));
                        }
                    }
                    if (!flag)
                    {
                        countOfNewWords -= 1;

                        o.SetWordEmbedding(newWED[countOfNewWords].getWordEmbedding());
                        o.setPosition(transform.position + new Vector3(
                                (float)(5 * newWED[countOfNewWords].getPCADistance()[0]),
                                (float)(5 * newWED[countOfNewWords].getPCADistance()[1]),
                                (float)(5 * newWED[countOfNewWords].getPCADistance()[2])));
                        o.setTargetPosition(transform.position + new Vector3(
                                (float)(5 * newWED[countOfNewWords].getPCADistance()[0]),
                                (float)(5 * newWED[countOfNewWords].getPCADistance()[1]),
                                (float)(5 * newWED[countOfNewWords].getPCADistance()[2])));
                    }
                }
            }

            //int newOptionCounter = 0;
            //for (int i = 0; i < k; i++)
            //{
            //    GameObject option = options[i];
            //    Option o = option.GetComponent<Option>();
            //
            //    Boolean flag = false;
            //    for (int j = 0; j < k; j++)
            //    {
            //        if (exisitingWED[j] != null && exisitingWED[j].getWordEmbedding().GetWord() == o.getWord())
            //        {
            //            flag = true;
            //            o.SetWordEmbedding(exisitingWED[j].getWordEmbedding());
            //            o.setTargetPosition(transform.position + new Vector3(
            //                (float)(5 * exisitingWED[j].getPCADistance()[0]),
            //                (float)(5 * exisitingWED[j].getPCADistance()[1]),
            //                (float)(5 * exisitingWED[j].getPCADistance()[2])));
            //        }
            //    }
            //    if (!flag)
            //    {
            //        o.SetWordEmbedding(newWED[newOptionCounter].getWordEmbedding());
            //        o.setPosition(transform.position + new Vector3(
            //                (float)(5 * newWED[newOptionCounter].getPCADistance()[0]),
            //                (float)(5 * newWED[newOptionCounter].getPCADistance()[1]),
            //                (float)(5 * newWED[newOptionCounter].getPCADistance()[2])));
            //        newOptionCounter += 1;
            //    }
            //}

            //GameObject targetOption = options[0];
            //Option to = targetOption.GetComponent<Option>();

            //to.SetWordEmbedding(Target);
            //targetOption.transform.position = transform.position;

            //for (int i = 0; i < k; i++)
            //{
            //    WordEmbeddingDistance neighbour = NN[i];
            //    GameObject option = options[i + 1];
            //    Option o = option.GetComponent<Option>();

            //    o.SetWordEmbedding(NN[i].getWordEmbedding());
            //    o.setTargetPosition(transform.position + new Vector3(
            //            (float)(5 * neighbour.getPCADistance()[0]),
            //            (float)(5 * neighbour.getPCADistance()[1]),
            //            (float)(5 * neighbour.getPCADistance()[2])));
            //}
        }
    }
}
