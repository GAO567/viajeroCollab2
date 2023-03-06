using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskLog
{
   
    string separator = ",";
    bool training = false;

    float errorFirstTarget;
    float errorEndTarget;
    bool isTraining;

    Vector3 centerPosDominantPlayer;
    Vector3 centerRotDominantPlayer;

    CollabType collabType = CollabType.FacetoFaceIntersect;

    float timestampInit = 0;
    float timeElapsed = 0;

    int userId = 0;
    int trialNumber = 0;

    float amounttimeDominantInteracting = 0;
    float amounttimePassiveInteracting = 0;
    

    public string headerTaskFile = "UserId,TrialNumber,dominantPlayer,"+ Utils.vecNameToString("centerAreaDominantPlayerPos")+","+ Utils.vecNameToString("centerAreaDominantPlayerRot")+","+
                                    Utils.vecNameToString ("boundsSize") + "\n";

    Vector3 boundsSize = Vector3.one;
    private Vector3 centerPosArea;
    public string dominantPlayer = "P1";
    public string puzzleId = "DEFAULT";

    private float normalizedErrorDistanceInitTarget;
    private float normalizedErrorDistanceEndTarget;

    List<GameObject> objParts;
    List<GameObject> bluePrintParts;
    private Vector3 centerRotArea;

    public TaskLog(int userId, int trialNumber, string dominantPlayer, string puzzleId, Transform areaDominantPlayer, CollabType collabType, Vector3 boundsSize)
    {
        this.dominantPlayer = dominantPlayer;
        this.puzzleId = puzzleId;
        this.userId = userId;
        this.collabType = collabType;
        this.boundsSize = boundsSize;
        this.centerPosArea = areaDominantPlayer.transform.position;
        this.centerRotArea = areaDominantPlayer.transform.eulerAngles;
    }

    public string toLogString()
    {
        string logStr = "";

        logStr = userId + "," + trialNumber + "," + dominantPlayer + "," + puzzleId + "," + Utils.vector3ToString(centerPosArea) + "," + Utils.vector3ToString(centerRotArea) + "," + collabType.ToString() +
            "," + Utils.vector3ToString(boundsSize);
        if(bluePrintParts != null && objParts != null)
        {
            logStr += "," + objParts.Count;//add the number of parts to make sure we have the right amount
            foreach (GameObject obj in objParts)
            {
                logStr += "," + obj.name + "_part$" + "," + Utils.vector3ToString(obj.transform.position) + "," + Utils.vector3ToString(obj.transform.eulerAngles);
            }

            foreach (GameObject obj in bluePrintParts)
            {
                logStr += "," + obj.name + "_blue@" + "," + Utils.vector3ToString(obj.transform.position) + "," + Utils.vector3ToString(obj.transform.eulerAngles);
            }
        }
        logStr += "\n";
        return logStr;
    }

    public void setTaskEndTime(float timestampEnd)
    {
        timeElapsed = timestampEnd - timestampInit;
    }

    public void addTimeInteracting(string player, float timeElapsed)
    {
        if (player == dominantPlayer)
        {
            this.amounttimeDominantInteracting += timeElapsed;
        }
        else
            this.amounttimePassiveInteracting += timeElapsed;
    }

    public void setPuzzleParts(ref List<GameObject> obj)
    {
        objParts = obj;
    }

    public void setBlueprintParts(ref List<GameObject> obj)
    {
        bluePrintParts = obj;
    }
    
    //string headerTaskFile = "UserId,CurrentGainLevel,InitMovementTime,TargetPressedTime,ReachTime,TargetReleasedTime,SlidingTaskTime,TotalTime";

}
