using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundaryViolation 
{
    Bodypart bPart = 0;
    bool active = false;
    float timestampInit = 0;
    public float timestampEnd = 0;
    float shortestDistance = 0;
    public float distance = 0;

    public BoundaryViolation(Bodypart bPart, float timeInit, float distance)
    {
        this.bPart = bPart;
        this.timestampInit = timeInit;
        this.distance = distance;
    }

    void incrementBoundaryViolations()
    {

    }
}
