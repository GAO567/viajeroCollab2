using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Photon.Pun;

public class NetworkPlayer : MonoBehaviour
{
    
    public Transform head;
    public Transform rightHand;
    public Transform leftHand;
    public Material remoteMaterial;
    public Material localMaterial;
    public GameObject rayRightHand;
    public GameObject rayLeftHand;
    public ViewportDrawer drawer;
    TaskManager manager;
    public float headLowThreshold = -30.0f;
    public float headHighThreshold = 30.0f;
    PhotonView photonView;

    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        manager = GameObject.Find("RootAreas").GetComponent<TaskManager>();
        drawer = head.gameObject.GetComponentInChildren<ViewportDrawer>();
        drawer.GetComponent<LineRenderer>().enabled = false;
    }

    private void drawRayAndViewport()
    {
        if (manager)
        {

        }
    }

    // Update is called once per frame
    void Update()
    {

        LineRenderer raycasterRight = rightHand.GetComponentInChildren<LineRenderer>();
        LineRenderer raycasterLeft = leftHand.GetComponentInChildren<LineRenderer>();
        ViewportDrawer viewportDrawer = head.GetComponentInChildren<ViewportDrawer>();
        if (manager)
        {
            
        }

        if (photonView.IsMine)
        {
            //head.GetComponentInChildren<MeshRenderer>().enabled = false;
            rightHand.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
            leftHand.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
            gameObject.name = "Local Network Player";
            
            manager.setPlayer1(head.gameObject, rightHand.gameObject, leftHand.gameObject);
            MapPosition(head, XRNode.Head);
            MapPosition(rightHand, XRNode.RightHand);
            MapPosition(leftHand, XRNode.LeftHand);

            if (manager)
            {
                if (manager.isRemotePlayer)
                {
                    if (raycasterLeft && raycasterRight)
                    {
                        raycasterRight.material = remoteMaterial;
                        raycasterLeft.material = remoteMaterial;
                    }
                    if (drawer)
                    {
                        drawer.lineRenderer.material = remoteMaterial;
                    }
                    //if (manager.taskStartedP2)
                    //{
                        manager.drawBoundaryViolationP2(head.gameObject, rightHand.gameObject, leftHand.gameObject);
                    //}
                }
                else
                {
                    if (raycasterLeft && raycasterRight)
                    {
                        raycasterRight.material = localMaterial;
                        raycasterLeft.material = localMaterial;
                    }
                    //if (viewportDrawer)
                    //{
                    //    viewportDrawer.lineRenderer.material = localMaterial;
                    //    viewportDrawer.gameObject.gameObject.SetActive(false);
                    //}
                }
            }
        }
        else // !isMine
        {
            gameObject.name = "Remote Network Player";
            raycasterRight = rightHand.GetComponentInChildren<LineRenderer>();
            raycasterLeft = leftHand.GetComponentInChildren<LineRenderer>();
            if (manager)
            {
                //if (manager.currentTaskState > TaskState.BothConnected)
                //{

                //}
                if (manager.isRemotePlayer)
                {
                    if (raycasterLeft && raycasterRight)
                    {
                        raycasterRight.material = localMaterial;
                        raycasterLeft.material = localMaterial;
                    }
                    if (drawer)
                    {
                        drawer.lineRenderer.material = localMaterial;
                    }
                    //if (manager.taskStartedP2)
                    //{
                        manager.drawBoundaryViolationP2(head.gameObject, rightHand.gameObject, leftHand.gameObject);
                    //}
                }
                else
                {
                    if (raycasterLeft && raycasterRight)
                    {
                        raycasterRight.material = remoteMaterial;
                        raycasterLeft.material = remoteMaterial;
                    }
                    if (viewportDrawer)
                    {
                        viewportDrawer.lineRenderer.material = remoteMaterial;
                        viewportDrawer.gameObject.gameObject.SetActive(false);
                    }
                }


                if (manager.collabType == CollabType.CoupledView)
                {
                    rightHand.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
                    leftHand.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true; 
                    //head.GetComponentInChildren<MeshRenderer>().enabled = false;
                }
                else
                {
                    rightHand.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
                    leftHand.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
                }

                
                if (manager.collabType == CollabType.CoupledView || manager.collabType == CollabType.SideBySide)
                {
                    if (drawer)
                    {
                        drawer.gameObject.SetActive(true);
                        drawer.GetComponent<LineRenderer>().enabled = true;
                    }
                }
                else
                {
                    if (head)
                    {
                        //print(" head = " + head.transform.localEulerAngles.ToString());
                        if(head.transform.localEulerAngles.y < headLowThreshold && head.transform.localEulerAngles.y > headHighThreshold)
                        {
                            if (drawer)
                            {
                                drawer.gameObject.SetActive(true);
                                drawer.GetComponent<LineRenderer>().enabled = true;
                            }
                               
                            
                            //print("head angle " + head.transform.localEulerAngles.ToString());
                        }
                        else
                        {
                            if(drawer)
                                drawer.gameObject.SetActive(false);
                        }
                    }
                    
                }
                manager.setPlayer2(head.gameObject, rightHand.gameObject, leftHand.gameObject);
                //                }
            }
            
        }


        
        
    }

    void MapPosition(Transform target, XRNode node)
    {
        InputDevices.GetDeviceAtXRNode(node).TryGetFeatureValue(CommonUsages.devicePosition,out Vector3 position);
        InputDevices.GetDeviceAtXRNode(node).TryGetFeatureValue(CommonUsages.deviceRotation,out Quaternion rotation);

        target.localPosition = position;
        target.localRotation = rotation;
    }
}
