﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridOverlay : MonoBehaviour
{
    public GameObject plane;

    public bool showMain = true;
    public bool showSub = false;

    public int gridSizeX;
    public int gridSizeY;
    public int gridSizeZ;

    public float smallStep;
    public float largeStep;

    public float startX;
    public float startY;
    public float startZ;

    private float offsetY = 0;
    private float scrollRate = 0.1f;
    private float lastScroll = 0f;

    private Material lineMaterial;

    private Color mainColor = new Color(0f, 1f, 0f, 1f);
    private Color subColor = new Color(0f, 0.5f, 0f, 1f);

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {
        if (lastScroll + scrollRate < Time.time)
        {
            if (Input.GetKey(KeyCode.KeypadPlus))
            {
                plane.transform.position = new Vector3(plane.transform.position.x, plane.transform.position.y + smallStep, plane.transform.position.z);
                offsetY += smallStep;
                lastScroll = Time.time;
            }
            if (Input.GetKey(KeyCode.KeypadMinus))
            {
                plane.transform.position = new Vector3(plane.transform.position.x, plane.transform.position.y - smallStep, plane.transform.position.z);
                offsetY -= smallStep;
                lastScroll = Time.time;
            }
        }
    }

    void CreateLineMaterial()
    {

        if (!lineMaterial)
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            var shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            // Turn on alpha blending
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // Turn backface culling off
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            // Turn off depth writes
            lineMaterial.SetInt("_ZWrite", 0);
        }
    }

    void OnPostRender()
    {
        CreateLineMaterial();
        // set the current material
        lineMaterial.SetPass(0);

        GL.Begin(GL.LINES);

        if (showSub)
        {
            GL.Color(subColor);

            //Layers
            for (float j = 0; j <= gridSizeY; j += smallStep)
            {
                //X axis lines
                for (float i = 0; i <= gridSizeZ; i += smallStep)
                {
                    GL.Vertex3(startX, j + offsetY, startZ + i);
                    GL.Vertex3(gridSizeX, j + offsetY, startZ + i);
                }

                //Z axis lines
                for (float i = 0; i <= gridSizeX; i += smallStep)
                {
                    GL.Vertex3(startX + i, j + offsetY, startZ);
                    GL.Vertex3(startX + i, j + offsetY, gridSizeZ);
                }
            }

            //Y axis lines
            for (float i = 0; i <= gridSizeZ; i += smallStep)
            {
                for (float k = 0; k <= gridSizeX; k += smallStep)
                {
                    GL.Vertex3(startX + k, startY + offsetY, startZ + i);
                    GL.Vertex3(startX + k, gridSizeY + offsetY, startZ + i);
                }
            }
        }

        if (showMain)
        {
            GL.Color(mainColor);

            //Layers
            for (float j = 0; j <= gridSizeY; j += largeStep)
            {
                //X axis lines
                for (float i = 0; i <= gridSizeZ; i += largeStep)
                {
                    GL.Vertex3(startX, j + offsetY, startZ + i);
                    GL.Vertex3(gridSizeX, j + offsetY, startZ + i);
                }

                //Z axis lines
                for (float i = 0; i <= gridSizeX; i += largeStep)
                {
                    GL.Vertex3(startX + i, j + offsetY, startZ);
                    GL.Vertex3(startX + i, j + offsetY, gridSizeZ);
                }
            }

            //Y axis lines
            for (float i = 0; i <= gridSizeZ; i += largeStep)
            {
                for (float k = 0; k <= gridSizeX; k += largeStep)
                {
                    GL.Vertex3(startX + k, startY + offsetY, startZ + i);
                    GL.Vertex3(startX + k, gridSizeY + offsetY, startZ + i);
                }
            }
        }


        GL.End();
    }
}
