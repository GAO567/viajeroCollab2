using System.Collections;
using System.Collections.Generic;

using System;
using UnityEngine;
using System.Linq;
using System.Security.Cryptography;

using OculusSampleFramework;
using System.IO;
using UnityEngine.UI;

using TMPro;



public enum CollabType
{
    FacetoFaceIntersect, FaceToFaceNoIntersect, SideBySide, CoupledView, Angled45, Angled90
};

public enum TaskState
{
    Idle, Connected, BothConnected, Player1Dominant, Player2Dominant, InteractingWithObject, EndTask
};

public enum Bodypart
{
    HeadP1, rightHandP1, leftHandP1, HeadP2, rightHandP2, leftHandP2
};


public class TaskManager : MonoBehaviour
{
    public bool isRemotePlayer = false;// { get; private set; }
    [SerializeField] int totalNumberTasks = 4;
    [SerializeField] float totalTimePerTask = 300.0f;
    [SerializeField] float timeTraining = 120.0f;
    public int avatarId = 0;
    public int groupId = 0;
    public CollabType collabType = CollabType.FacetoFaceIntersect;
    public Vector3 boundsSize = new Vector3(0.6f,1.0f,0.8f);

    [SerializeField] bool debug = false;
    public  GameObject Player1Area;
    public  GameObject Player2Area;
    [SerializeField] PuzzleGenerator generator;
    [SerializeField] GameObject userHead;
    List<GameObject> puzzleObjects;
    public List<GameObject> blueprintObjects = new List<GameObject>();

    public GameObject rootPossiblePositionsForPuzzle;
    public GameObject distractorsRoot;
    List<GameObject> listPossiblePositionsForPuzzle;
    List<GameObject> currentBlueprint;

    string pathDirectory = "";

    List<TaskLog> logTasks;
    List<GameObject> objectPartsForThisTask;
    TaskLog currentTaskLog;
    public int currentTask = 0;
    bool training = false;

    List<float[]> conditionsByUserId;
    //List<TaskCondition[]> taskConditionsByUserId;
    public float angleBetween;
    private string dominantplayer;

    GameObject headPlayer1;
    GameObject rightHandPlayer1;
    GameObject leftHandPlayer1;

    [NonSerialized]
    public GameObject transformRootForP1Blueprint;
    WireareaDrawer boundaryDrawerP1;

    GameObject headPlayer2;
    GameObject leftHandPlayer2;
    GameObject rightHandPlayer2;
    GameObject transformRootForP2Blueprint;
    WireareaDrawer boundaryDrawerP2;
    
    GameObject rootForObjects;



    bool player1Interacting = false;
    bool player2Interacting = false;

    string[] headerStrTaskLog = new string[2];
    string[] headerStrMovement = new string[2];

    GameObject objP1;// = new GameObject("partBeingInteracted");
    GameObject objP2;// = new G

    public string PathDirectory { get => pathDirectory; set => pathDirectory = value; }
    

    public TaskState currentTaskState = TaskState.Idle;

    public string headerPlayer1Interaction = "userId,currentTask,dominantPlayer," + Utils.vecNameToString("centerBoundsPos") + "," + Utils.vecNameToString("centerAreaRot") + "," +
                                                Utils.vecNameToString("headPos") + "," + Utils.vecNameToString("headRot") + ","+  Utils.vecNameToString("rightHandPos") + Utils.vecNameToString("rightHandRot") + ","+
                                                Utils.vecNameToString("leftHandPos") + Utils.vecNameToString("leftHandRot") + "partId," + Utils.vecNameToString("objInteractedPos") + "," + Utils.vecNameToString("objInteractedRot") + "\n";
    
    public string player1InteractionStr = "userId,dominantPlayer,timestamp,collabType,currentTask,dominantPlayer,isViolatingBoundary," + Utils.vecNameToString("Player1AreaCenter") + "," + Utils.vecNameToString("Player1AreaRot")
                                + Utils.vecNameToString("headPosP1") + "," + Utils.vecNameToString("headRotP1") + "," + Utils.vecNameToString("rightHandPosP1") + Utils.vecNameToString("rightHandRotP1")
                                 + Utils.vecNameToString("leftHandPosP1") + "," + Utils.vecNameToString("leftHandRotP1") + "\n";
    private string player2InteractionStr = "userId,dominantPlayer,timestamp,collabType,currentTask,dominantPlayer,isViolatingBoundary," + Utils.vecNameToString("Player2AreaCenter") + "," + Utils.vecNameToString("Player2AreaRot")
                                + Utils.vecNameToString("headPosP2") + "," + Utils.vecNameToString("headRotP2") + "," + Utils.vecNameToString("rightHandPosP2") + Utils.vecNameToString("rightHandRotP2")
                                 + Utils.vecNameToString("leftHandPosP2") + "," + Utils.vecNameToString("leftHandRotP2") + "\n";

    string logTaskAccuracy = "UserId,collaborationType,dominantPlayer,currentTask,NameBlueprintObj," + Utils.vecNameToString("blueprintPos") + ",NameUserPlacedObj," + Utils.vecNameToString("UserPlacedPos") + "," + 
        "AbsoluteDistance," + Utils.vecNameToString("RelativeDistance")+ ",correctObject\n";

    Dictionary<Bodypart, BoundaryViolation> listActiveViolations = new Dictionary<Bodypart, BoundaryViolation>();
    List<BoundaryViolation> finishedViolations = new List<BoundaryViolation>();


    bool objInteractedP1 = false;
    float objP1InteractedInitTIme = 0;
    bool objInteractedP2 = false;
    float objP2InteractedInitTIme = 0;

    bool taskStarted = false;

    [NonSerialized]
    public bool taskStartedP2 = false;

    bool outsideBoundsLastFrameP1 = false;
    bool outsideBoundsLastFrameP2 = false;

    //deals with the player number
    int playerNumber = 0;

    public bool drawAreas = true;
    int correctObjectsPerTask = 0;

    float timeRemaining = 0;

    bool remoteTaskStarted = false;
    [SerializeField] TMP_Text debugTextLabel;
    [SerializeField] TMP_Text timerText;
    [SerializeField] TMP_Text dominantplayerLabel;
    public void setPlayerNumber(int number)
    {
        playerNumber = number;
    }

    bool hiddenAvatar = false;

    //receive the message and set the references for the gameobjects of the corresponding player (for log purposes) 
    public void setReference(GameObject obj)
    {
        print("Player Number" + playerNumber);
        if (playerNumber == 1)
        {
            GameObject go = GameObject.Find("Local Network Player");
            OVRCameraRig player1 = obj.GetComponent<OVRCameraRig>();
            
            rightHandPlayer1 = player1.rightControllerAnchor.gameObject;
            headPlayer1 = player1.centerEyeAnchor.gameObject;
            leftHandPlayer1 = player1.leftControllerAnchor.gameObject;
            if (go)
            {
                NetworkPlayer networkPlayer = go.GetComponent<NetworkPlayer>();
                rightHandPlayer1 = networkPlayer.rightHand.gameObject;
                leftHandPlayer1 = networkPlayer.leftHand.gameObject;
                headPlayer1 = networkPlayer.head.gameObject;
            }
            currentTaskState = TaskState.Connected;
            //Photon.Pun.PhotonNetwork.Instantiate("DistractorCube", new Vector3(1, 0, 0), Quaternion.identity);

        }
        else if (playerNumber == 2)
        {
            if (obj.name == "RemoteAvatar")
            {
                for (int i = 0; i < obj.transform.childCount; i++)
                {
                    /*
                     *   Joint Head
                     *   Joint RightHandIndexProximal
                     *   Joint LeftHandIndexProximal
                     *   Joint Chest
                     *   Joint LeftHandWrist
                     *   Joint RightHandWrist
                     * 
                     * 
                     */
                    GameObject child = obj.transform.GetChild(i).gameObject;
                    if (child.name == "Joint Head")
                    {
                        headPlayer2 = child;
                    }
                    else if (child.name == "Joint RightHandWrist")
                    {
                        rightHandPlayer2 = child;
                    }
                    else if (child.name == "Joint LeftHandWrist")
                    {
                        leftHandPlayer2 = child;
                    }
                }
            }
            GameObject go = GameObject.Find("Remote Network Player");
            if (go)
            {
                NetworkPlayer networkPlayer = go.GetComponent<NetworkPlayer>();
                rightHandPlayer2 = networkPlayer.rightHand.gameObject;
                leftHandPlayer2 = networkPlayer.leftHand.gameObject;
                headPlayer2 = networkPlayer.head.gameObject;
            }
            //Player2Area.GetComponent<Photon.Pun.PhotonView>().RPC("testStuff", Photon.Pun.RpcTarget.AllBuffered, true);
            /*
            OVRCameraRig player2 = obj.GetComponent<OVRCameraRig>();
            rightHandPlayer2 = player2.rightControllerAnchor.gameObject;
            headPlayer2 = player2.centerEyeAnchor.gameObject;
            leftHandPlayer2 = player2.leftControllerAnchor.gameObject;*/

            currentTaskState = TaskState.BothConnected;
        }
        else
        {
            //do nothing... for now
        }
    }

    

