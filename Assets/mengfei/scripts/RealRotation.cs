using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;



public class RealRotation : MonoBehaviourPun, IPunObservable
{
    public TaskManager taskManager;

    private RotationManager rotationManager;
    private Transform arrow;
    private Transform frustum;

    private bool isPlaced=false;
    public Transform head;
    private Material material;
    public Quaternion adjustRotaion;
    private int arrowColor = -1;
    private bool adjusted = false;
    // Start is called before the first frame update
    void Start()
    {
        taskManager = GameObject.FindObjectOfType<TaskManager>();

        arrow = transform.GetChild(0);
        frustum = transform.GetChild(1);
        material = arrow.GetComponent<Renderer>().material;
        rotationManager = GameObject.FindObjectOfType<RotationManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isPlaced)
        {
            //don't know when the avatr head is created, check if the head exists
            //put this gameobject into "Joint Head" so that it can follow the head movement
            if (transform.parent.childCount > 1) {
                transform.SetParent(transform.parent.GetChild(1));
                isPlaced = true;
            }
        }
        //adjustRotation should be same as the PlayerAreaRotation(related to Collabtype)
        // Player1 is default(0,0,0), just do for Player2
        if (!adjusted && taskManager.isRemotePlayer)
        {
            if(taskManager.collabType == CollabType.FacetoFaceIntersect)
            {
                adjustRotaion = Quaternion.Euler(0, 180, 0);
            }else if(taskManager.collabType == CollabType.FaceToFaceNoIntersect)
            {
                adjustRotaion = Quaternion.Euler(0, 180, 0);

            }else if(taskManager.collabType == CollabType.Angled90)
            {
                adjustRotaion = Quaternion.Euler(0, 270, 0);

            }
            else
            {
                adjustRotaion = Quaternion.Euler(0, 0, 0);
            }
            adjusted = true;

        }else if (!adjusted && !taskManager.isRemotePlayer)
        {
            adjustRotaion = Quaternion.Euler(0, 0, 0);
            adjusted = true;

        }
        /*{
            //caculate the adjustroation based on the colla type (player area rotation)
            Debug.Log("name of tpp"+transform.parent.parent.eulerAngles);
            adjustRotaion = Quaternion.Euler(0, transform.parent.parent.eulerAngles.y, 0);
            adjusted = true;
        }*/


        if (isPlaced && adjusted && photonView.IsMine)
        {
            //transform.parent == Joint Head
            //transform.parent.parent == Local Avatar
            if (!head)
            {
                //get the head of local player
                GameObject go = GameObject.Find("Local Network Player");
                if (go != null)
                {
                    head = go.GetComponent<NetworkPlayer>().head;
                }
                gameObject.name = "Local";
            }
            else
            {
                float headRotationY = head.localRotation.eulerAngles.y;
                if (headRotationY > 180)
                {
                    //for negative rotation(turn left), the value is between 270-360
                    headRotationY = 360 - headRotationY;
                }
                if (headRotationY < 30.0f)
                {
                    arrowColor = 0;
                }
                else if (headRotationY < 60.0f)
                {
                    arrowColor = 1;
                }
                else if (headRotationY < 90.0f)
                {
                    arrowColor = 2;
                }
                Quaternion headRotation = head.localRotation;

                if (rotationManager.real_virtualType == RealVirtualType.RArrow_VAvatar)
                {
                    
                    arrow.transform.rotation = headRotation * adjustRotaion * Quaternion.Euler(90, 90, 0);
                    frustum.transform.rotation = headRotation * adjustRotaion;
                }
                else if(rotationManager.real_virtualType == RealVirtualType.RAvatar_VArrow)
                {
                    //since the roatation gain function changes the PlayerCenter, we should keep the avatar at the original position
                    // maybe just show it from the remote player's view...
                    transform.parent.parent.rotation = headRotation * adjustRotaion;
                    if(headRotation.eulerAngles.y > 180)
                    {
                        arrow.transform.rotation = Quaternion.Euler(headRotation.eulerAngles.x, 360.0f- 1.5f * headRotationY, headRotation.eulerAngles.z) * adjustRotaion * Quaternion.Euler(90, 90, 0);

                    }
                    else
                    {
                        arrow.transform.rotation = Quaternion.Euler(headRotation.eulerAngles.x, 1.5f * headRotationY, headRotation.eulerAngles.z) * adjustRotaion * Quaternion.Euler(90, 90, 0);

                    }

                }
               
            }
        }
        if (arrowColor == 0)
        {
            material.color = Color.green;

        } else if (arrowColor == 1)
        {
            material.color = Color.yellow;
        } else if (arrowColor == 2)
        {
            material.color = Color.red;
        }
        else
        {
            material.color = Color.white;
        }
        if (rotationManager.isArrow)
        {
            arrow.gameObject.SetActive(true);
        }
        else
        {
            arrow.gameObject.SetActive(false);
        }
        if(rotationManager.isFrustum)
        {
            frustum.gameObject.SetActive(true);
        }
        else
        {
            frustum.gameObject.SetActive(false);
        }


    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        Debug.Log("arrow send receive");
        if(stream.IsWriting)
        {
            stream.SendNext(arrowColor);
        }
        else
        {
            this.arrowColor = (int)stream.ReceiveNext();
        }
    }
}
