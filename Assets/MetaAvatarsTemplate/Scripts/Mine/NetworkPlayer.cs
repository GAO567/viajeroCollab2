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
    TaskManager manager;

    PhotonView photonView;

    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        manager = GameObject.Find("RootAreas").GetComponent<TaskManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            //head.GetComponentInChildren<MeshRenderer>().enabled = false;
            rightHand.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
            leftHand.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
            gameObject.name = "Local Network Player";
            LineRenderer raycasterRight = rightHand.GetComponentInChildren<LineRenderer>();
            LineRenderer raycasterLeft = leftHand.GetComponentInChildren<LineRenderer>(); 
            ViewportDrawer viewportDrawer = head.GetComponentInChildren<ViewportDrawer>();
            if (raycasterLeft && raycasterRight)
            {
                raycasterRight.material = localMaterial;
                raycasterLeft.material = localMaterial;
            }
            if (viewportDrawer)
            {
                viewportDrawer.lineRenderer.material = localMaterial;
                viewportDrawer.gameObject.gameObject.SetActive(false);
            }
        }
        else
        {
            gameObject.name = "Remote Network Player";
            LineRenderer raycasterRight = rightHand.GetComponentInChildren<LineRenderer>();
            LineRenderer raycasterLeft = leftHand.GetComponentInChildren<LineRenderer>();
            ViewportDrawer drawer = head.GetComponentInChildren<ViewportDrawer>();
            if (manager)
            {
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

                /*
                if (manager.collabType == CollabType.CoupledView)
                {
                if(head)
                    head.GetComponentInChildren<ViewportDrawer>().gameObject.SetActive(true);
                }
                else
                {
                if(head)
                    head.GetComponentInChildren<ViewportDrawer>().gameObject.SetActive(false);
                }*/
                //                }
            }
            if (raycasterLeft && raycasterRight)
            {
                raycasterRight.material = remoteMaterial;
                raycasterLeft.material = remoteMaterial;
            }
            if (drawer)
            {
                drawer.lineRenderer.material = remoteMaterial;
            }
        }


        MapPosition(head, XRNode.Head);
        MapPosition(rightHand, XRNode.RightHand);
        MapPosition(leftHand, XRNode.LeftHand);
        
    }

    void MapPosition(Transform target, XRNode node)
    {
        InputDevices.GetDeviceAtXRNode(node).TryGetFeatureValue(CommonUsages.devicePosition,out Vector3 position);
        InputDevices.GetDeviceAtXRNode(node).TryGetFeatureValue(CommonUsages.deviceRotation,out Quaternion rotation);

        target.localPosition = position;
        target.localRotation = rotation;
    }
}