    void setHeaders()
    {
        headerStrMovement[0] =  "userId,timestamp,collabType,currentTask,dominantPlayer,isViolatingBoundary," + Utils.vecNameToString("Player1AreaCenter") + "," + Utils.vecNameToString("Player1AreaRot")
                                + Utils.vecNameToString("headPosP1") + "," + Utils.vecNameToString("headRotP1") + "," + Utils.vecNameToString("rightHandPosP1") + Utils.vecNameToString("rightHandRotP1")
                                 + Utils.vecNameToString("leftHandPosP1") + "," + Utils.vecNameToString("leftHandRotP1") + "\n";
        ;
        headerStrMovement[1] = "";

        headerStrTaskLog[0] =  "userId,currentTask,dominantPlayer," + Utils.vecNameToString("centerBoundsPosP1") + "," + Utils.vecNameToString("centerAreaRotP1") + "," +
                                                Utils.vecNameToString("headPosP1") + "," + Utils.vecNameToString("headRotP1") + "," + Utils.vecNameToString("rightHandPosP1") + Utils.vecNameToString("rightHandRotP1") + "," +
                                                Utils.vecNameToString("leftHandPosP1") + Utils.vecNameToString("leftHandRotP1") + "partId," + Utils.vecNameToString("objInteractedPosP1") + "," + Utils.vecNameToString("objInteractedRot") + "\n";

        headerStrTaskLog[1] = "userId,currentTask,dominantPlayer," + Utils.vecNameToString("centerBoundsPosP2") + "," + Utils.vecNameToString("centerAreaRotP2") + "," +
                                               Utils.vecNameToString("headP2Pos") + "," + Utils.vecNameToString("headP2Rot") + "," + Utils.vecNameToString("rightHandP2Pos") + Utils.vecNameToString("rightHandP2Rot") + "," +
                                               Utils.vecNameToString("leftHandP2Pos") + Utils.vecNameToString("leftHandP2Rot") + "partId," + Utils.vecNameToString("objInteractedPos") + "," + Utils.vecNameToString("objInteractedRot") + "\n";


    }

    public void setPlayer1(GameObject headP1, GameObject rightHandP1, GameObject leftHandP1)
    {
        headPlayer1 = headP1;
        rightHandPlayer1 = rightHandP1;
        leftHandPlayer1 = leftHandP1;
    }

    public void  setPlayer2(GameObject headP2, GameObject rightHandP2, GameObject leftHandP2)
    {
        headPlayer2 = headP2;
        rightHandPlayer2 = rightHandP2;
        leftHandPlayer2 = leftHandP2;
    }

    public GameObject getInteractPart(string player)
    {
        return objP1;
    }

    [Photon.Pun.PunRPC]
    public void objectInteractedByP2(bool interactionHappening)
    {
        if (interactionHappening)
        {
            if (!objInteractedP2)
            {
                if (currentTaskLog != null)
                {
                    objP2InteractedInitTIme = Time.realtimeSinceStartup;
                    objInteractedP2 = true;
                }
            }
            else
            {
                if (currentTaskLog != null)
                {
                    float delta = Time.realtimeSinceStartup - objP2InteractedInitTIme;
                    currentTaskLog.addTimeInteracting("P2", delta);
                    objP2InteractedInitTIme = Time.realtimeSinceStartup;
                }
            }
        }
        else
        {
            objP2InteractedInitTIme = 0;
            objInteractedP2 = false;
        }
        
    }

    [Photon.Pun.PunRPC]
    void setDominantPlayer(string dominantStr)
    {
        if (!isRemotePlayer)
            return;
        dominantplayer = dominantStr;
    }

    [Photon.Pun.PunRPC]
    void updateTimer(float time)
    {
        if (!isRemotePlayer)
            return;
        try
        {


            int timeRemainingInt = (int) time;
            int seconds = timeRemainingInt % 60;
            int minutes = (int)timeRemainingInt / 60;
            print("coming from rpc");
            if (minutes == 0)
            {
                timerText.color = Color.yellow;
            }
            else
            {
                timerText.color = Color.red;
            }

            if (seconds == 0)
            {
                timerText.text = minutes + ":" + seconds + "0";
            }
            else
            {
                timerText.text = minutes + ":" + seconds;
            }

            if (dominantplayer == "P1")
            {
                dominantplayerLabel.text = "not my turn";
                dominantplayerLabel.color = Color.red;
                //add a label saying not my turn
            }
            else
            {
                dominantplayerLabel.text = "my turn";
                dominantplayerLabel.color = Color.green;
                //add a label saying my turn
            }
        }
        catch (Exception ex)
        {
            print("exception " + isRemotePlayer + " e" + time);
        }
        
    }

    public void objectInteractedByP1(bool interactionHappening)
    {
        if (interactionHappening)
        {
            if (!objInteractedP1)
            {
                if (currentTaskLog != null)
                {
                    objP2InteractedInitTIme = Time.realtimeSinceStartup;
                    objInteractedP1 = true;
                }
            }
            else
            {
                if (currentTaskLog != null)
                {
                    float delta = Time.realtimeSinceStartup - objP1InteractedInitTIme;
                    currentTaskLog.addTimeInteracting("P1", delta);
                    objP1InteractedInitTIme = Time.realtimeSinceStartup;
                }
            }
        }
        else
        {
            objP1InteractedInitTIme = 0;
            objInteractedP1 = false;
        }

    }


    void initTask()
    {
        currentTask = 0;

        dominantplayer = "P1";

        //ShufflePossiblePositionsArray();
        //initializePartsForTask();

        if (Player1Area)
        {
            WireareaDrawer drawer =  Player1Area.GetComponentInChildren<WireareaDrawer>();
            if (drawer) { 
                drawer.BoundsSize1 = boundsSize;
            }
           // drawer.
        }

        if (Player2Area)
        {
            WireareaDrawer drawer = Player2Area.GetComponentInChildren<WireareaDrawer>();
            if(drawer) drawer.BoundsSize1 = boundsSize;
        }
        currentTaskState = TaskState.Player1Dominant;

        currentTaskLog = new TaskLog((groupId*2)-1, currentTask, "P1", currentTask.ToString(), Player1Area.transform, collabType, boundsSize);
        blueprintObjects = generator.generateBlueprint(new Vector3(0, 0, 0), 6, 4, 3, 0.09f, transformRootForP1Blueprint);
        listPossiblePositionsForPuzzle = generator.generatePuzzle(true, Player1Area);
        taskStarted = true;
    }

    void logUsersMovementsSplitByUser()
    {
        if(currentTaskState > TaskState.Connected)
        {
            string violatingP1 = "NoViolationP1";
            if (currentTaskLog.startTimeOutsideBoundsP1 > 0)
                violatingP1 = "ViolationP1";
            string violatingP2 = "NoViolationP2";
            if (currentTaskLog.startTimeOutsideBoundsP2 > 0)
            {
                violatingP2 = "ViolationP2";
            }
        }
    }
    
