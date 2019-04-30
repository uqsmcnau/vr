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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.LookRotation(GetComponent<Renderer>().bounds.center - cam.transform.position);

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
}
