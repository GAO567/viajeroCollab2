using System.Collections;
using System.Collections.Generic;

using System;
using UnityEngine;
using System.Linq;
using System.Security.Cryptography;

using OculusSampleFramework;
using System.IO;
using UnityEngine.UI;



public enum CollabType
{
    FacetoFaceIntersect, FaceToFaceNoIntersect, SideBySide, AngledFaceToFace, CoupledView
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
    public int userId = 0;
    public CollabType collabType = CollabType.FacetoFaceIntersect;
    public Vector3 boundsSize = new Vector3(0.6f,1.0f,0.8f);
    [SerializeField] bool debug = false;
    public  GameObject Player1Area;
    public  GameObject Player2Area;
    [SerializeField] GameObject userHead;
    [SerializeField] List<GameObject> puzzleObjects;
    [SerializeField] GameObject rootPossiblePositionsForPuzzle;
    [SerializeField] GameObject distractorsRoot;
    List<GameObject> listPossiblePositionsForPuzzle;
    List<GameObject> currentBlueprint;

    string pathDirectory = "";

    List<TaskLog> logTasks;
    List<GameObject> objectPartsForThisTask;
    TaskLog currentTaskLog;
    int currentTask = 0;

    List<float[]> conditionsByUserId;
    //List<TaskCondition[]> taskConditionsByUserId;
    public float angleBetween;
    private string dominantplayer;

    GameObject headPosP1;
    GameObject rightHandPosP1;
    GameObject leftHandPosP1;

    GameObject headPosP2;
    GameObject leftHandPosP2;
    GameObject rightHandPosP2;



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
    public string player1InteractionStr = "";
    private string player2InteractionStr;



    Dictionary<Bodypart, BoundaryViolation> listActiveViolations = new Dictionary<Bodypart, BoundaryViolation>();
    List<BoundaryViolation> finishedViolations = new List<BoundaryViolation>();

    bool taskStarted = false;