     bool isViolatingP1()
    {
        if(listActiveViolations.ContainsKey(Bodypart.HeadP1) || listActiveViolations.ContainsKey(Bodypart.rightHandP1) || listActiveViolations.ContainsKey(Bodypart.leftHandP1))
        {
            return true;
        }
        return false;
    }

    bool isViolatingP2()
    {
        if (listActiveViolations.ContainsKey(Bodypart.HeadP2) || listActiveViolations.ContainsKey(Bodypart.rightHandP2) || listActiveViolations.ContainsKey(Bodypart.leftHandP2))
        {
            return true;
        }
        return false;
    }
    void logUsersMovements()
    {
        if (currentTaskState > TaskState.BothConnected && !debug && !training)
        {
            string violatingP1 = "NoViolationP1";
            if (isViolatingP1())
                violatingP1 = "ViolationP1";
            string violatingP2 = "NoViolationP2";
            if (isViolatingP2())
            {
                violatingP2 = "ViolationP2";
            }
            
            player1InteractionStr += ((groupId * 2)-1) + "," + dominantplayer + "," + collabType.ToString() + "," + Time.realtimeSinceStartup + "," + currentTask + "," + (dominantplayer == "P2" ? true : false) + "," + violatingP2 + "," + Utils.vector3ToString(Player1Area.transform.position) + "," + Utils.vector3ToString(Player1Area.transform.eulerAngles) + ","
                                    + Utils.vector3ToString(headPlayer1.transform.position) + "," + Utils.vector3ToString(headPlayer1.transform.eulerAngles) + ","+ Utils.vector3ToString(rightHandPlayer1.transform.position) + "," + Utils.vector3ToString(rightHandPlayer1.transform.eulerAngles) +
                                     Utils.vector3ToString(leftHandPlayer1.transform.position) + "," + Utils.vector3ToString(leftHandPlayer1.transform.eulerAngles);
            if (player1Interacting && getInteractPart("P1"))
            {
                player1InteractionStr += "," + Utils.vector3ToString(getInteractPart("P1").transform.position) + "," + Utils.vector3ToString(getInteractPart("P1").transform.eulerAngles) + "\n";
               
            }
            else
            {
                player1InteractionStr += "\n";
            }

            if (player1InteractionStr.Length > 400)
            {
                //flush to file
                System.IO.File.AppendAllText(pathDirectory + "MovementReportP1_" + collabType.ToString() + ".csv", player1InteractionStr);

                //   System.IO.File.AppendAllText(pathDirectory + "handMovement_" + retargettingStr + "_" + surfaceOrientation.ToString() + ".csv", handMovementLogStr);
                player1InteractionStr = "";
            }



            string header2q = "userId,dominantPlayer,timestamp,collabType,currentTask,dominantPlayer,isViolatingBoundary," + Utils.vecNameToString("Player2AreaCenter") + "," + Utils.vecNameToString("Player2AreaRot")
                                + Utils.vecNameToString("headPosP2") + "," + Utils.vecNameToString("headRotP2") + "," + Utils.vecNameToString("rightHandPosP2") + Utils.vecNameToString("rightHandRotP2")
                                 + Utils.vecNameToString("leftHandPosP2") + "," + Utils.vecNameToString("leftHandRotP2") ;


            player2InteractionStr += (groupId*2) + ","+dominantplayer+","+ collabType.ToString() + "," + Time.realtimeSinceStartup + "," + currentTask + "," + (dominantplayer == "P2" ? true : false) + "," + violatingP2 + ","+ Utils.vector3ToString(Player2Area.transform.position) + "," + Utils.vector3ToString(Player2Area.transform.eulerAngles) + ","
                                    + Utils.vector3ToString(headPlayer2.transform.position) + "," + Utils.vector3ToString(headPlayer2.transform.eulerAngles) + ","+Utils.vector3ToString(rightHandPlayer2.transform.position) + "," + Utils.vector3ToString(rightHandPlayer2.transform.eulerAngles) +
                                     Utils.vector3ToString(leftHandPlayer2.transform.position) + "," + Utils.vector3ToString(leftHandPlayer2.transform.eulerAngles);
            
            if (player2Interacting && getInteractPart("P2"))
            {
                player2InteractionStr += "," + Utils.vector3ToString(getInteractPart("P2").transform.position) + "," + Utils.vector3ToString(getInteractPart("P2").transform.eulerAngles) + "\n";
            }
            else
            {
                player2InteractionStr += "\n";
            }


            if (player2InteractionStr.Length > 400)
            {
                //flush to file
                System.IO.File.AppendAllText(pathDirectory + "MovementReportP2_" + collabType.ToString() + ".csv", player2InteractionStr);

                player2InteractionStr = "";
            }



        }
    }

