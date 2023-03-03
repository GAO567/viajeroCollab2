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


public class TaskManager : MonoBehaviour
{
   

    public int userId = 0;
    public CollabType collabType = CollabType.FacetoFaceIntersect;
    public Vector3 boundsSize = new Vector3(0.6f,1.0f,0.8f);
    [SerializeField] GameObject Player1Area;
    [SerializeField] GameObject Player2Area;
    [SerializeField] GameObject userHead;
    [SerializeField] List<GameObject> puzzleObjects;
    [SerializeField] GameObject rootPossiblePositionsForPuzzle;
    List<GameObject> listPossiblePositionsForPuzzle;

    List<TaskLog> logTasks;
    List<GameObject> objectPartsForThisTask;
    TaskLog currentTaskLog;
    string pathDirectory;
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
        if(currentTaskState > TaskState.Connected)
        {
            player1InteractionStr += userId + "," + currentTask + "," + (dominantplayer == "P1" ? true : false) + "," + Utils.vector3ToString(Player1Area.transform.position)+ "," + Utils.vector3ToString(Player1Area.transform.eulerAngles) +  ","
                                    + Utils.vector3ToString(headPosP1.position) + "," + Utils.vector3ToString(headPosP1.eulerAngles) + Utils.vector3ToString(rightHandPosP1.position) + "," + Utils.vector3ToString(rightHandPosP1.eulerAngles) +
                                     Utils.vector3ToString(leftHandPosP1.position) + "," + Utils.vector3ToString(leftHandPosP1.eulerAngles);
            if (player1Interacting && getInteractPart("P1"))
            {
                player1InteractionStr += "," + Utils.vector3ToString(getInteractPart("P1").transform.position) + "," + Utils.vector3ToString(getInteractPart("P1").transform.eulerAngles) + "\n";
            }


            if(player1InteractionStr.Length > 200)
            {
                //flush to file

                //   System.IO.File.AppendAllText(pathDirectory + "handMovement_" + retargettingStr + "_" + surfaceOrientation.ToString() + ".csv", handMovementLogStr);
                player1InteractionStr = "";
            }

            player2InteractionStr += userId + "," + currentTask + "," + (dominantplayer == "P2" ? true : false) + "," + Utils.vector3ToString(Player2Area.transform.position) + "," + Utils.vector3ToString(Player2Area.transform.eulerAngles) + ","
                                    + Utils.vector3ToString(headPosP2.position) + "," + Utils.vector3ToString(headPosP2.eulerAngles) + Utils.vector3ToString(rightHandPosP2.position) + "," + Utils.vector3ToString(rightHandPosP2.eulerAngles) +
                                     Utils.vector3ToString(leftHandPosP2.position) + "," + Utils.vector3ToString(leftHandPosP2.eulerAngles);
            if (player2Interacting && getInteractPart("P2"))
            {
                player2InteractionStr += "," + Utils.vector3ToString(getInteractPart("P2").transform.position) + "," + Utils.vector3ToString(getInteractPart("P2").transform.eulerAngles) + "\n";
            }


            if (player2InteractionStr.Length > 200)
            {
                //flush to file
                player2InteractionStr = "";
            }



        }
    } 


    // Start is called before the first frame update
    void Start()
    {
        int[] arr = { 1, 2, 3, 4, 5 };

        var random = new System.Random();
        arr = arr.OrderBy(x => random.Next()).ToArray();
        foreach (var i in arr)
        {
            print("number "+i);
        }

        
    }

    // Update is called once per frame
    void Update()
    {
        //calculateAngle();


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

        for(int j = objectPartsForThisTask.Count - 1; j < listPossiblePositionsForPuzzle.Count; j++)
        {
            //add some distractors to the scene
        }

        //objectPartsForThisTask = puzzleObjects[currentTask].transform.GetComponentsInChildren<PuzzlePart>().gameObject;


    }


    void nextPuzzle()
    {
        if(dominantplayer == "P2")
        {
            if (currentTaskLog!=null)
            {
                currentTaskLog.addTimeInteracting("P2", Time.realtimeSinceStartup);
            }
            dominantplayer = "P1";
            currentTaskState = TaskState.Player2Dominant;
            //hidecurrenttask
        }
        else
        {
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
            /* foreach (TaskLog tLog in logTasks)
             {
                 logTaskStr += tLog.toLogString();
             }
             if (!File.Exists(pathDirectory + "TaskReport_" + retargettingStr + "_" + surfaceOrientation + ".csv"))
             {
                 System.IO.File.AppendAllText(pathDirectory + "TaskReport_" + retargettingStr + "_" + surfaceOrientation + ".csv", logTaskStr);
                 System.IO.StreamWriter file = new System.IO.StreamWriter(pathDirectory + "TaskReport.csv");
                 file.WriteLine(logTaskStr);
                 file.Close();
        }
        else
            {
                System.IO.File.AppendAllText(pathDirectory + "TaskReport_" + retargettingStr + "_" + surfaceOrientation + ".csv", logTaskStr);

        
                while (File.Exists(pathDirectory + "TaskReport_" + i + ".csv"))
                {
                    i++;
                }
                logTaskStr += "#TaskTotalTime=" + (endTime - startTime);
                System.IO.File.WriteAllText(pathDirectory + "TaskReport_" + i + ".csv", logTaskStr);
                /*System.IO.StreamWriter file = new System.IO.StreamWriter(pathDirectory + "TaskReport_0.csv");
                file.Write(logTaskStr);
                file.Close();
            }
    }
    */

            //System.IO.File.WriteAllText(pathDirectory + "test.txt", logTaskStr);
            //printToFile
        }
    }


}
