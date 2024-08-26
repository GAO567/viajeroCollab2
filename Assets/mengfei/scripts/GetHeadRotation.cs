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
    public GameObject leftCapsule;
    public GameObject rightCapsule;
    public GameObject commonCapsule;
    public Transform leftT;
    public Transform rightT;
    public eyeBlink eye;
    [Range(0.0f, 1.0f)] public float transparency = 0f;

    #endregion

    #region Private field
    private RotationManager rotationManager;
    private int Remote; //1 for left avatar, 2 for right avatar, use pun to send to others' client
    private float headRotationY;
    private bool avatarTransformed = false;
    private bool Angled90;
    private bool sideByside;
    private GameObject leftBystander;
    private GameObject rightBystander;
    #endregion


    #region MonoBehaviour Callbacks
    void Start()
    {
        taskManager = GameObject.FindObjectOfType<TaskManager>();
        rotationManager = GameObject.FindObjectOfType<RotationManager>();
        eye = GameObject.FindObjectOfType<eyeBlink>();


        if (rotationManager.bystanderType != BystanderType.Avatar)
        {

            // mix just for angled90 
            if (taskManager.collabType == CollabType.Angled90)
            {
                if (taskManager.isRemotePlayer)
                {

                    if (photonView.IsMine)
                    {
                        //set left to commoncapsule,hide leftavatar and leftcapsule
                        avatar.SetActive(false);
                        leftCapsule.SetActive(false);
                        leftBystander = commonCapsule;
                        if (rotationManager.bystanderType == BystanderType.Capsule) 
                        { 
                            rightBystander = rightCapsule;
                            avatar2.SetActive(false);
                        } 
                        else{
                            rightBystander = avatar2;
                            rightCapsule.SetActive(false);
                        }

                    }
                    else
                    {
                        avatar2.SetActive(false);
                        rightCapsule.SetActive(false);
                        rightBystander = commonCapsule;
                        commonCapsule.SetActive(false);
                        if (rotationManager.bystanderType == BystanderType.Capsule)
                        { 
                            leftBystander = leftCapsule;
                            avatar.SetActive(false) ;
                        }
                        else { 
                            leftBystander = avatar;
                            leftCapsule.SetActive(false);

                        }


                    }
                }
                else
                {
                    if (photonView.IsMine)
                    {
                        avatar2.SetActive(false);
                        rightCapsule.SetActive(false);
                        rightBystander = commonCapsule;
                        if (rotationManager.bystanderType == BystanderType.Capsule)
                        { 
                            leftBystander = leftCapsule;
                            avatar.SetActive(false);
                        }
                        else { 
                            leftBystander = avatar;
                            leftCapsule.SetActive(false);
                        }


                    }
                    else
                    {
                        avatar.SetActive(false);
                        leftCapsule.SetActive(false);
                        leftBystander = commonCapsule;
                        commonCapsule.SetActive(false);
                        if (rotationManager.bystanderType == BystanderType.Capsule)
                        { 
                            rightBystander = rightCapsule;
                            avatar2.SetActive(false);
                        }
                        else { 
                            rightBystander = avatar2;
                            rightCapsule.SetActive(false);
                        }
                    }
                }
            }
            else
            {
                //just keep capusles for other conditions
                avatar.SetActive(false) ;
                avatar2.SetActive(false) ;
                commonCapsule.SetActive(false);
                leftBystander = leftCapsule;
                rightBystander = rightCapsule;
            }
                
        } 
       
        else
        {
            leftCapsule.SetActive(false);
            rightCapsule.SetActive(false);
            commonCapsule.SetActive(false);
            leftBystander = avatar;
            rightBystander = avatar2;
        }
        //        head = Camera.main.transform;

        //left avatar
        SetChildrenTransparency(leftBystander, 0);
        //right avatar
        SetChildrenTransparency(rightBystander, 0);
        //avatar in the corner (angled90)
        SetChildrenTransparency(commonCapsule, 0);


        Angled90 = false;
        // for sidebyside condition
        if (taskManager.collabType == CollabType.SideBySide)
        {
            //hide the avatar to avoid blocking user's view
            if (taskManager.isRemotePlayer)
            {
                if (photonView.IsMine)
                {
                    //leftT.gameObject.SetActive(false);

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
                   // rightT.gameObject.SetActive(false);
                }
                else
                {
                    leftT.gameObject.SetActive(false);
                }
            }
            sideByside = true;

        }else if (taskManager.collabType == CollabType.Angled90){

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
            //eyeIcon
            if (rotationManager.isEyeIcon)
            {
                eye.UpdateEye(transparency,left);
            }
            //for sidebyside, make the transparency range to 0-0.9(only for the avatar that may occlude the player avtar)
            if (sideByside)
            {
                if(taskManager.isRemotePlayer)
                {
                    if (left)
                    {
                        transparency = transparency * 0.9f;
                    }
                }
                else
                {
                    if(!left)
                    {
                        transparency = transparency * 0.9f;
                    }
                }
            }
           
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
                SetChildrenTransparency(leftBystander, 0);
                SetChildrenTransparency(rightBystander, 0);


            }
            if (left)
            {
                SetChildrenTransparency(leftBystander, transparency);
                SetChildrenTransparency(rightBystander, 0);

                Remote = 1;
            }
            else {
                SetChildrenTransparency(rightBystander, transparency);
                SetChildrenTransparency(leftBystander, 0);

                Remote = 2;
            }

        }
        else
        {
            //when puntonview is not mine
            if(Remote == 1)
            {
                SetChildrenTransparency(leftBystander, transparency);
                SetChildrenTransparency(rightBystander, 0);


            }
            else if (Remote == 2)
            {
                SetChildrenTransparency(rightBystander, transparency);
                SetChildrenTransparency(leftBystander, 0);

            }
            else
            {
                SetChildrenTransparency(leftBystander, 0);
                SetChildrenTransparency(rightBystander, 0);

            }

            
        }
        if (Angled90)
        {
            if (!taskManager.isRemotePlayer)
            {
                //slightly change the position of the corner avatar
                if (photonView.IsMine)
                {
                    commonCapsule.transform.position = new Vector3(0.6f, 0.1f, -0.4f);
                }
                /*else
                {
                    float left_x = leftCapsule.transform.position.x;
                    float left_z = leftCapsule.transform.position.z;
                    leftCapsule.transform.position = new Vector3(left_x, 0.1f, left_z);

                }*/
            }
            else
            {
                if (photonView.IsMine)
                {
                    commonCapsule.transform.localRotation = Quaternion.Euler(new Vector3(0, 135, 0));
                    commonCapsule.transform.position = new Vector3(0.6f, 0.1f, -0.4f);

                }
                /*else
                {
                    float right_x = rightCapsule.transform.position.x;
                    float right_z = rightCapsule.transform.position.z;
                    rightCapsule.transform.position = new Vector3(right_x, 0.1f, right_z);
                }*/
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
                            //outline color and width
                            material.SetFloat("_OutlineWidth", 6);
                            material.SetColor("_OutlineColor", Color.white);
                        }
                    }
                    else
                    {
                        if (rotationManager.bystanderType == BystanderType.Capsule)
                        {
                            if (material.name.Contains("Capsule")) {
                                if (taskManager.isRemotePlayer)
                                {
                                    if (photonView.IsMine)
                                    {
                                        material.color = Color.blue;

                                    }
                                    else
                                    {
                                        material.color = Color.red;
                                    }
                                }
                                else
                                {
                                    if (photonView.IsMine)
                                    {
                                        material.color = Color.red;

                                    }
                                    else
                                    {
                                        material.color = Color.blue;
                                    }
                                }
                            }

                            
                        }
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