    private void Awake()
    {
        int globalCollabType = GlobalVariables.Get<int>("collabType");
        int id = GlobalVariables.Get<int>("userId");
        int avatar = GlobalVariables.Get<int>("avatarId");
        int remote = GlobalVariables.Get<int>("remote");

        bool cameFromVideoScene = false;
        cameFromVideoScene = GlobalVariables.Get<bool>("cameFromVideoScene");
        if (cameFromVideoScene)
        {
            groupId = id;
            collabType = (CollabType)globalCollabType;
            avatarId = avatar;
            if(remote == 1)
            {
                isRemotePlayer = true;
            }
            else
            {
                isRemotePlayer = false;
            }
        }
        else
        {
            GlobalVariables.Set("CollabType", (int)collabType);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        dominantplayer = "P1";
        //timeRemaining = totalTimePerTask;
        timeRemaining = timeTraining;
        if (Application.isEditor)
        {
            print("is editor");
        }
        else
        {
            print("isNotEditor");
        }
        if (isRemotePlayer)
            return;

        PathDirectory = Directory.GetCurrentDirectory() + "\\LogFiles";

        int i = 0;
        while (Directory.Exists(PathDirectory + "\\user" + groupId + "_" + i + "\\"))
        {
            i++;
        }

        PathDirectory += "\\user" + groupId + "_" + i;// + travelTechnique.ToString();// + "/";

        PathDirectory += "\\";
        if (!debug) { 
            if (!Directory.Exists(PathDirectory))
            {
                System.IO.Directory.CreateDirectory(PathDirectory);
            }
        }
        logTasks = new List<TaskLog>();
        initializeTask();
        //se nao houver diretorios
    }

    // Update is called once per frame
    void Update()
    {
        if (currentTask == 0 || currentTask == 1)
            training = true;
        else
            training = false;

        string strTraining = training  ? "training" : "real deal"; 
        debugTextLabel.text = "USER ID = " + groupId + " current task " + currentTask  + " "+ " Time Remaining "+ timeRemaining + " Training "+ strTraining + " Current State" + currentTaskState.ToString() + " Collab Type " + collabType.ToString();

        if(isRemotePlayer && collabType == CollabType.CoupledView && !hiddenAvatar)
        {
            GameObject spawnPointRemotePlayer = GameObject.Find("RemoteAvatar");
            if (spawnPointRemotePlayer)
            {
                Chiligames.MetaAvatarsPun.PunAvatarEntity punAvatarEntity = spawnPointRemotePlayer.GetComponent<Chiligames.MetaAvatarsPun.PunAvatarEntity>();
                if (punAvatarEntity && collabType == CollabType.CoupledView)
                {
                    punAvatarEntity.disableAvatar();
                    print("disabling remote avatar");
                    hiddenAvatar = true;
                }
            }
        }
        if(isRemotePlayer)
        {
            return;
        }

        if(currentTaskState == TaskState.Connected)
        {
            if (GameObject.Find("RemoteAvatar"))
            {
                print("RemoteAvatar");
            }
            if (Player2Area)
            {
                GameObject spawnPointRemotePlayer = GameObject.Find("RemoteAvatar");
                if (spawnPointRemotePlayer)
                {
                    playerNumber = 2;
                    setReference(spawnPointRemotePlayer);
                        print("COME OVER HERE");
                        currentTaskState = TaskState.BothConnected;
                    Chiligames.MetaAvatarsPun.PunAvatarEntity punAvatarEntity = spawnPointRemotePlayer.GetComponent<Chiligames.MetaAvatarsPun.PunAvatarEntity>();
                    if (punAvatarEntity && collabType == CollabType.CoupledView)
                    {
                        punAvatarEntity.disableAvatar(); 
                        print("disabling remote avatar");
                    }
                }
            }
            
        }
        //if(Player2Area)
        //calculateAngle();

        if(Input.GetKeyDown(KeyCode.S) && !taskStarted)
        {
            initTask();
        }
        if(isRemotePlayer && taskStartedP2 && timeRemaining > 0)
        {
            int timeRemainingInt = (int)timeRemaining;
            int seconds = timeRemainingInt % 60;
            int minutes = (int)timeRemainingInt / 60;
            if (minutes == 0)
            {
                timerText.color = Color.yellow;
            }
            else
            {
                timerText.color = Color.red;
            }

            if (seconds == 0)
            {
                timerText.text = minutes + ":" + seconds + "0";
            }
            else
            {
                timerText.text = minutes + ":" + seconds;
            }
            if (dominantplayer == "P1")
            {
                dominantplayerLabel.text = "not my turn";
                dominantplayerLabel.color = Color.green;
            }
            else if (dominantplayer == "P2")
            {
                dominantplayerLabel.text = "my turn";
                dominantplayerLabel.color = Color.red;
            }
            else
            {
                dominantplayerLabel.text = "not my turn";
                dominantplayerLabel.color = Color.red;
            }
        }
        if (timeRemaining > 0 && taskStarted && !isRemotePlayer)
        {
            if (currentTaskState == TaskState.EndTask)
            {
                timeRemaining = totalTimePerTask;
            }
            else
            {
                timeRemaining -= Time.deltaTime;
            }
            //training stuff
            

            int timeRemainingInt = (int)timeRemaining;
            int seconds = timeRemainingInt % 60;
            int minutes = (int) timeRemainingInt / 60;
            if (minutes == 0)
            {
                timerText.color = Color.yellow;
            }
            else
            {
                timerText.color = Color.red;
            }

            if(seconds == 0)
            {
                timerText.text = minutes + ":" + seconds+"0";
            }
            else
            {
                timerText.text = minutes + ":" + seconds;
            }
            if(dominantplayer == "P1")
            {
                dominantplayerLabel.text = "my turn";
                dominantplayerLabel.color = Color.green;
            }
            else if(dominantplayer == "P2")
            {
                dominantplayerLabel.text = "not my turn";
                dominantplayerLabel.color = Color.red;
            }
            else if(currentTaskState == TaskState.EndTask)
            {
                dominantplayerLabel.text = "GAME OVER";
                dominantplayerLabel.color = Color.red;
            }
            else
            {
                dominantplayerLabel.text = "not my turn";
                dominantplayerLabel.color = Color.red;
            }

        }
        else if(timeRemaining <= 0 && taskStarted)
        {
            nextPuzzle();
        }
        
        gameObject.GetComponent<Photon.Pun.PhotonView>().RPC("setDominantPlayer", Photon.Pun.RpcTarget.AllBuffered, dominantplayer);
        gameObject.GetComponent<Photon.Pun.PhotonView>().RPC("updateTimer", Photon.Pun.RpcTarget.AllBuffered, timeRemaining);
            
        
        bool nextTaskButtonRightController = OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.RHand);
        bool nextTaskButtonLeftController = OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.LHand);
        bool nextTaskButtonKeyboard = Input.GetKeyDown(KeyCode.N);

        if (nextTaskButtonRightController || nextTaskButtonLeftController || nextTaskButtonKeyboard)
        {
            if (!taskStarted)
            {
                initTask();
            }
            else
            {
                nextPuzzle();
            }
            
        }


        if(currentTaskState > TaskState.BothConnected && currentTaskState < TaskState.EndTask)
        {
            calculateBoundaryViolation();
        }

