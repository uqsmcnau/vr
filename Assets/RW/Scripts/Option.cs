using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Option : Selectable
{
    private WordEmbeddingModel parent;
    private WordEmbedding we;

    public Camera cam;
    public TextMeshPro text;
    
    private float startTimer = 0.0f;
    private readonly float moveWindow = 3.0f;
    private Vector3 startingPosition;
    private Vector3 targetPosition;

    private bool fadingIn = false;
    private bool fadingOut = false;
    
    private Color color;
    
    // Start is called before the first frame update
    void Start()
    {
        startingPosition = transform.position;
        targetPosition = transform.position;

        color = GetComponent<MeshRenderer>().material.color;
}

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.LookRotation(GetComponent<Renderer>().bounds.center - cam.transform.position);

        if ((transform.position != this.targetPosition) || fadingOut)
        {
            float currentTime = Time.time;
            if (currentTime < (startTimer + moveWindow)) {
                transform.position = startingPosition + (((currentTime - startTimer) / moveWindow) * (targetPosition - startingPosition));

                if (fadingIn)
                {
                    GetComponent<MeshRenderer>().material.color = new Color(color.r, color.g, color.b, color.a * ((currentTime - startTimer) / moveWindow));
                }
                else if (fadingOut)
                {
                    GetComponent<MeshRenderer>().material.color = new Color(color.r, color.g, color.b, color.a * (1.0f - ((currentTime - startTimer) / moveWindow)));
                }
                else
                {
                    GetComponent<MeshRenderer>().material.color = color;
                }
            } else {
                transform.position = targetPosition;
                GetComponent<MeshRenderer>().material.color = color;

                if (fadingIn)
                {
                    fadingIn = false;

                }
                if (fadingOut)
                {
                    Object.Destroy(this.gameObject);
                }
            }
        }
    }

    public void SetParent(WordEmbeddingModel p)
    {
        parent = p;
    }

    public void SetWordEmbedding(WordEmbedding e)
    {
        we = e;

        text.text = we.GetWord();
    }

    public WordEmbedding GetWordEmbedding()
    {
        return we;
    }
    
    public override void Select()
    {
        parent.SetTarget(we);
        parent.Select();
    }

    public void SetPosition(Vector3 targetPosition)
    {
        transform.position = targetPosition;
    }

    public void SetTargetPosition(Vector3 targetPosition)
    {
        startingPosition = transform.position;
        startTimer = Time.time;
        this.targetPosition = targetPosition;
    }

    public string getWord()
    {
        return we.GetWord();
    }

    public void SetVisible()
    {
        fadingIn = false;
        fadingOut = false;
    }

    public void StartFadeIn()
    {
        fadingIn = true;
        fadingOut = false;
    }

    public void StartFadeOut()
    {
        fadingIn = false;
        fadingOut = true;

        Object.Destroy(GetComponent<BoxCollider>());
    }
}
