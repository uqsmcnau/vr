using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Option : Selectable
{
    private Scoreboard parent;
    private WordEmbedding we;
    public Camera cam;
    public TextMeshPro text;
    
    private float startTimer = 0.0f;
    private readonly float moveWindow = 15.0f;
    private Vector3 startPosition;
    private Vector3 targetPosition;

    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.position;
        targetPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.LookRotation(GetComponent<Renderer>().bounds.center - cam.transform.position);

        if (targetPosition != transform.position)
        {
            float currentTime = Time.time;
            if (currentTime < (startTimer + moveWindow)) {
                transform.position = startPosition + (((currentTime - startTimer) / moveWindow) * (targetPosition - startPosition));
            } else {
                transform.position = targetPosition;
            }
        }
    }

    public void SetParent(Scoreboard p)
    {
        parent = p;
    }

    public void SetWordEmbedding(WordEmbedding e)
    {
        we = e;

        text.text = we.GetWord();
    }
    
    public override void Select()
    {
        parent.SetTarget(we);
        parent.Select();
    }

    public void setTargetPosition(Vector3 targetPosition)
    {
        startPosition = transform.position;
        startTimer = Time.time;
        this.targetPosition = targetPosition;
    }
}