    bool outsideBoundsLastFrameP1 = false;
    private bool outsideBoundsLastFrameP2 = false;


    

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
        headPosP1 = headP1;
        rightHandPosP1 = rightHandP1;
        leftHandPosP1 = leftHandP1;
    }

    public void  setPlayer2(GameObject headP2, GameObject rightHandP2, GameObject leftHandP2)
    {
        headPosP2 = headP2;
        rightHandPosP2 = rightHandP2;
        leftHandPosP2 = leftHandP2;
    }

    public GameObject getInteractPart(string player)
    {
        return objP1;
    }

    void initTask()
    {
        currentTask = 0;

        dominantplayer = "P1";

        ShufflePossiblePositionsArray();
        initializePartsForTask();

        if (Player1Area)
        {
            WireareaDrawer drawer =  Player1Area.GetComponentInChildren<WireareaDrawer>();
            if(drawer) drawer.BoundsSize1 = boundsSize;
           // drawer.
        }

        if (Player2Area)
        {
            WireareaDrawer drawer = Player2Area.GetComponentInChildren<WireareaDrawer>();
            if(drawer) drawer.BoundsSize1 = boundsSize;
        }

        currentTaskLog = new TaskLog(userId, 0, "P1", currentTask.ToString(), Player1Area.transform, collabType, boundsSize);
    }
    

    void logUsersMovements()
    {
        if (currentTaskState > TaskState.Connected && !debug)
        {
            string violatingP1 = "NoViolationP1";
            if (currentTaskLog.startTimeOutsideBoundsP1 > 0)
                violatingP1 = "ViolationP1";
            string violatingP2 = "NoViolationP2";
            if (currentTaskLog.startTimeOutsideBoundsP2 > 0)
            {
                violatingP2 = "ViolationP2";
            }
            player1InteractionStr += userId + "," + collabType.ToString() + "," + Time.realtimeSinceStartup + "," + currentTask + "," + (dominantplayer == "P1" ? true : false) + "," + violatingP1 + ","+  Utils.vector3ToString(Player1Area.transform.position)+ "," + Utils.vector3ToString(Player1Area.transform.eulerAngles) +  ","
                                    + Utils.vector3ToString(headPosP1.transform.position) + "," + Utils.vector3ToString(headPosP1.transform.eulerAngles) + Utils.vector3ToString(rightHandPosP1.transform.position) + "," + Utils.vector3ToString(rightHandPosP1.transform.eulerAngles) +
                                     Utils.vector3ToString(leftHandPosP1.transform.position) + "," + Utils.vector3ToString(leftHandPosP1.transform.eulerAngles);
            if (player1Interacting && getInteractPart("P1"))
            {
                player1InteractionStr += "," + Utils.vector3ToString(getInteractPart("P1").transform.position) + "," + Utils.vector3ToString(getInteractPart("P1").transform.eulerAngles) + "\n";
               
            }

            if (player1InteractionStr.Length > 200)
            {
                //flush to file
                System.IO.File.AppendAllText(pathDirectory + "MovementReportP1_" + collabType.ToString() + ".csv", player1InteractionStr);

                //   System.IO.File.AppendAllText(pathDirectory + "handMovement_" + retargettingStr + "_" + surfaceOrientation.ToString() + ".csv", handMovementLogStr);
                player1InteractionStr = "";
            }



            string header2q = "userId,timestamp,collabType,currentTask,dominantPlayer,isViolatingBoundary," + Utils.vecNameToString("Player1AreaCenter") + "," + Utils.vecNameToString("Player1AreaRot")
                                + Utils.vecNameToString("headPosP2") + "," + Utils.vecNameToString("headRotP2") + "," + Utils.vecNameToString("rightHandPosP2") + Utils.vecNameToString("rightHandRotP2")
                                 + Utils.vecNameToString("leftHandPosP2") + "," + Utils.vecNameToString("leftHandRotP2") + "\n";


            player2InteractionStr += userId + ","+ collabType.ToString() + "," + Time.realtimeSinceStartup + "," + currentTask + "," + (dominantplayer == "P2" ? true : false) + "," + violatingP2 + ","+ Utils.vector3ToString(Player2Area.transform.position) + "," + Utils.vector3ToString(Player2Area.transform.eulerAngles) + ","
                                    + Utils.vector3ToString(headPosP2.transform.position) + "," + Utils.vector3ToString(headPosP2.transform.eulerAngles) + Utils.vector3ToString(rightHandPosP2.transform.position) + "," + Utils.vector3ToString(rightHandPosP2.transform.eulerAngles) +
                                     Utils.vector3ToString(leftHandPosP2.transform.position) + "," + Utils.vector3ToString(leftHandPosP2.transform.eulerAngles);
            if (player2Interacting && getInteractPart("P2"))
            {
                player2InteractionStr += "," + Utils.vector3ToString(getInteractPart("P2").transform.position) + "," + Utils.vector3ToString(getInteractPart("P2").transform.eulerAngles) + "\n";
            }


            if (player2InteractionStr.Length > 200)
            {
                //flush to file
                System.IO.File.AppendAllText(pathDirectory + "MovementReportP2_" + collabType.ToString() + ".csv", player2InteractionStr);

                player2InteractionStr = "";
            }



        }
    } 


    // Start is called before the first frame update
    void Start()
    {

        PathDirectory = Directory.GetCurrentDirectory() + "\\LogFiles";

        int i = 0;
        while (Directory.Exists(PathDirectory + "\\user" + userId + "_" + i + "\\"))
        {
            i++;
        }

        PathDirectory += "\\user" + userId + "_" + i;// + travelTechnique.ToString();// + "/";

        PathDirectory += "\\";

        if (!Directory.Exists(PathDirectory))
        {
            System.IO.Directory.CreateDirectory(PathDirectory);
        }

        logTasks = new List<TaskLog>();
        //se nao houver diretorios
    }

    // Update is called once per frame
    void Update()
    {
        //calculateAngle();

        if(Input.GetKeyDown(KeyCode.S) && !taskStarted)
        {
            initTask();
        }

        if(Input.GetKeyDown(KeyCode.N) && taskStarted)
        {
            nextPuzzle();
        }

        if(!debug)
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

    void ShufflePossiblePositionsArray()
    {
        if (rootPossiblePositionsForPuzzle)
        {
            List<int> auxIndex = new List<int>();
            listPossiblePositionsForPuzzle = new List<GameObject>();
            List<GameObject> listAux = new List<GameObject>();
            for (int i = 0; i < rootPossiblePositionsForPuzzle.transform.childCount; i++)
            {
                listPossiblePositionsForPuzzle.Add(rootPossiblePositionsForPuzzle.transform.GetChild(i).gameObject);
                auxIndex.Add(i);
            }
            var random = new System.Random();
            auxIndex = auxIndex.OrderBy(x => random.Next()).ToList();
            listAux = listPossiblePositionsForPuzzle;
            for(int i = 0;i < auxIndex.Count; i++)
            {
                listPossiblePositionsForPuzzle[auxIndex[i]] = listAux[i];
            }
        }
    }


    void initializePartsForTask()
    {
        ShufflePossiblePositionsArray();

        int numberOfObjects = 20;
        int count = 0;
        objectPartsForThisTask = new List<GameObject>();
        for(int i = 0; i < puzzleObjects[currentTask].transform.childCount; i++)
        {
            objectPartsForThisTask.Add(puzzleObjects[currentTask].transform.GetChild(i).gameObject); 
        }

        for(int i = 0; i < objectPartsForThisTask.Count; i++)
        {
            objectPartsForThisTask[i].transform.position = listPossiblePositionsForPuzzle[i].transform.position;
            objectPartsForThisTask[i].transform.rotation = listPossiblePositionsForPuzzle[i].transform.rotation;
        }

        int numberOfDistractorsToAdd = listPossiblePositionsForPuzzle.Count - objectPartsForThisTask.Count;

        if (distractorsRoot)
        {

            List<GameObject> listDistractors1 = new List<GameObject>();
            List<int> auxIndex = new List<int>();
            List<GameObject> listAux = new List<GameObject>();

            for (int i = 0; i < distractorsRoot.transform.childCount; i++)
            {
                distractorsRoot.transform.GetChild(i).gameObject.SetActive(false);
                listDistractors1.Add(distractorsRoot.transform.GetChild(i).gameObject);
                auxIndex.Add(i); 
            }

            //shuffle

            var random = new System.Random();
            auxIndex = auxIndex.OrderBy(x => random.Next()).ToList();

            for (int j = objectPartsForThisTask.Count - 1; j < objectPartsForThisTask.Count; j++)
            {
                //add some distractors to the scene
                objectPartsForThisTask[j].transform.position = listDistractors1[auxIndex[j]].transform.position;// listPossiblePositionsForPuzzle[i].transform.position;
                objectPartsForThisTask[j].transform.rotation = listDistractors1[auxIndex[j]].transform.rotation;
            }
        }
        

        //objectPartsForThisTask = puzzleObjects[currentTask].transform.GetComponentsInChildren<PuzzlePart>().gameObject;


    }



    Vector3 calculateBoundaryViolation()
    {
        Vector3 violation = new Vector3();
        if (headPosP1 && rightHandPosP1 && leftHandPosP1)
        {
            //we dont care about the Y
            Vector3 headP1Local = Player1Area.transform.InverseTransformPoint(headPosP1.transform.position);
            Vector3 rightHandP1Local = Player1Area.transform.InverseTransformPoint(rightHandPosP1.transform.position);
            Vector3 leftHandP1Local = Player1Area.transform.InverseTransformPoint(leftHandPosP1.transform.position);

            headP1Local = new Vector3(Mathf.Abs(headP1Local.x), Mathf.Abs(headP1Local.y), Mathf.Abs(headP1Local.z));
            rightHandP1Local = new Vector3(Mathf.Abs(rightHandP1Local.x), Mathf.Abs(rightHandP1Local.y), Mathf.Abs(rightHandP1Local.z));
            leftHandP1Local = new Vector3(Mathf.Abs(leftHandP1Local.x), Mathf.Abs(leftHandP1Local.y), Mathf.Abs(leftHandP1Local.z));

            float delta = Time.deltaTime;
            if (!outsideBoundsLastFrameP1)
            {

            }

            Bodypart activeBodyPart = Bodypart.HeadP1;
            if(headP1Local.x > boundsSize.x/2.0f || headP1Local.y > boundsSize.y / 2.0f || headP1Local.z > boundsSize.z / 2.0f)
            {
                float distance = Vector3.Distance(Vector3.zero, headP1Local);
                if (!listActiveViolations.ContainsKey(activeBodyPart))
                {
                    listActiveViolations[activeBodyPart] = new BoundaryViolation(activeBodyPart, Time.realtimeSinceStartup,distance);
                    currentTaskLog.incrementViolationNumber((int)activeBodyPart);
                }
                else
                {
                    if(distance > listActiveViolations[activeBodyPart].distance)
                    {
                        listActiveViolations[activeBodyPart].distance = distance;
                    }
                }
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
                    finishedViolations.Add(finished);
                }
                else
                {
                    //do nothing
                }
            }

            activeBodyPart = Bodypart.rightHandP1;
            if (rightHandP1Local.x > boundsSize.x / 2.0f || rightHandP1Local.y > boundsSize.y / 2.0f || rightHandP1Local.z > boundsSize.z / 2.0f)
            {
                float distance = Vector3.Distance(Vector3.zero, rightHandP1Local);
                if (!listActiveViolations.ContainsKey(activeBodyPart))
                {
                    listActiveViolations[activeBodyPart] = new BoundaryViolation(activeBodyPart, Time.realtimeSinceStartup, distance);
                    currentTaskLog.incrementViolationNumber((int)activeBodyPart);
                }
                else
                {
                    if (distance > listActiveViolations[activeBodyPart].distance)
                    {
                        listActiveViolations[activeBodyPart].distance = distance;
                    }
                }
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
                    finishedViolations.Add(finished);
                }
                else
                {
                    //do nothing
                }
            }

            activeBodyPart = Bodypart.leftHandP1;
            if (leftHandP1Local.x > boundsSize.x / 2.0f || leftHandP1Local.y > boundsSize.y / 2.0f || leftHandP1Local.z > boundsSize.z / 2.0f)
            {
                float distance = Vector3.Distance(Vector3.zero, leftHandP1Local);
                if (!listActiveViolations.ContainsKey(activeBodyPart))
                {
                    listActiveViolations[activeBodyPart] = new BoundaryViolation(activeBodyPart, Time.realtimeSinceStartup, distance);
                    currentTaskLog.incrementViolationNumber((int)activeBodyPart);
                }
                else
                {
                    if (distance > listActiveViolations[activeBodyPart].distance)
                    {
                        listActiveViolations[activeBodyPart].distance = distance;
                    }
                }
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
                    finishedViolations.Add(finished);
                }
                else
                {
                    //do nothing
                }
            }


        }

        if(headPosP2 && rightHandPosP2 && leftHandPosP2)
        {
            //we dont care about the Y
            Vector3 headP2Local = Player1Area.transform.InverseTransformPoint(headPosP2.transform.position);
            Vector3 rightHandP2Local = Player1Area.transform.InverseTransformPoint(rightHandPosP2.transform.position);
            Vector3 leftHandP2Local = Player1Area.transform.InverseTransformPoint(leftHandPosP2.transform.position);

            headP2Local = new Vector3(Mathf.Abs(headP2Local.x), Mathf.Abs(headP2Local.y), Mathf.Abs(headP2Local.z));
            rightHandP2Local = new Vector3(Mathf.Abs(rightHandP2Local.x), Mathf.Abs(rightHandP2Local.y), Mathf.Abs(rightHandP2Local.z));
            leftHandP2Local = new Vector3(Mathf.Abs(leftHandP2Local.x), Mathf.Abs(leftHandP2Local.y), Mathf.Abs(leftHandP2Local.z));

            float delta = Time.deltaTime;


            Bodypart activeBodyPart = Bodypart.HeadP2;
            if (headP2Local.x > boundsSize.x / 2.0f || headP2Local.y > boundsSize.y / 2.0f || headP2Local.z > boundsSize.z / 2.0f)
            {
                float distance = Vector3.Distance(Vector3.zero, headP2Local);
                if (!listActiveViolations.ContainsKey(activeBodyPart))
                {
                    listActiveViolations[activeBodyPart] = new BoundaryViolation(activeBodyPart, Time.realtimeSinceStartup, distance);
                    currentTaskLog.incrementViolationNumber((int)activeBodyPart);
                }
                else
                {
                    if (distance > listActiveViolations[activeBodyPart].distance)
                    {
                        listActiveViolations[activeBodyPart].distance = distance;
                    }
                }
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
                    finishedViolations.Add(finished);
                }
                else
                {
                    //do nothing
                }
            }

            activeBodyPart = Bodypart.rightHandP2;
            if (rightHandP2Local.x > boundsSize.x / 2.0f || rightHandP2Local.y > boundsSize.y / 2.0f || rightHandP2Local.z > boundsSize.z / 2.0f)
            {
                float distance = Vector3.Distance(Vector3.zero, rightHandP2Local);
                if (!listActiveViolations.ContainsKey(activeBodyPart))
                {
                    listActiveViolations[activeBodyPart] = new BoundaryViolation(activeBodyPart, Time.realtimeSinceStartup, distance);
                    currentTaskLog.incrementViolationNumber((int)activeBodyPart);
                }
                else
                {
                    if (distance > listActiveViolations[activeBodyPart].distance)
                    {
                        listActiveViolations[activeBodyPart].distance = distance;
                    }
                }
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
                    finishedViolations.Add(finished);
                }
                else
                {
                    //do nothing
                }
            }

            activeBodyPart = Bodypart.leftHandP2;
            if (leftHandP2Local.x > boundsSize.x / 2.0f || leftHandP2Local.y > boundsSize.y / 2.0f || leftHandP2Local.z > boundsSize.z / 2.0f)
            {
                float distance = Vector3.Distance(Vector3.zero, leftHandP2Local);
                if (!listActiveViolations.ContainsKey(activeBodyPart))
                {
                    listActiveViolations[activeBodyPart] = new BoundaryViolation(activeBodyPart, Time.realtimeSinceStartup, distance);
                    currentTaskLog.incrementViolationNumber((int)activeBodyPart);
                }
                else
                {
                    if (distance > listActiveViolations[activeBodyPart].distance)
                    {
                        listActiveViolations[activeBodyPart].distance = distance;
                    }
                }
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
                    finishedViolations.Add(finished);
                }
                else
                {
                    //do nothing
                }
            }


        }
        return violation;
    }

    void nextPuzzle()
    {
        if(dominantplayer == "P2")
        {
            if (currentTaskLog!=null)
            {
                //currentTaskLog.addTimeInteracting("P2", Time.realtimeSinceStartup);
                currentTaskLog.endTask();
            }
            dominantplayer = "P1";
            currentTaskState = TaskState.Player2Dominant;
            //hidecurrenttask
        }
        else
        {
            if (currentTaskLog != null)
            {
                //currentTaskLog.addTimeInteracting("P2", Time.realtimeSinceStartup);
                currentTaskLog.endTask();
            }
            dominantplayer = "P2";
            currentTaskState = TaskState.Player1Dominant;
            //hidecurrenttask
        }
        currentTask++;
        ShufflePossiblePositionsArray();
        initializePartsForTask();


        currentTaskLog = new TaskLog(userId,0, dominantplayer, currentTask.ToString(), Player1Area.transform, collabType, boundsSize);
        
    }

    void initializeTask()
    {
        if (Player1Area && Player2Area)
        {

        
            if (collabType == CollabType.FacetoFaceIntersect)
            {
                Player1Area.transform.localPosition = new Vector3(0, 0, 0);
                Player1Area.transform.localEulerAngles = new Vector3(0, 0, 0);
                Player2Area.transform.localPosition = new Vector3(0, 0, 0.49f);
                Player2Area.transform.localEulerAngles = new Vector3(0, 0, 0);

               
            }
            else if (collabType == CollabType.FaceToFaceNoIntersect)
            {
                Player1Area.transform.localPosition = new Vector3(0, 0, 0);
                Player1Area.transform.localEulerAngles = new Vector3(0, 0, 0);
                Player2Area.transform.localPosition = new Vector3(0, 0, 0.631f);
                Player2Area.transform.localEulerAngles = new Vector3(0, 0, 0);


                

            }
            else if (collabType == CollabType.AngledFaceToFace)
            {
                Player1Area.transform.localPosition = new Vector3(0, 0, 0);
                Player1Area.transform.localEulerAngles = new Vector3(0, 0, 0);
                Player2Area.transform.localPosition = new Vector3(0, 0, 0);
                Player2Area.transform.localEulerAngles = new Vector3(0, 0, 0);
            }
            else if (collabType == CollabType.CoupledView)
            {
                Player1Area.transform.localPosition = new Vector3(0.709f, 0, 0.453f);
                Player1Area.transform.localEulerAngles = new Vector3(0, 0, 0);
                Player2Area.transform.localPosition = new Vector3(0, 116.0f, 0);
                Player2Area.transform.localEulerAngles = new Vector3(0, 0, 0);
            }
            else if(collabType == CollabType.SideBySide)
            {
                Player1Area.transform.localPosition = new Vector3(0, 0, 0);
                Player1Area.transform.localEulerAngles = new Vector3(0.59f, 0, 0);
                Player2Area.transform.localPosition = new Vector3(0, 0, 0);
                Player2Area.transform.localEulerAngles = new Vector3(0, 0, 0);
            }

            GameObject goPlayer1 = new GameObject();
            goPlayer1.transform.parent = Player1Area.transform;
            goPlayer1.transform.localPosition = Vector3.zero;
            goPlayer1.transform.localPosition = new Vector3(-0.013f, 0.197f, 0.058f);

            GameObject traytablePlayer1 = new GameObject();
            traytablePlayer1.transform.parent = Player1Area.transform;
            traytablePlayer1.transform.localPosition = new Vector3(0.003f, -0.137f, 0.243f);
            traytablePlayer1.transform.localScale = new Vector3(0.5137f, 0.019f, 0.29f);

            GameObject goPlayer2 = new GameObject();
            goPlayer2.transform.parent = Player2Area.transform;
            goPlayer2.transform.localPosition = Vector3.zero;
            goPlayer2.transform.localPosition = new Vector3(-0.013f, 0.197f, 0.058f);

            GameObject traytablePlayer2 = new GameObject();
            traytablePlayer2.transform.parent = Player1Area.transform;
            traytablePlayer2.transform.localPosition = new Vector3(0.003f, -0.137f, 0.243f);
            traytablePlayer2.transform.localScale = new Vector3(0.5137f, 0.019f, 0.29f);
        }
    }
    void calculateAngle()
    {
       /* if (rootObject && userHead)
        {
            angleBetween = Vector3.Angle(rootObject.transform.forward, userHead.transform.forward);
            Debug.DrawRay(rootObject.transform.position, rootObject.transform.forward, Color.blue);
            Debug.DrawRay(userHead.transform.position, userHead.transform.forward, Color.cyan); 
        }*/
    }
    public void OnDisable()
    {
        if (!debug)
        {
            CompleteTaskReport();
            //CompleteHandPathReport();
        }

    }
    public void CompleteTaskReport()
    {
        //string header = "UserId,CurrentGainLevel,TargetSize,InitMovementTime,TargetPressedTime,ReachTime,TargetReleasedTime,numberErrorsFirstTarget,numberErrorsFinalTarget,SlidingTaskTime,TotalTime,ErrorFirstTime,ErrorSecondTime\n";
        string header = "UserId,TrialNumber,CurrentGainLevel,TargetSize,InitMovementTime,TargetPressedTime,ReachTime,TargetReleasedTime,DraggingTaskTime,TotalTime," +
        "ErrorInitTarget,ErrorEndTarget,InitTargetPosX,InitTargetPosY,InitTargetPosZ,FingerInitX,FingerInitY,FingerInitZ,EndTargetPosX,EndTargetPosY,EndTargetPosZ,FingerEndX,FingerEndY,FingerEndZ," +
        "nameInitTarget,nameEndTarget,ErrorInitTargetNormalized,ErrorEndTargetNormalized,snapToPlane,pathLength," + Utils.logVariable("CalibratedPlanePos") + "," + Utils.logVariable("CalibratedPlaneRot") + ",fingerReturnTime" + "\n";

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
                System.IO.File.AppendAllText(pathDirectory + "TaskReport_" + collabType.ToString() + ".csv", logTaskStr);

                int i = 0;
                while (File.Exists(pathDirectory + "TaskReport_" + collabType.ToString() + i + ".csv"))
                {
                    i++;
                }
                //logTaskStr += "#TaskTotalTime=" + (endTime - startTime);
                System.IO.File.WriteAllText(pathDirectory + "TaskReport_" + collabType.ToString() + ".csv", logTaskStr);
                /*System.IO.StreamWriter file = new System.IO.StreamWriter(pathDirectory + "TaskReport_0.csv");
                file.Write(logTaskStr);
                file.Close();*/
            }
    }
    

            //System.IO.File.WriteAllText(pathDirectory + "test.txt", logTaskStr);
            //printToFile
        }
    
}
