using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using Accord.Statistics.Analysis;
using Accord.MachineLearning.Clustering;
using System.Data.SqlTypes;

public class WordEmbeddingModel : Selectable
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

    private int k = 10;

    private float zoom = 1.0f;

    public string word;

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

        count = 5000;

        embeddings = new WordEmbedding[count];
        //double[][] data = new double[count][];


        double[] maxs = { 0, 0, 0 };
        double[] mins = { 0, 0, 0 };

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

            WordEmbedding Target = new WordEmbedding(currentword, currentVectors);

            int id = GetWordembeddingID(Target.GetWord());
            double[] pcavectors = GetWordembeddingPCA(id);

            for (int k = 0; k < 3; k++)
            {
                if (pcavectors[k] > maxs[k])
                {
                    maxs[k] = pcavectors[k];
                }
                else if (pcavectors[k] < mins[k])
                {
                    mins[k] = pcavectors[k];
                }
            }
            Target.SetPCAVectors(pcavectors);
            embeddings[counter] = Target;

            counter++;
        }
        file.Close();

        //long time = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        
        UpdateTarget();

        SetTarget(GetWordEmbedding("ocean"));
        Select();
    }

    // Update is called once per frame
    void Update()
    {
        scoreboard.transform.rotation = Quaternion.LookRotation(scoreboard.GetComponent<Renderer>().bounds.center - cam.transform.position);
        if (preview && (counter%10 == 0))
        {
            UpdateTarget();
        } else if (!preview && word != Target.GetWord())
        {
            WordEmbedding we = GetWordEmbedding(word);
            if (we != null)
            {
                SetTarget(we);
                Refresh();
            }
        }
        counter++;
    }

    private WordEmbedding GetWordEmbedding(String word)
    {
        for (int i = 0; i < count; i++)
        {
            if (embeddings[i].GetWord() == word)
            {
                return embeddings[i];
            }
        }
        return null;
    }

    private static int GetWordembeddingID(string word)
    {
        int id = -1;
        MySqlConnection conn = new MySqlConnection("Server=localhost; database=wordembeddings; UID=root; password=password;");
        conn.Open();

        string query = "SELECT id FROM wordembedding WHERE word = @word";
        var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@word", word);

        var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            id = reader.GetInt32(0);
        }

        conn.Close();

        return id;
    }

    private static double[] GetWordembeddingPCA(int id)
    {
        MySqlConnection conn = new MySqlConnection("Server=localhost; database=wordembeddings; UID=root; password=password");
        conn.Open();

        double[] pca = new double[3];

        string query = "SELECT x, y, z FROM wordembedding WHERE id = @id";
        var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@id", id);

        var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            pca[0] = reader.GetDouble(0);
            pca[1] = reader.GetDouble(1);
            pca[2] = reader.GetDouble(2);
        }

        conn.Close();

        return pca;
    }

    public void SetTarget(WordEmbedding we)
    {
        OldTarget = Target;
        Target = we;
        word = Target.GetWord();
        Target.FindNN(embeddings, k);
        text.text = Target.GetWord();
    }
    
    private void UpdateTarget()
    {
        Target = embeddings[(counter / 10) % count];
        Target.FindNN(embeddings, k);
        text.text = Target.GetWord();
    }

    private void Refresh()
    {
        WordEmbeddingDistance[] NN = Target.GetNN();

        GameObject option = options[0];
        Option o = option.GetComponent<Option>();
        o.SetWordEmbedding(Target);
        o.SetTargetPosition(transform.position);
        o.StartFadeIn();

        // For each option update and move or recreate in model
        for (int i = 0; i < k; i++)
        {
            option = options[i + 1];
            o = option.GetComponent<Option>();

            o.SetWordEmbedding(NN[i].getWordEmbedding());
            o.SetPosition(transform.position + new Vector3(
                    (float)(zoom * NN[i].getPCADistance()[0]),
                    (float)(zoom * NN[i].getPCADistance()[1]),
                    (float)(zoom * NN[i].getPCADistance()[2])));
            o.SetTargetPosition(transform.position + new Vector3(
                    (float)(zoom * NN[i].getPCADistance()[0]),
                    (float)(zoom * NN[i].getPCADistance()[1]),
                    (float)(zoom * NN[i].getPCADistance()[2])));
            o.StartFadeIn();
        }
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
                        (float)(zoom * neighbour.getPCADistance()[0]),
                        (float)(zoom * neighbour.getPCADistance()[1]),
                        (float)(zoom * neighbour.getPCADistance()[2]));

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
                                (float)(zoom * exisitingWED[j].getPCADistance()[0]),
                                (float)(zoom * exisitingWED[j].getPCADistance()[1]),
                                (float)(zoom * exisitingWED[j].getPCADistance()[2])));
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
                                (float)(zoom * newWED[countOfNewWords].getPCADistance()[0]),
                                (float)(zoom * newWED[countOfNewWords].getPCADistance()[1]),
                                (float)(zoom * newWED[countOfNewWords].getPCADistance()[2])));
                        o.SetTargetPosition(transform.position + new Vector3(
                                (float)(zoom * newWED[countOfNewWords].getPCADistance()[0]),
                                (float)(zoom * newWED[countOfNewWords].getPCADistance()[1]),
                                (float)(zoom * newWED[countOfNewWords].getPCADistance()[2])));
                        o.StartFadeIn();
                    }
                }
            }
        }
    }

    // Reculate the position of each option
    public void refreshPosition()
    {
        WordEmbeddingDistance[] NN = Target.GetNN();

        Vector3 targetCurrentPosition = transform.position;

        // For each option update and move or recreate in model
        for (int i = 0; i < k + 1; i++)
        {
            GameObject option = options[i];
            Option o = option.GetComponent<Option>();

            for (int j = 0; j < k; j++)
            {
                // If so move it towards its new location relative to the target word
                if (NN[j] != null && NN[j].getWordEmbedding().GetWord() == o.getWord())
                {
                    o.SetTargetPosition(transform.position + new Vector3(
                        (float)(zoom * NN[j].getPCADistance()[0]),
                        (float)(zoom * NN[j].getPCADistance()[1]),
                        (float)(zoom * NN[j].getPCADistance()[2])));
                    o.SetVisible();
                }
            }
        }
        Debug.Log(zoom);
    }

    public void ZoomIn()
    {
        zoom *= 1.25f;
        refreshPosition();
    }

    public void ZoomOut()
    {
        zoom *= 0.8f;
        refreshPosition();
    }
}
