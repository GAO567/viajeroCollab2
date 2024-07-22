using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Pun;
using Photon.Voice;

public class GetHeadRotation : MonoBehaviourPun, IPunObservable
{
    #region Public field
    public GameObject playerAreaCenter;
    public TaskManager taskManager;
    public Transform head;
    public GameObject avatar;
    public GameObject avatar2;
    public Transform leftT;
    public Transform rightT;
    [Range(0.0f, 1.0f)] public float transparency = 0f;

    #endregion

    #region Private field
    private RotationManager rotationManager;
    private int Remote; //1 for left avatar, 2 for right avatar, use pun to send to others' client
    private float headRotationY;
    private bool avatarTransformed = false;
    private bool Angled90;
    #endregion

 
    #region MonoBehaviour Callbacks
    void Start()
    {
        taskManager = GameObject.FindObjectOfType<TaskManager>();
        rotationManager = GameObject.FindObjectOfType<RotationManager>();


//        head = Camera.main.transform;
        
        //left avatar
        SetChildrenTransparency(avatar, transparency);
        //right avatar
        SetChildrenTransparency(avatar2, transparency);

        Angled90 = false;
        // for sidebyside condition
        if (taskManager.collabType == CollabType.SideBySide)
        {
            //hide the right avatar of localplayer,hide the left avatar of remoteplayer
            if (taskManager.isRemotePlayer)
            {
                if (photonView.IsMine)
                {
                    leftT.gameObject.SetActive(false);

                }
                else
                {
                    rightT.gameObject.SetActive(false);
                }
            }
            else
            {
                if (photonView.IsMine)
                {
                    rightT.gameObject.SetActive(false);
                }
                else
                {
                    leftT.gameObject.SetActive(false);
                }
            }

        }else if (taskManager.collabType == CollabType.Angled90){
            //the avatars that intersect with each other 
            //(local' right avatar and remote's left avatar)
            //share the same transparency(the bigger one)
            Angled90 = true;

        }
        else if (taskManager.collabType == CollabType.CoupledView)
        {
            //for coupled view, we don't need the photon transform view and material view
            //just let the avatars be lcoal
            photonView.Synchronization = ViewSynchronization.Off;

        }

    }
    void Update()
    {
       

        if (!head)
        {
            getHead();
        }
        if (taskManager != null)
        {

            if (taskManager.taskStarted)
            {
                //disable the PhotonTransformView if task starts
                gameObject.GetComponent<PhotonTransformView>().enabled = false;
                // with head gain, we should slight modify the position of the bystanders
                if (rotationManager.isHeadGain && !avatarTransformed)
                {
                    leftT.Rotate(0, -30, 0, Space.World);
                    rightT.Rotate(0, 30, 0, Space.World);

                    avatarTransformed = true;
                }
                else if(!rotationManager.isHeadGain && avatarTransformed)
                {
                    leftT.localEulerAngles = Vector3.zero;
                    rightT.localEulerAngles = Vector3.zero;
                    avatarTransformed = false;
                }
                
            }
        }
        //let the bystanders look at the player
        /*if (head != null)
        {
            Vector3 lookAtDirection = head.position - avatar.transform.position;
            Vector3 lookAtDirection2 = head.position - avatar2.transform.position;

            lookAtDirection.y = 0f; // Ignore the vertical component
            lookAtDirection2.y = 0f;

            // Use Quaternion.LookRotation to determine the rotation to look at the camera
            Quaternion rotation = Quaternion.LookRotation(lookAtDirection);
            Quaternion rotation2 = Quaternion.LookRotation(lookAtDirection2);


            // Set the avatar's rotation to only rotate around the Y-axis
            avatar.transform.rotation = Quaternion.Euler(0f, rotation.eulerAngles.y, 0f);
            avatar2.transform.rotation = Quaternion.Euler(0f, rotation2.eulerAngles.y, 0f);

        }*/
            
        if (photonView.IsMine)
        {
            transform.localPosition = playerAreaCenter.transform.localPosition;
            //transform.localRotation = playerAreaCenter.transform.localRotation;

            if (rotationManager.isHeadGain)
            {
                float yrotation = head.localEulerAngles.y;
                Vector3 euler = Vector3.zero;
                // gain = head(1x)+ playArea(0.5x) = 1.5x
                //turn right
                if (yrotation < 60.0f)
                {
                    euler.y = yrotation/2;
                    playerAreaCenter.transform.localRotation = Quaternion.Euler(euler);

                }//turn left
                else if (yrotation > 300.0f)
                {
                    euler.y = (360+yrotation) / 2;
                    playerAreaCenter.transform.localRotation = Quaternion.Euler(euler);

                }

            }
            else
            {
                playerAreaCenter.transform.localRotation = Quaternion.identity;
            }


            //check which avatar to show,  right or left
            bool left = false;

            headRotationY = head.localEulerAngles.y;
            if (headRotationY > 180)
            {
                left = true;
                headRotationY = 360-headRotationY;
            }
            transparency = headRotationY > 90f ? 1f : headRotationY/90.0f;
            //
           
            //
            //notify intrusion 
            if (headRotationY < 30.0f)
            {
                //Debug.Log("seatback zone");
            }   else if (headRotationY < 60.0f)
            {
               // Debug.Log("mild violation");
            }else if (headRotationY < 90.0f)
            {
               // Debug.Log("extreme violation");
            }
            else
            {
                //angles greater than 90
                SetChildrenTransparency(avatar, 0);
                SetChildrenTransparency(avatar2, 0);


            }
            if (left)
            {
                SetChildrenTransparency(avatar, transparency);
                SetChildrenTransparency(avatar2, 0);

                Remote = 1;
            }
            else {
                SetChildrenTransparency(avatar2, transparency);
                SetChildrenTransparency(avatar, 0);

                Remote = 2;
            }

        }
        else
        {
            //when puntonview is not mine
            if(Remote == 1)
            {
                SetChildrenTransparency(avatar, transparency);
                SetChildrenTransparency(avatar2, 0);


            }
            else if (Remote == 2)
            {
                SetChildrenTransparency(avatar2, transparency);
                SetChildrenTransparency(avatar, 0);

            }
            else
            {
                SetChildrenTransparency(avatar, 0);
                SetChildrenTransparency(avatar2, 0);

            }
            //maxmium 3 avatars in the scene, so make one invisable
            if (Angled90)
            {
                if (taskManager.isRemotePlayer)
                {
                    SetChildrenTransparency(avatar2, 0);
                }
                else
                {
                    SetChildrenTransparency(avatar, 0);
                }
            }
        }

 
    }

