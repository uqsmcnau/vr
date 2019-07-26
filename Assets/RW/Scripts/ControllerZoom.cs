using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class ControllerZoom : MonoBehaviour
{
    public WordEmbeddingModel parent;

    public SteamVR_Input_Sources handType;
    public SteamVR_Behaviour_Pose controllerPose;
    public SteamVR_Action_Boolean zoomInAction;
    public SteamVR_Action_Boolean zoomOutAction;
    public SteamVR_Action_Boolean pauseAction;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // 1
        if (zoomInAction.GetStateUp(handType))
        {
            parent.ZoomIn();
        }

        // 1
        if (zoomOutAction.GetStateUp(handType))
        {
            parent.ZoomOut();
        }

        // 1
        if (pauseAction.GetStateUp(handType))
        {
            parent.Pause();
        }
    }
}
