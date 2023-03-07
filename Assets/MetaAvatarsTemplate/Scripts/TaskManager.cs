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

    Transform headPosP1;
    Transform rightHandPosP1;
    Transform leftHandPosP1;

    Transform headPosP2;
    Transform leftHandPosP2;
    Transform rightHandPosP2;



    bool player1Interacting = false;
    bool player2Interacting = false;

    GameObject objP1;// = new GameObject("partBeingInteracted");
    GameObject objP2;// = new G

    public string PathDirectory { get => pathDirectory; set => pathDirectory = value; }

    public TaskState currentTaskState = TaskState.Idle;

    public string headerPlayer1Interaction = "userId,currentTask,dominantPlayer," + Utils.vecNameToString("centerBoundsPos") + "," + Utils.vecNameToString("centerAreaRot") + "," +
                                                Utils.vecNameToString("headPos") + "," + Utils.vecNameToString("headRot") + ","+  Utils.vecNameToString("rightHandPos") + Utils.vecNameToString("rightHandRot") + ","+
                                                Utils.vecNameToString("leftHandPos") + Utils.vecNameToString("leftHandRot") + "partId," + Utils.vecNameToString("objInteractedPos") + "," + Utils.vecNameToString("objInteractedRot") + "\n";
    public string player1InteractionStr = "";
    private string player2InteractionStr;




    Dictionary<string, List<ActiveCollision>> activeCollisions;
    List<FinishedCollision> finishedCollisions;
    Dictionary<string, FinishedCollision> finishedCollisionsAux;

    bool taskStarted = false;

    bool outsideBoundsLastFrameP1 = false;
    private bool outsideBoundsLastFrameP2 = false;

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
        if(currentTaskState > TaskState.Connected && !debug) 
        {
            string violatingP1 = "NoViolationP1";
            if (currentTaskLog.startTimeOutsideBoundsP1 > 0)
                violatingP1 = "ViolationP1";
            string violatingP2 = "NoViolationP2";
            if(currentTaskLog.startTimeOutsideBoundsP2 > 0)
            {
                violatingP2 = "ViolationP2";
            }

            player1InteractionStr += userId + "," + Time.realtimeSinceStartup + "," + currentTask + "," + (dominantplayer == "P1" ? true : false) + "," + violatingP1 + ","+  Utils.vector3ToString(Player1Area.transform.position)+ "," + Utils.vector3ToString(Player1Area.transform.eulerAngles) +  ","
                                    + Utils.vector3ToString(headPosP1.position) + "," + Utils.vector3ToString(headPosP1.eulerAngles) + Utils.vector3ToString(rightHandPosP1.position) + "," + Utils.vector3ToString(rightHandPosP1.eulerAngles) +
                                     Utils.vector3ToString(leftHandPosP1.position) + "," + Utils.vector3ToString(leftHandPosP1.eulerAngles);
            if (player1Interacting && getInteractPart("P1"))
            {
                player1InteractionStr += "," + Utils.vector3ToString(getInteractPart("P1").transform.position) + "," + Utils.vector3ToString(getInteractPart("P1").transform.eulerAngles) + "\n";
                System.IO.File.AppendAllText(pathDirectory + "MovementReportP1_" + collabType.ToString() + ".csv", player1InteractionStr);

            }


            if (player1InteractionStr.Length > 200)
            {
                //flush to file

                //   System.IO.File.AppendAllText(pathDirectory + "handMovement_" + retargettingStr + "_" + surfaceOrientation.ToString() + ".csv", handMovementLogStr);
                player1InteractionStr = "";
            }

            player2InteractionStr += userId + ","+ Time.realtimeSinceStartup + "," + currentTask + "," + (dominantplayer == "P2" ? true : false) + "," + violatingP2 + ","+ Utils.vector3ToString(Player2Area.transform.position) + "," + Utils.vector3ToString(Player2Area.transform.eulerAngles) + ","
                                    + Utils.vector3ToString(headPosP2.position) + "," + Utils.vector3ToString(headPosP2.eulerAngles) + Utils.vector3ToString(rightHandPosP2.position) + "," + Utils.vector3ToString(rightHandPosP2.eulerAngles) +
                                     Utils.vector3ToString(leftHandPosP2.position) + "," + Utils.vector3ToString(leftHandPosP2.eulerAngles);
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
            Vector3 headPosP1Local = Player1Area.transform.InverseTransformPoint(headPosP1.transform.position);
            Vector3 rightHandPosP1Local = Player1Area.transform.InverseTransformPoint(rightHandPosP1.transform.position);
            Vector3 leftHandPosP1Local = Player1Area.transform.InverseTransformPoint(leftHandPosP1.transform.position);

            headPosP1Local = new Vector3(Mathf.Abs(headPosP1Local.x), Mathf.Abs(headPosP1Local.y), Mathf.Abs(headPosP1Local.z));
            rightHandPosP1Local = new Vector3(Mathf.Abs(rightHandPosP1Local.x), Mathf.Abs(rightHandPosP1Local.y), Mathf.Abs(rightHandPosP1Local.z));
            leftHandPosP1Local = new Vector3(Mathf.Abs(leftHandPosP1Local.x), Mathf.Abs(leftHandPosP1Local.y), Mathf.Abs(leftHandPosP1Local.z));

            float delta = Time.deltaTime;
            if (!outsideBoundsLastFrameP1)
            {

            }
            if(headPosP1Local.x > boundsSize.x/2.0f || headPosP1Local.y > boundsSize.y / 2.0f || headPosP1Local.z > boundsSize.z / 2.0f)
            {
                outsideBoundsLastFrameP1 = currentTaskLog.lastFrameBoundaryViolation[(int)Bodypart.HeadP1];
                float distance = Vector3.Distance(Vector3.zero, headPosP1Local);
                if (!outsideBoundsLastFrameP1)
                {
                    outsideBoundsLastFrameP1 = true;
                    currentTaskLog.startTimeOutsideBoundsP1 = Time.realtimeSinceStartup;
                    currentTaskLog.incrementBoundViolationsP1();

                    currentTaskLog.violationNumber[(int)Bodypart.HeadP1]++;
                    currentTaskLog.shortestDistances[(int)Bodypart.HeadP1] = distance;
                    currentTaskLog.lastFrameBoundaryViolation[(int)Bodypart.HeadP1] = true;


                    //if(currentTaskLog.)
                }
                else
                {
                    if(distance > currentTaskLog.shortestDistances[(int)Bodypart.HeadP1])
                    {
                        currentTaskLog.shortestDistances[(int)Bodypart.HeadP1] = distance;
                    }
                }
                //currentTaskLog.incrementTimeOutsideBounds(Time.deltaTime);
                //
            }
            else if (rightHandPosP1Local.x > boundsSize.x / 2.0f || rightHandPosP1Local.y > boundsSize.y / 2.0f || rightHandPosP1Local.z > boundsSize.z / 2.0f)
            {
                float distance = Vector3.Distance(Vector3.zero, rightHandPosP1Local);

                outsideBoundsLastFrameP1 = currentTaskLog.lastFrameBoundaryViolation[(int)Bodypart.rightHandP1];
                if (!outsideBoundsLastFrameP1)
                {
                    outsideBoundsLastFrameP1 = true;
                    currentTaskLog.startTimeOutsideBoundsP1 = Time.realtimeSinceStartup;
                    currentTaskLog.incrementBoundViolationsP1();
                    currentTaskLog.violationNumber[(int)Bodypart.rightHandP1]++;
                    currentTaskLog.shortestDistances[(int)Bodypart.rightHandP1] = distance;
                    currentTaskLog.lastFrameBoundaryViolation[(int)Bodypart.rightHandP1] = true;
                }
                else
                {
                    if (distance > currentTaskLog.shortestDistances[(int)Bodypart.rightHandP1])
                    {
                        currentTaskLog.shortestDistances[(int)Bodypart.rightHandP1] = distance;
                    }
                }
                //currentTaskLog.incrementTimeOutsideBounds(Time.deltaTime);
                //
            }
            else if (leftHandPosP1Local.x > boundsSize.x / 2.0f || leftHandPosP1Local.y > boundsSize.y / 2.0f || leftHandPosP1Local.z > boundsSize.z / 2.0f)
            {
                float distance = Vector3.Distance(Vector3.zero, leftHandPosP1Local);
                outsideBoundsLastFrameP1 = currentTaskLog.lastFrameBoundaryViolation[(int)Bodypart.leftHandP1];
                if (!outsideBoundsLastFrameP1)
                {
                    outsideBoundsLastFrameP1 = true;
                    currentTaskLog.startTimeOutsideBoundsP1 = Time.realtimeSinceStartup;
                    currentTaskLog.incrementBoundViolationsP1();

                    currentTaskLog.violationNumber[(int)Bodypart.leftHandP1]++;
                    currentTaskLog.shortestDistances[(int)Bodypart.leftHandP1] = distance;
                    currentTaskLog.lastFrameBoundaryViolation[(int)Bodypart.leftHandP1] = true;
                }
                else
                {
                    if (distance > currentTaskLog.shortestDistances[(int)Bodypart.leftHandP1])
                    {
                        currentTaskLog.shortestDistances[(int)Bodypart.leftHandP1] = distance;
                    }
                }
                //currentTaskLog.incrementTimeOutsideBounds(Time.deltaTime);
                //
            }
            else
            {
                if (outsideBoundsLastFrameP1)
                {
                    currentTaskLog.incrementTimeOutsideBoundsP1(Time.realtimeSinceStartup);
                }
            }


        }

        if(headPosP2 && rightHandPosP2 && leftHandPosP2)
        {
            //we dont care about the Y
            Vector3 headPosP2Local = Player1Area.transform.InverseTransformPoint(headPosP2.transform.position);
            Vector3 rightHandPosP2Local = Player1Area.transform.InverseTransformPoint(rightHandPosP2.transform.position);
            Vector3 leftHandPosP2Local = Player1Area.transform.InverseTransformPoint(leftHandPosP2.transform.position);

            headPosP2Local = new Vector3(Mathf.Abs(headPosP2Local.x), Mathf.Abs(headPosP2Local.y), Mathf.Abs(headPosP2Local.z));
            rightHandPosP2Local = new Vector3(Mathf.Abs(rightHandPosP2Local.x), Mathf.Abs(rightHandPosP2Local.y), Mathf.Abs(rightHandPosP2Local.z));
            leftHandPosP2Local = new Vector3(Mathf.Abs(leftHandPosP2Local.x), Mathf.Abs(leftHandPosP2Local.y), Mathf.Abs(leftHandPosP2Local.z));

            float delta = Time.deltaTime;
            if (headPosP2Local.x > boundsSize.x / 2.0f || headPosP2Local.y > boundsSize.y / 2.0f || headPosP2Local.z > boundsSize.z / 2.0f)
            {
                float distance = Vector3.Distance(Vector3.zero, headPosP2Local);
                outsideBoundsLastFrameP1 = currentTaskLog.lastFrameBoundaryViolation[(int)Bodypart.HeadP2];
                if (!outsideBoundsLastFrameP2)
                {
                    outsideBoundsLastFrameP2 = true;
                    currentTaskLog.startTimeOutsideBoundsP2 = Time.realtimeSinceStartup;
                    currentTaskLog.incrementBoundViolationsP2();
                    currentTaskLog.violationNumber[(int)Bodypart.HeadP2]++;
                    currentTaskLog.shortestDistances[(int)Bodypart.HeadP2] = distance;
                    currentTaskLog.lastFrameBoundaryViolation[(int)Bodypart.HeadP2] = true;

                }
                else
                {
                    if (distance > currentTaskLog.shortestDistances[(int)Bodypart.HeadP2])
                    {
                        currentTaskLog.shortestDistances[(int)Bodypart.HeadP2] = distance;
                    }
                }
                //currentTaskLog.incrementTimeOutsideBounds(Time.deltaTime);
                //
            }
            else if(rightHandPosP2Local.x > boundsSize.x / 2.0f || rightHandPosP2Local.y > boundsSize.y / 2.0f || rightHandPosP2Local.z > boundsSize.z / 2.0f)
            {
                float distance = Vector3.Distance(Vector3.zero, rightHandPosP2Local);
                outsideBoundsLastFrameP1 = currentTaskLog.lastFrameBoundaryViolation[(int)Bodypart.rightHandP2];
                if (!outsideBoundsLastFrameP2)
                {
                    outsideBoundsLastFrameP2 = true;
                    currentTaskLog.startTimeOutsideBoundsP2 = Time.realtimeSinceStartup;
                    currentTaskLog.incrementBoundViolationsP2();
                    currentTaskLog.violationNumber[(int)Bodypart.rightHandP2]++;
                    currentTaskLog.shortestDistances[(int)Bodypart.rightHandP2] = distance;
                    currentTaskLog.lastFrameBoundaryViolation[(int)Bodypart.rightHandP2] = true;
                }
                else
                {
                    if (distance > currentTaskLog.shortestDistances[(int)Bodypart.rightHandP2])
                    {
                        currentTaskLog.shortestDistances[(int)Bodypart.rightHandP2] = distance;
                    }
                }
                //currentTaskLog.incrementTimeOutsideBounds(Time.deltaTime);
                //
            }
            else if (leftHandPosP2Local.x > boundsSize.x / 2.0f || leftHandPosP2Local.y > boundsSize.y / 2.0f || leftHandPosP2Local.z > boundsSize.z / 2.0f)
            {
                float distance = Vector3.Distance(Vector3.zero, leftHandPosP2Local);
                outsideBoundsLastFrameP1 = currentTaskLog.lastFrameBoundaryViolation[(int)Bodypart.leftHandP2];
                if (!outsideBoundsLastFrameP2)
                {
                    outsideBoundsLastFrameP2 = true;
                    currentTaskLog.startTimeOutsideBoundsP2 = Time.realtimeSinceStartup;
                    currentTaskLog.incrementBoundViolationsP2();
                    currentTaskLog.violationNumber[(int)Bodypart.leftHandP2]++;
                    currentTaskLog.shortestDistances[(int)Bodypart.leftHandP2] = distance;
                    currentTaskLog.lastFrameBoundaryViolation[(int)Bodypart.leftHandP2] = true;
                }
                else
                {
                    if (distance > currentTaskLog.shortestDistances[(int)Bodypart.leftHandP2])
                    {
                        currentTaskLog.shortestDistances[(int)Bodypart.leftHandP2] = distance;
                    }
                }
                //currentTaskLog.incrementTimeOutsideBounds(Time.deltaTime);
                //
            }
            else
            {
                outsideBoundsLastFrameP2 = currentTaskLog.lastFrameBoundaryViolation[(int)Bodypart.leftHandP2];
                if (outsideBoundsLastFrameP2)
                {
                    currentTaskLog.incrementTimeOutsideBoundsP2(Time.realtimeSinceStartup);
                }
                outsideBoundsLastFrameP2 = false;
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
        if (collabType == CollabType.FacetoFaceIntersect)
        {

        }
        else if (collabType == CollabType.FaceToFaceNoIntersect)
        {

        }
        else if (collabType == CollabType.AngledFaceToFace)
        {

        }
        else if (collabType == CollabType.CoupledView)
        {

        }
        else if(collabType == CollabType.SideBySide)
        {

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