        if(!debug && currentTaskState > TaskState.BothConnected && currentTaskState < TaskState.EndTask)
        {
            logUsersMovements();
        }
    }

    public void incrementTimeOutsideBounds(float time)
    {
        if (currentTaskLog!=null)
        {
            currentTaskLog.incrementTimeOutsideBoundsP1(time);
        }
    }

    public void incrementBoundaryViolations(string player)
    {
        if (currentTaskLog != null)
        {
            if (player == "P1")
                currentTaskLog.incrementBoundViolationsP1();
            else
                currentTaskLog.incrementBoundViolationsP2();
        }
    }
    //not in use
    void generateParts()
    {
        rootForObjects = new GameObject("rootForObjects");
        if (rootPossiblePositionsForPuzzle)
        {
            rootForObjects.transform.parent = rootPossiblePositionsForPuzzle.transform;
            rootForObjects.transform.localPosition = Vector3.zero;
               // obj.transform.position = rootForObjects.transform.position;

            puzzleObjects = new List<GameObject>();
            for (int i = 0; i < rootForObjects.transform.childCount; i++)
            {
                blueprintObjects.Add(rootForObjects.transform.GetChild(i).gameObject);

                Material mat = rootForObjects.transform.GetChild(i).gameObject.GetComponent<MeshRenderer>().material;
                mat.color = new Color(1, 1, 1, 0.3f);

                GameObject duplicate = GameObject.Instantiate(rootForObjects.transform.GetChild(i).gameObject);

                mat = duplicate.GetComponent<MeshRenderer>().material;
                mat.color = new Color(1, 1, 1, 1.0f);
                duplicate.transform.parent = rootForObjects.transform;
                puzzleObjects.Add(duplicate);

                //mat.shader.
            }
        }
    }


    void finishActiveBoundaryViolations()
    {
        for(int i = 0; i < 6; i++)
        {
            Bodypart activeCol = (Bodypart)i;
            if (listActiveViolations.ContainsKey(activeCol))
            {
                
                BoundaryViolation finished = listActiveViolations[activeCol];// new BoundaryViolation()
                listActiveViolations.Remove((Bodypart)i);
                finished.timestampEnd = Time.realtimeSinceStartup;
                if(finished.userId % 2 == 0)
                {
                    currentTaskLog.incrementTimeOutsideBoundsP2(finished.timestampEnd - finished.timestampInit);
                }
                else
                {
                    currentTaskLog.incrementTimeOutsideBoundsP1(finished.timestampEnd - finished.timestampInit);
                }
                finishedViolations.Add(finished);
            }
        }
    }
    
    public void drawBoundaryViolationP2(GameObject headPlayer2, GameObject rightHandPlayer2, GameObject leftHandPlayer2)
    {
        if(headPlayer2 && rightHandPlayer2 && leftHandPlayer2)
        {
            Vector3 headP2Local = Player2Area.transform.InverseTransformPoint(headPlayer2.transform.position);
            Vector3 rightHandP2Local = Player2Area.transform.InverseTransformPoint(rightHandPlayer2.transform.position);
            Vector3 leftHandP2Local = Player2Area.transform.InverseTransformPoint(leftHandPlayer2.transform.position);

            headP2Local = new Vector3(Mathf.Abs(headP2Local.x), Mathf.Abs(headP2Local.y), Mathf.Abs(headP2Local.z));
            rightHandP2Local = new Vector3(Mathf.Abs(rightHandP2Local.x), Mathf.Abs(rightHandP2Local.y), Mathf.Abs(rightHandP2Local.z));
            leftHandP2Local = new Vector3(Mathf.Abs(leftHandP2Local.x), Mathf.Abs(leftHandP2Local.y), Mathf.Abs(leftHandP2Local.z));

            float delta = Time.deltaTime;
            bool drawboundaryP2 = false;

            if(Utils.IsViolatingBoundary(headP2Local, boundsSize))
            {
                drawboundaryP2 = true;
            }
            else
            {

            }

            if(Utils.IsViolatingBoundary(rightHandP2Local, boundsSize))
            {
                drawboundaryP2 = true;
            }

            if(Utils.IsViolatingBoundary(leftHandP2Local, boundsSize))
            {
                drawboundaryP2 = true;
            }

            if (drawboundaryP2)
            {
                boundaryDrawerP2 = Player2Area.GetComponent<WireareaDrawer>();
                if (boundaryDrawerP2)
                    boundaryDrawerP2.drawBoundary(boundaryDrawerP2);
            }
        }
    }
    
    Vector3 calculateBoundaryViolation()
    {
        Vector3 violation = new Vector3();
        if(currentTaskLog == null)
        {
            currentTaskLog = new TaskLog(groupId*2, currentTask, "P1", "test", Player1Area.transform, collabType, Player1Area.transform.localScale);
        }

        if (headPlayer1 && rightHandPlayer1 && leftHandPlayer1)
        {
            //we dont care about the Y
            Vector3 headP1Local = Player1Area.transform.InverseTransformPoint(headPlayer1.transform.position);
            Vector3 rightHandP1Local = Player1Area.transform.InverseTransformPoint(rightHandPlayer1.transform.position);
            Vector3 leftHandP1Local = Player1Area.transform.InverseTransformPoint(leftHandPlayer1.transform.position);

            headP1Local = new Vector3(Mathf.Abs(headP1Local.x), Mathf.Abs(headP1Local.y), Mathf.Abs(headP1Local.z));
            rightHandP1Local = new Vector3(Mathf.Abs(rightHandP1Local.x), Mathf.Abs(rightHandP1Local.y), Mathf.Abs(rightHandP1Local.z));
            leftHandP1Local = new Vector3(Mathf.Abs(leftHandP1Local.x), Mathf.Abs(leftHandP1Local.y), Mathf.Abs(leftHandP1Local.z));
            //print("head = " + headP1Local.ToString() + " rightHand" + rightHandP1Local.ToString() + " leftHand " + leftHandP1Local.ToString() + " bounds" + boundsSize.ToString() );//
            float delta = Time.deltaTime;
            if (!outsideBoundsLastFrameP1)
            {
                
            }

            bool drawBoundaryP1 = false;

            Bodypart activeBodyPart = Bodypart.HeadP1;
            if(headP1Local.x > boundsSize.x/2.0f || headP1Local.y > boundsSize.y / 2.0f || headP1Local.z > boundsSize.z / 2.0f)
            {
                float distance = Vector3.Distance(Vector3.zero, headP1Local);
                if (!listActiveViolations.ContainsKey(activeBodyPart))
                {
                    listActiveViolations[activeBodyPart] = new BoundaryViolation((groupId *2)-1, currentTask, collabType, activeBodyPart, Time.realtimeSinceStartup,distance);
                    currentTaskLog.incrementViolationNumber((int)activeBodyPart);
                    currentTaskLog.incrementBoundViolationsP1();
                }
                else
                {
                    if(distance > listActiveViolations[activeBodyPart].distance)
                    {
                        listActiveViolations[activeBodyPart].distance = distance;
                    }
                }
                boundaryDrawerP1.drawBoundary(true);
                //currentTaskLog.incrementTimeOutsideBounds(Time.deltaTime);
                //
            }
            else if(headP1Local.x < boundsSize.x / 2.0f || headP1Local.y < boundsSize.y / 2.0f || headP1Local.z < boundsSize.z / 2.0f)
            {
                if (listActiveViolations.ContainsKey(activeBodyPart))
                {
                    BoundaryViolation finished = listActiveViolations[activeBodyPart];// new BoundaryViolation()
                    listActiveViolations.Remove(activeBodyPart);
                    finished.timestampEnd = Time.realtimeSinceStartup;
                    currentTaskLog.incrementTimeOutsideBoundsP1(finished.timestampEnd - finished.timestampInit);
                    finishedViolations.Add(finished);
                }
                else
                {
                    //do nothing
                }
                boundaryDrawerP1.drawBoundary(false);
            }

            activeBodyPart = Bodypart.rightHandP1;
            if (rightHandP1Local.x > boundsSize.x / 2.0f || rightHandP1Local.y > boundsSize.y / 2.0f || rightHandP1Local.z > boundsSize.z / 2.0f)
            {
                float distance = Vector3.Distance(Vector3.zero, rightHandP1Local);
                if (!listActiveViolations.ContainsKey(activeBodyPart))
                {
                    listActiveViolations[activeBodyPart] = new BoundaryViolation((groupId * 2) - 1, currentTask, collabType, activeBodyPart, Time.realtimeSinceStartup, distance);
                    //listActiveViolations[activeBodyPart] = new BoundaryViolation(activeBodyPart, Time.realtimeSinceStartup, distance);
                    currentTaskLog.incrementViolationNumber((int)activeBodyPart);
                    currentTaskLog.incrementBoundViolationsP1();

                }
                else
                {
                    if (distance > listActiveViolations[activeBodyPart].distance)
                    {
                        listActiveViolations[activeBodyPart].distance = distance;
                    }
                }
                drawBoundaryP1 = true;
                //boundaryDrawerP1.drawBoundary(true);

                
                //currentTaskLog.incrementTimeOutsideBounds(Time.deltaTime);
                //
            }
            else if (rightHandP1Local.x < boundsSize.x / 2.0f || rightHandP1Local.y < boundsSize.y / 2.0f || rightHandP1Local.z < boundsSize.z / 2.0f)
            {
                if (listActiveViolations.ContainsKey(activeBodyPart))
                {
                    BoundaryViolation finished = listActiveViolations[activeBodyPart];// new BoundaryViolation()
                    listActiveViolations.Remove(activeBodyPart);
                    finished.timestampEnd = Time.realtimeSinceStartup;
                    currentTaskLog.incrementTimeOutsideBoundsP1(finished.timestampEnd - finished.timestampInit);
                    finishedViolations.Add(finished);
                }
                else
                {
                    //do nothing
                }
                //boundaryDrawerP1.drawBoundary(false);
            }
            
            activeBodyPart = Bodypart.leftHandP1;
            if (leftHandP1Local.x > boundsSize.x / 2.0f || leftHandP1Local.y > boundsSize.y / 2.0f || leftHandP1Local.z > boundsSize.z / 2.0f)
            {
                float distance = Vector3.Distance(Vector3.zero, leftHandP1Local);
                if (!listActiveViolations.ContainsKey(activeBodyPart))
                {
                    listActiveViolations[activeBodyPart] = new BoundaryViolation((groupId * 2) - 1, currentTask, collabType, activeBodyPart, Time.realtimeSinceStartup, distance);
                    //listActiveViolations[activeBodyPart] = new BoundaryViolation(activeBodyPart, Time.realtimeSinceStartup, distance);
                    currentTaskLog.incrementViolationNumber((int)activeBodyPart);
                    currentTaskLog.incrementBoundViolationsP1();
                }
                else
                {
                    if (distance > listActiveViolations[activeBodyPart].distance)
                    {
                        listActiveViolations[activeBodyPart].distance = distance;
                    }
                }
                drawBoundaryP1 = true;
                //boundaryDrawerP1.drawBoundary(true);
                //currentTaskLog.incrementTimeOutsideBounds(Time.deltaTime);
                //
            }
            else if (leftHandP1Local.x < boundsSize.x / 2.0f || leftHandP1Local.y < boundsSize.y / 2.0f || leftHandP1Local.z < boundsSize.z / 2.0f)
            {
                if (listActiveViolations.ContainsKey(activeBodyPart))
                {
                    BoundaryViolation finished = listActiveViolations[activeBodyPart];// new BoundaryViolation()
                    listActiveViolations.Remove(activeBodyPart);
                    finished.timestampEnd = Time.realtimeSinceStartup;
                    currentTaskLog.incrementTimeOutsideBoundsP1(finished.timestampEnd - finished.timestampInit);


                    finishedViolations.Add(finished);
                }
                else
                {
                    //do nothing
                }
                //boundaryDrawerP1.drawBoundary(false);
            }

            boundaryDrawerP1.drawBoundary(drawBoundaryP1);
        }

        if(headPlayer2 && rightHandPlayer2 && leftHandPlayer2)
        {
            //we dont care about the Y
            Vector3 headP2Local = Player2Area.transform.InverseTransformPoint(headPlayer2.transform.position);
            Vector3 rightHandP2Local = Player2Area.transform.InverseTransformPoint(rightHandPlayer2.transform.position);
            Vector3 leftHandP2Local = Player2Area.transform.InverseTransformPoint(leftHandPlayer2.transform.position);

            headP2Local = new Vector3(Mathf.Abs(headP2Local.x), Mathf.Abs(headP2Local.y), Mathf.Abs(headP2Local.z));
            rightHandP2Local = new Vector3(Mathf.Abs(rightHandP2Local.x), Mathf.Abs(rightHandP2Local.y), Mathf.Abs(rightHandP2Local.z));
            leftHandP2Local = new Vector3(Mathf.Abs(leftHandP2Local.x), Mathf.Abs(leftHandP2Local.y), Mathf.Abs(leftHandP2Local.z));

            float delta = Time.deltaTime;
            bool drawboundaryP2 = false;

            Bodypart activeBodyPart = Bodypart.HeadP2;
            if (headP2Local.x > boundsSize.x / 2.0f || headP2Local.y > boundsSize.y / 2.0f || headP2Local.z > boundsSize.z / 2.0f)
            {
                float distance = Vector3.Distance(Vector3.zero, headP2Local);
                if (!listActiveViolations.ContainsKey(activeBodyPart))
                {
                    listActiveViolations[activeBodyPart] = new BoundaryViolation((groupId * 2) , currentTask, collabType, activeBodyPart, Time.realtimeSinceStartup, distance);
                    //listActiveViolations[activeBodyPart] = new BoundaryViolation(activeBodyPart, Time.realtimeSinceStartup, distance);
                    currentTaskLog.incrementViolationNumber((int)activeBodyPart);
                    currentTaskLog.incrementBoundViolationsP2();
                }
                else
                {
                    if (distance > listActiveViolations[activeBodyPart].distance)
                    {
                        listActiveViolations[activeBodyPart].distance = distance;
                    }
                }
                drawboundaryP2 = true;
                // boundaryDrawerP2.drawBoundary(true);
                //Player2Area.GetComponent<Photon.Pun.PhotonView>().RPC("drawBoundary", Photon.Pun.RpcTarget.AllBuffered, true);
                //currentTaskLog.incrementTimeOutsideBounds(Time.deltaTime);
                //
            }
            else if (headP2Local.x < boundsSize.x / 2.0f || headP2Local.y < boundsSize.y / 2.0f || headP2Local.z < boundsSize.z / 2.0f)
            {
                if (listActiveViolations.ContainsKey(activeBodyPart))
                {
                    BoundaryViolation finished = listActiveViolations[activeBodyPart];// new BoundaryViolation()
                    listActiveViolations.Remove(activeBodyPart);
                    finished.timestampEnd = Time.realtimeSinceStartup;
                    currentTaskLog.incrementTimeOutsideBoundsP2(finished.timestampInit - finished.timestampEnd);
                    finishedViolations.Add(finished);
                }
                else
                {
                    //do nothing
                }
                //boundaryDrawerP2.drawBoundary(false);
                //Player2Area.GetComponent<Photon.Pun.PhotonView>().RPC("drawBoundary", Photon.Pun.RpcTarget.AllBuffered, false);
            }

            activeBodyPart = Bodypart.rightHandP2;
            if (rightHandP2Local.x > boundsSize.x / 2.0f || rightHandP2Local.y > boundsSize.y / 2.0f || rightHandP2Local.z > boundsSize.z / 2.0f)
            {
                float distance = Vector3.Distance(Vector3.zero, rightHandP2Local);
                if (!listActiveViolations.ContainsKey(activeBodyPart))
                {
                    listActiveViolations[activeBodyPart] = new BoundaryViolation((groupId * 2), currentTask,collabType, activeBodyPart, Time.realtimeSinceStartup, distance);
                    //listActiveViolations[activeBodyPart] = new BoundaryViolation(activeBodyPart, Time.realtimeSinceStartup, distance);
                    currentTaskLog.incrementViolationNumber((int)activeBodyPart);
                    currentTaskLog.incrementBoundViolationsP2();
                }
                else
                {
                    if (distance > listActiveViolations[activeBodyPart].distance)
                    {
                        listActiveViolations[activeBodyPart].distance = distance;
                    }
                }
                drawboundaryP2 = true;
                //boundaryDrawerP2.drawBoundary(true);
                //Player2Area.GetComponent<Photon.Pun.PhotonView>().RPC("drawBoundary", Photon.Pun.RpcTarget.AllBuffered, true);
                //currentTaskLog.incrementTimeOutsideBounds(Time.deltaTime);
                //
            }
            else if (rightHandP2Local.x < boundsSize.x / 2.0f || rightHandP2Local.y < boundsSize.y / 2.0f || rightHandP2Local.z < boundsSize.z / 2.0f)
            {
                if (listActiveViolations.ContainsKey(activeBodyPart))
                {
                    BoundaryViolation finished = listActiveViolations[activeBodyPart];// new BoundaryViolation()
                    listActiveViolations.Remove(activeBodyPart);
                    finished.timestampEnd = Time.realtimeSinceStartup;
                    currentTaskLog.incrementTimeOutsideBoundsP2(finished.timestampInit - finished.timestampEnd);
                    finishedViolations.Add(finished);
                }
                else
                {
                    //do nothing
                }
                //boundaryDrawerP2.drawBoundary(false);
                //Player2Area.GetComponent<Photon.Pun.PhotonView>().RPC("drawBoundary", Photon.Pun.RpcTarget.AllBuffered, false);
            }

            activeBodyPart = Bodypart.leftHandP2;
            if (leftHandP2Local.x > boundsSize.x / 2.0f || leftHandP2Local.y > boundsSize.y / 2.0f || leftHandP2Local.z > boundsSize.z / 2.0f)
            {
                float distance = Vector3.Distance(Vector3.zero, leftHandP2Local);
                if (!listActiveViolations.ContainsKey(activeBodyPart))
                {
                    listActiveViolations[activeBodyPart] = new BoundaryViolation((groupId * 2), currentTask, collabType, activeBodyPart, Time.realtimeSinceStartup, distance);
                    //listActiveViolations[activeBodyPart] = new BoundaryViolation(activeBodyPart, Time.realtimeSinceStartup, distance);
                    currentTaskLog.incrementViolationNumber((int)activeBodyPart);
                    currentTaskLog.incrementBoundViolationsP2();
                }
                else
                {
                    if (distance > listActiveViolations[activeBodyPart].distance)
                    {
                        listActiveViolations[activeBodyPart].distance = distance;
                    }
                }
                drawboundaryP2 = true;
                //boundaryDrawerP2.drawBoundary(true);
                //Player2Area.GetComponent<Photon.Pun.PhotonView>().RPC("drawBoundary", Photon.Pun.RpcTarget.AllBuffered, true);
                //currentTaskLog.incrementTimeOutsideBounds(Time.deltaTime);
                //
            }
            else if (leftHandP2Local.x < boundsSize.x / 2.0f || leftHandP2Local.y < boundsSize.y / 2.0f || leftHandP2Local.z < boundsSize.z / 2.0f)
            {
                if (listActiveViolations.ContainsKey(activeBodyPart))
                {
                    BoundaryViolation finished = listActiveViolations[activeBodyPart];// new BoundaryViolation()
                    listActiveViolations.Remove(activeBodyPart);
                    finished.timestampEnd = Time.realtimeSinceStartup;
                    currentTaskLog.incrementTimeOutsideBoundsP2(finished.timestampInit - finished.timestampEnd);
                    finishedViolations.Add(finished);
                }
                else
                {
                    //do nothing
                }
                //boundaryDrawerP2.drawBoundary(false);
                //Player2Area.GetComponent<Photon.Pun.PhotonView>().RPC("drawBoundary", Photon.Pun.RpcTarget.AllBuffered, false);
            }

            //boundaryDrawerP2.drawBoundary(drawboundaryP2);
            //Player2Area.GetComponent<Photon.Pun.PhotonView>().RPC("drawBoundary", Photon.Pun.RpcTarget.AllBuffered, drawboundaryP2);
        }
        return violation;
    }

    [Photon.Pun.PunRPC]
    public void nextPuzzleP2(int currentState)
    {
        TaskState tState = (TaskState)currentState;
            if (tState == TaskState.EndTask)
                dominantplayer = "Game OVER";
            else
                dominantplayer = dominantplayer == "P1 " ? "P2" : "P1"; 
    }

    void nextPuzzle()
    {

        string str = "";
        for(int i = 0;i < blueprintObjects.Count; i++)
        {
            GameObject obj = GameObject.Find(blueprintObjects[i].name+"_root(Clone)");//part2_root(Clone)
            if (obj)
            {
                float threshold = 0.3f;
                bool correctObject = false;
                float distanceBetweenBlueprintAndUserPlacedObject = Vector3.Distance(blueprintObjects[i].transform.position, obj.transform.position);
                if (distanceBetweenBlueprintAndUserPlacedObject > threshold)
                {
                    correctObjectsPerTask++;
                    correctObject = true;
                }
                Vector3 distanceByAxis = obj.transform.position - blueprintObjects[i].transform.position;
                logTaskAccuracy += groupId + "," + collabType + ","+ dominantplayer + ","+ currentTask + ","  + blueprintObjects[i].name + ","+ Utils.vector3ToString(blueprintObjects[i].transform.position) + "," 
                    +obj.name + Utils.vector3ToString(obj.transform.position) + ","+ obj.name + "," + Utils.vector3ToString(distanceByAxis) + "," + distanceBetweenBlueprintAndUserPlacedObject + ","+correctObject +"\n" ;
                
            }
        }

        currentTaskLog.endTask();
        logTasks.Add(currentTaskLog);
        finishActiveBoundaryViolations();

        currentTask++;
        if (currentTask == 0 || currentTask == 1)
            timeRemaining = timeTraining;
        else
            timeRemaining = totalTimePerTask;
        if (currentTask == totalNumberTasks)
        {
            currentTaskState = TaskState.EndTask;
            dominantplayer = "P3";
            for (int i = 0; i < blueprintObjects.Count; i++)
            {
                blueprintObjects[i].gameObject.SetActive(false);
            }
            for (int i = 0; i < listPossiblePositionsForPuzzle.Count; i++)
            {
                listPossiblePositionsForPuzzle[i].SetActive(false);
            }
            //finishActiveBoundaryViolations();

            return;
        }


        //
        GameObject dominantArea;
        GameObject dominantRootPuzzle;
        if(dominantplayer == "P2")
        {
            
            dominantplayer = "P1";
            currentTaskState = TaskState.Player1Dominant;
            dominantArea = Player1Area.gameObject;
            dominantRootPuzzle = transformRootForP1Blueprint;
            //hidecurrenttask
        }
        else
        {
           
            dominantplayer = "P2";
            currentTaskState = TaskState.Player2Dominant;
            dominantArea = Player2Area;
            dominantRootPuzzle = transformRootForP2Blueprint;
            //hidecurrenttask
        }


        blueprintObjects = generator.generateBlueprint(new Vector3(0, 0, 0), 6, 4, 3, 0.09f, dominantRootPuzzle);
        listPossiblePositionsForPuzzle = generator.generatePuzzle(false, dominantArea);
        int idToUse = 0;
        GameObject player;
        if(currentTaskState == TaskState.Player1Dominant)
        {
            idToUse = (groupId * 2) - 1;
            player = Player1Area;
        }
        else
        {
            idToUse = (groupId * 2);
            player = Player2Area;
        }
        
        currentTaskLog = new TaskLog(idToUse,currentTask, dominantplayer, currentTask.ToString(), player.transform, collabType, boundsSize);
        //gameObject.GetComponent<Photon.Pun.PhotonView>().RPC("nextPuzzleP2", Photon.Pun.RpcTarget.AllBuffered, (int)currentTaskState);
        //GetComponent<Photon.Pun.PhotonView>().RPC("nextPuzzleP2", Photon.Pun.RpcTarget.AllBuffered, );
    }

    public void skipTraining()
    {
        if (currentTask == 0 && currentTaskState >= TaskState.BothConnected)
        {
            nextPuzzle();
            nextPuzzle();
        }
        else if(currentTask == 1)
        {
            nextPuzzle();
        }
    }

    [Photon.Pun.PunRPC]
    public void initializeTaskP2(int number)
    {
        dominantplayer = "P1";
        taskStartedP2 = true;
        timeRemaining = totalTimePerTask;
    }

    void initializeTask()
    {

        if (Player1Area && Player2Area)
        {
            //this.gameObject.GetComponent<Photon.Pun.PhotonView>().RPC("initializeTaskP2", Photon.Pun.RpcTarget.AllBuffered, 1);

            if (collabType == CollabType.FacetoFaceIntersect)
            {
                Player1Area.transform.localPosition = new Vector3(0, 0, 0);
                Player1Area.transform.localEulerAngles = new Vector3(0, 0, 0);
                Player2Area.transform.localPosition = new Vector3(0, 0, 0.49f);
                Player2Area.transform.localEulerAngles = new Vector3(0, 180, 0);


            }
            else if (collabType == CollabType.FaceToFaceNoIntersect)
            {
                Player1Area.transform.localPosition = new Vector3(0, 0, 0);
                Player1Area.transform.localEulerAngles = new Vector3(0, 0, 0);
                Player2Area.transform.localPosition = new Vector3(0, 0, 0.814f);
                Player2Area.transform.localEulerAngles = new Vector3(0, 180, 0);




            }
            else if (collabType == CollabType.Angled45)
            {
                Player1Area.transform.localPosition = new Vector3(0, 0, 0);
                Player1Area.transform.localEulerAngles = new Vector3(0, 0, 0);
                Player2Area.transform.localPosition = new Vector3(0.363f, 0, 0.647f);
                Player2Area.transform.localEulerAngles = new Vector3(0, 125.0f, 0);
            }
            else if(collabType == CollabType.Angled90)
            {
                Player1Area.transform.localPosition = new Vector3(0, 0, 0);
                Player1Area.transform.localEulerAngles = new Vector3(0, 0, 0);
                Player2Area.transform.localPosition = new Vector3(0.172f, 0, 0.644f);
                Player2Area.transform.localEulerAngles = new Vector3(0, 90.0f, 0);
            }
            else if (collabType == CollabType.CoupledView)
            {
                Player1Area.transform.localPosition = new Vector3(0, 0, 0);
                Player1Area.transform.localEulerAngles = new Vector3(0, 0, 0);
                Player2Area.transform.localPosition = new Vector3(0, 0, 0);
                Player2Area.transform.localEulerAngles = new Vector3(0, 0, 0);
            }
            else if (collabType == CollabType.SideBySide)
            {
                Player1Area.transform.localPosition = new Vector3(0, 0, 0);
                Player1Area.transform.localEulerAngles = new Vector3(0, 0, 0);
                Player2Area.transform.localPosition = new Vector3(0.59f, 0, 0);
                Player2Area.transform.localEulerAngles = new Vector3(0, 0, 0);
            }

            /*GameObject goPlayer1 = new GameObject("Player1Head");
            goPlayer1.transform.parent = Player1Area.transform;
            goPlayer1.transform.localPosition = Vector3.zero;
            goPlayer1.transform.localPosition = new Vector3(-0.013f, 0.197f, 0.058f);

            headPlayer1 = goPlayer1;
            rightHandPlayer1 = new GameObject("rightHandP1");
            leftHandPlayer1 = new GameObject("leftHandP1");*/
            boundaryDrawerP1 = Player1Area.GetComponent<WireareaDrawer>();

            //headPosP1.transform.parent = Player1Area.transform;
            //rightHandPlayer1.transform.parent = Player1Area.transform;
            //leftHandPlayer1.transform.parent = Player1Area.transform;

            GameObject traytablePlayer1 = GameObject.Find("traytableP1");// new GameObject("TraytableP1");
            traytablePlayer1.transform.parent = Player1Area.transform;
            traytablePlayer1.transform.localPosition = new Vector3(0.003f, -0.137f, 0.243f);
            traytablePlayer1.transform.localScale = new Vector3(0.5137f, 0.019f, 0.29f);
            traytablePlayer1.name = "TraytableP1";
            //traytablePlayer1.AddComponent<Photon.Pun.PhotonView>();

            //Photon.Pun.PhotonTransformView pt =  traytablePlayer1.AddComponent<Photon.Pun.PhotonTransformView>();
            //pt.m_SynchronizeScale = true;
            //pt.m_UseLocal = false;
            GameObject rootObjForPuzzlesP1 = GameObject.Find("rootForObjsP1");
            rootObjForPuzzlesP1.transform.parent = Player1Area.transform;
            rootObjForPuzzlesP1.transform.localPosition = new Vector3(-0.243f, -0.095f, 0.104f);
            this.transformRootForP1Blueprint = rootObjForPuzzlesP1;

            /*GameObject goPlayer2 = new GameObject("Player2Head");
            goPlayer2.transform.parent = Player2Area.transform;
            goPlayer2.transform.localPosition = Vector3.zero;
            goPlayer2.transform.localPosition = new Vector3(-0.013f, 0.197f, 0.058f);
            goPlayer2.transform.localEulerAngles = Vector3.zero;

            headPlayer2 = goPlayer2;
            rightHandPlayer2 = new GameObject("rightHandP2");
            leftHandPlayer2 = new GameObject("leftHandP2");*/
            boundaryDrawerP2 = Player2Area.GetComponent<WireareaDrawer>();

            //headPosP1.transform.parent = Player1Area.transform;
            //rightHandPlayer2.transform.parent = Player2Area.transform;
            //leftHandPlayer2.transform.parent = Player2Area.transform;

            
            if (collabType == CollabType.FacetoFaceIntersect || collabType == CollabType.CoupledView)
            {
                GameObject traytablePlayer2 = GameObject.Find("traytableP2");// CreatePrimitive(PrimitiveType.Cube);
                traytablePlayer2.transform.name = "TraytableP2";
                traytablePlayer2.transform.parent = Player2Area.transform;
                traytablePlayer2.transform.localEulerAngles = Vector3.zero;
                traytablePlayer2.transform.localPosition = new Vector3(0.003f, -100.137f, 0.243f);
                traytablePlayer2.transform.localScale = new Vector3(0.5137f, 0.019f, 0.29f);
                traytablePlayer2.AddComponent<Photon.Pun.PhotonView>();
                traytablePlayer2.GetComponent<MeshRenderer>().enabled = false;
            }
            else 
            {
                GameObject traytablePlayer2 = GameObject.Find("traytableP2");// CreatePrimitive(PrimitiveType.Cube);
                traytablePlayer2.transform.name = "TraytableP2";
                traytablePlayer2.transform.parent = Player2Area.transform;
                traytablePlayer2.transform.localEulerAngles = Vector3.zero;
                traytablePlayer2.transform.localPosition = new Vector3(0.003f, -0.137f, 0.243f);
                traytablePlayer2.transform.localScale = new Vector3(0.5137f, 0.019f, 0.29f);
                traytablePlayer2.AddComponent<Photon.Pun.PhotonView>();
                //traytablePlayer2.GetComponent<MeshRenderer>().enabled = false;
                //Photon.Pun.PhotonTransformView pt2 = traytablePlayer2.AddComponent<Photon.Pun.PhotonTransformView>();
                //pt2.m_SynchronizeScale = true;
                //pt2.m_UseLocal = false;
            }
            GameObject rootObjForPuzzlesP2 = GameObject.Find("rootForObjsP2");
            rootObjForPuzzlesP2.transform.parent = Player2Area.transform;
            rootObjForPuzzlesP2.transform.localPosition = new Vector3(-0.243f, -0.095f, 0.104f);
            rootObjForPuzzlesP2.transform.localEulerAngles = Vector3.zero;
            this.transformRootForP2Blueprint = rootObjForPuzzlesP2;
        }
    }
    string calculateAngle(Vector3 angle)
    {
        string state = "Within Gaze Bounds";
        if(angle.y < 15.0f && angle.y > -15.0f)
        {
            state = "Within Gaze Bounds";
        }
        else if(angle.y < 45.0f && angle.y > -45.0f)
        {
            state =  "Comfortable but gaze violation";
        }
        else
        {
            state = "Extreme violation";
        }
        return state;
    }

    public void OnDisable()
    {
        if (!debug && !isRemotePlayer)
        {
            CompleteTaskReport();
            //CompleteHandPathReport();
        }

    }
    public void CompleteTaskReport()
    {
        //string header = "UserId,CurrentGainLevel,TargetSize,InitMovementTime,TargetPressedTime,ReachTime,TargetReleasedTime,numberErrorsFirstTarget,numberErrorsFinalTarget,SlidingTaskTime,TotalTime,ErrorFirstTime,ErrorSecondTime\n";
        string header = "userId,collabType,trialNumber,dominantPlayer,puzzleId," + Utils.vecNameToString("centerPosArea") + "," + Utils.vecNameToString("centerRotArea") + "," + collabType.ToString() +
            "," + Utils.vecNameToString("boundsSize") + "," + "numberOfBoundViolationsP1,timeOutsideBoundsP1,numberOfBoundViolationsP2,timeOutsideBoundsP2,totalTime,isTraining\n";
        string logTaskStr = header;
        //string passiveHapticsStr = (isPassiveHaptics ? "PassiveHaptics" : "NoPassiveHaptics");
        //string retargettingStr = retargettingOption.ToString();
        if (logTasks != null)
        {
             foreach (TaskLog tLog in logTasks)
             {
                 logTaskStr += tLog.toLogString();
             }
             if (!File.Exists(pathDirectory + "TaskReport_" + collabType.ToString() + ".csv"))
             {
                 System.IO.File.AppendAllText(pathDirectory + "TaskReport_" + collabType.ToString() + ".csv", logTaskStr);
                 /*System.IO.StreamWriter file = new System.IO.StreamWriter(pathDirectory + "TaskReport.csv");
                 file.WriteLine(logTaskStr);
                 file.Close();*/
              }
        else
            {
                //System.IO.File.AppendAllText(pathDirectory + "TaskReport_" + collabType.ToString() + ".csv", logTaskStr);

                int i = 0;
                while (File.Exists(pathDirectory + "TaskReport_" + collabType.ToString() + i + ".csv"))
                {
                    i++;
                }
                //logTaskStr += "#TaskTotalTime=" + (endTime - startTime);
                System.IO.File.WriteAllText(pathDirectory + "TaskReport_" + collabType.ToString() + i+".csv", logTaskStr);
                /*System.IO.StreamWriter file = new System.IO.StreamWriter(pathDirectory + "TaskReport_0.csv");
                file.Write(logTaskStr);
                file.Close();*/
            }

            System.IO.File.WriteAllText(pathDirectory + "TaskReportAccuracy_" + collabType.ToString() + ".csv", logTaskAccuracy);

            string logBoundaryViolations = "userId,collabType,currentTask,bPart,distance,timeElapsed\n";
            foreach(BoundaryViolation finishedCOllision in finishedViolations)
            {
                logBoundaryViolations += finishedCOllision.toLogString();
                    
            }
            System.IO.File.WriteAllText(pathDirectory + "TaskReportCollisions_" + collabType.ToString() + ".csv", logBoundaryViolations);
        }
    

            //System.IO.File.WriteAllText(pathDirectory + "test.txt", logTaskStr);
            //printToFile
        }
    
}
