using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class LaserPointer : MonoBehaviour
{

    public SteamVR_Input_Sources handType;
    public SteamVR_Behaviour_Pose controllerPose;
    public SteamVR_Action_Boolean teleportAction;
    public SteamVR_Action_Boolean moveAction;

    public GameObject laserPrefab;
    private GameObject laser; 
    private Transform laserTransform;
    private Vector3 hitPoint; 
    
    public Transform cameraRigTransform;
    public GameObject teleportReticlePrefab;
    private GameObject reticle;
    private Transform teleportReticleTransform;
    public Transform headTransform;
    public Vector3 teleportReticleOffset;
    public LayerMask teleportMask;
    private bool shouldTeleport;

    public LayerMask selectMask;
    private bool shouldSelect;

    private Selectable target;

    public int moveMode = 0;

    private void ShowLaser(RaycastHit hit)
    {
        laser.SetActive(true);
        laserTransform.position = Vector3.Lerp(controllerPose.transform.position, hitPoint, .5f);
        laserTransform.LookAt(hitPoint);
        laserTransform.localScale = new Vector3(laserTransform.localScale.x,
                                                laserTransform.localScale.y,
                                                hit.distance);
    }

    void Start()
    {
        laser = Instantiate(laserPrefab);
        laserTransform = laser.transform;

        reticle = Instantiate(teleportReticlePrefab);
        teleportReticleTransform = reticle.transform;
    }

    void Update()
    {
        // Flight Mode
        if (moveMode == 1)
        {
            if (moveAction.GetState(handType))
            {
                cameraRigTransform.position += (transform.forward / 10);
            }
        // Teleport Mode
        } else if (moveMode == 2)
        {
            if (moveAction.GetState(handType))
            {
                cameraRigTransform.position += transform.forward;
            }
        }

        if (teleportAction.GetState(handType))
        {
            RaycastHit hit;
            
            if(Physics.Raycast(controllerPose.transform.position, transform.forward, out hit, 100, selectMask))
            {
                hitPoint = hit.point;
                ShowLaser(hit);

                target = hit.transform.GetComponent<Selectable>();

                reticle.SetActive(false);
                shouldTeleport = false;
                shouldSelect = true;
            }
            else if(Physics.Raycast(controllerPose.transform.position, transform.forward, out hit, 100, teleportMask))
            {
                hitPoint = hit.point;
                ShowLaser(hit);

                reticle.SetActive(true);
                teleportReticleTransform.position = hitPoint + teleportReticleOffset;
                shouldTeleport = true;
                shouldSelect = false;
            }
            else 
            {
                shouldTeleport = false;
                shouldSelect = false;
                laser.SetActive(false);
                reticle.SetActive(false);
            }
        }
        else 
        {
            laser.SetActive(false);
            reticle.SetActive(false);
        }

        if (teleportAction.GetStateUp(handType) && shouldTeleport)
        {
            Teleport();
        }


        if (teleportAction.GetStateUp(handType) && shouldSelect)
        {
            Select();
        }
    }

    private void Teleport()
    {
        shouldTeleport = false;
        reticle.SetActive(false);
        Vector3 difference = cameraRigTransform.position - headTransform.position;
        difference.y = 0;
        cameraRigTransform.position = hitPoint + difference;
    }



    private void Select()
    {
        target.Select();
    }
}