    private void getHead()
    {
        if (photonView.IsMine)
        {
            head = GameObject.Find("Local Network Player").GetComponent<NetworkPlayer>().head;
        }
        else
        {
            head = GameObject.Find("Remote Network Player").GetComponent<NetworkPlayer>().head;

        }
    }
    #endregion
    #region Method


    void SetChildrenTransparency(GameObject parent, float alpha)
    {
          
        if (parent != null)
        {
            // Get all the MeshRenderers in the children
            Renderer[] renderers = parent.GetComponentsInChildren<Renderer>(true);
            // Set transparency for each renderer
            foreach (Renderer renderer in renderers)
            {
                SetRendererTransparency(renderer, alpha);
            }
        }
    }
    void SetRendererTransparency(Renderer renderer, float alpha)
    {
        if (renderer != null)
        {
            // Ensure materials are not null
            Material[] materials = renderer.materials;

            if (materials != null)
            {
                // Set transparency for each material
                foreach (Material material in materials)
                {
                    if (material.name.Contains("OutlineMask"))
                    {

                    }else if (material.name.Contains("OutlineFill"))
                    {
                        if(alpha == 0)
                        {
                            material.SetFloat("_OutlineWidth", 0);
                        }
                        else
                        {
                            material.SetFloat("_OutlineWidth", 6);
                            material.SetColor("_OutlineColor", Color.white);
                        }
                    }
                    else
                    {
                        Color color = material.color;
                        color.a = alpha;
                        material.color = color;
                    }

                       
                }
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(Remote);
            stream.SendNext(transparency);
        }
        else
        {
            this.Remote = (int)stream.ReceiveNext();
            this.transparency = (float)stream.ReceiveNext();
        }
    }
    #endregion
}
