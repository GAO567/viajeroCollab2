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

    public string dominantPlayer = "P1";
    public string puzzleId = "DEFAULT";

    private float normalizedErrorDistanceInitTarget;
    private float normalizedErrorDistanceEndTarget;


    public TaskLog(int userId, int trialNumber, string dominantPlayer, string puzzleId, Transform areaDominantPlayer, CollabType collabType, Vector3 boundsSize)
    {
        this.dominantPlayer = dominantPlayer;
        this.puzzleId = puzzleId;
        this.userId = userId;
        this.collabType = collabType;
        this.boundsSize = boundsSize;
    }

    public string toLogString()
    {
        string logStr = "";



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
    
    //string headerTaskFile = "UserId,CurrentGainLevel,InitMovementTime,TargetPressedTime,ReachTime,TargetReleasedTime,SlidingTaskTime,TotalTime";

}
