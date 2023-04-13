using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundaryViolation 
{
    public Bodypart bPart = 0;
    public bool active = false;
    public float timestampInit = 0;
    public float timestampEnd = 0;
    public float shortestDistance = 0;
    public float distance = 0;
    public Vector3 lastPoint = Vector3.zero;
    public int userId = 0;
    public CollabType collabType = CollabType.CoupledView;
    public int puzzleId = 0;


    List<Vector3> pointsWhenOutsideBoundary;
    public BoundaryViolation(Bodypart bPart, float timeInit, float distance)
    {
        this.bPart = bPart;
        this.timestampInit = timeInit;
        this.distance = distance;
    }

    public BoundaryViolation(int userId, int puzzleId,CollabType collabType, Bodypart bPart, float timeInit, float distance)
    {
        this.userId = userId;
        this.collabType = collabType;
        this.bPart = bPart;
        this.timestampInit = timeInit;
        this.distance = distance;
        this.puzzleId = puzzleId;
    }

    void incrementBoundaryViolations()
    {

    }

    public void addpointOutsideBoundaryArray(Vector3 point)
    {
        if (pointsWhenOutsideBoundary != null)
        {
            Vector3 aux = pointsWhenOutsideBoundary[pointsWhenOutsideBoundary.Count - 1] - point;
            aux = new Vector3(Mathf.Abs(aux.x), Mathf.Abs(aux.y), Mathf.Abs(aux.z));

            float delta = 0.01f;
            if (aux.x > delta || aux.y > delta || aux.z > delta)
            {
                pointsWhenOutsideBoundary.Add(point);
            }
        }
        else
        {
            pointsWhenOutsideBoundary = new List<Vector3>();
            pointsWhenOutsideBoundary.Add(point);
        }
    }

    float calculateDistanceElapsed()
    {
        float dist = 0;
        if(pointsWhenOutsideBoundary == null)
        {
            return 0;
        }
        for(int i=1; i < pointsWhenOutsideBoundary.Count - 1; i++)
        {
            dist += Vector3.Distance(pointsWhenOutsideBoundary[i], pointsWhenOutsideBoundary[i - 1]);
        }

        return dist;
    }


    void logLastPosition(Vector3 pos)
    {
        float deltaS = Vector3.Distance(pos, lastPoint);

        Vector3 aux = pos - lastPoint;
        aux = new Vector3(aux.x, aux.y, aux.z);
        float tolerance = 0.01f;
        if(aux.x > tolerance || aux.y > tolerance || aux.z > tolerance)
        {
            distance += deltaS;
        }
    }

    public void calculateSpeed()
    {
        float deltaS = calculateDistanceElapsed() / (timestampEnd - timestampInit);


    }

    public string toLogString()
    {
        return userId + "," + collabType + "," + puzzleId + ","+ bPart.ToString() + "," + distance + "," + (timestampEnd - timestampInit) + "\n";
    }
}
