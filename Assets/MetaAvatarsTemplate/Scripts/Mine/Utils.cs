using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

using System.IO;
using System.Security.Cryptography;

using OculusSampleFramework;
using UnityEngine.UI;


public static class Utils
{
    public static string logVariable(string variableName)
    {
        string str = variableName + "X," + variableName + "Y," + variableName + "Z";
        return str;
    }

    public static string vector3ToString(Vector3 vec3)
    {
        string str = vec3.x + "," + vec3.y + "," + vec3.z;
        return str;
    }

    public static string quatToString(Quaternion quat)
    {
        string str = quat.x + "," + quat.y + "," + quat.z + "," + quat.w;
        return str;
    }

    public static string vecNameToString(string vectorName)
    {
        string str = vectorName + "X," + vectorName + "Y," + vectorName + "Z";
        return str;
    }

    public static Quaternion stringToQuaternion(string str, char separator)
    {
        Quaternion quat = Quaternion.identity;
        string[] strArray = str.Split(separator);
        quat = new Quaternion(float.Parse(strArray[0]), float.Parse(strArray[1]), float.Parse(strArray[2]), float.Parse(strArray[3]));
        //string 
        return quat;
    }

    public static Vector3 stringToVector3(string str, char separator)
    {
        string[] strArray = str.Split(separator);
        Vector3 vecAux = new Vector3(float.Parse(strArray[0]), float.Parse(strArray[1]), float.Parse(strArray[2]));
        return vecAux;
    }


    public static List<GameObject> ShuffleArray(List<GameObject> arrayToShuffle)
    {
        List<GameObject> aux = new List<GameObject>();// arrayToShuffle;

        List<int> auxIndex = new List<int>();
        for(int i = 0; i < arrayToShuffle.Count; i++)
        {
            auxIndex.Add(i);
        }

        var random = new System.Random();
        auxIndex = auxIndex.OrderBy(x => random.Next()).ToList();

        for(int i = 0;i < arrayToShuffle.Count; i++)
        {
            int j = auxIndex[i];
            aux.Add( arrayToShuffle[auxIndex[i]]) ;
        }

        return aux;
    }

    public static Vector3 snapToPlane(Transform anchorObj, Transform planeObj, Vector3 lastPositionAnchor, Vector3 currentPosAnchor, Vector3 handDisplacement)
    {
        //remember that the anchorObj and planeObj are the parents
        Vector3 rightHandRootInPlaneLocalPos = anchorObj.TransformPoint(currentPosAnchor) + planeObj.transform.InverseTransformPoint(currentPosAnchor);
        rightHandRootInPlaneLocalPos = anchorObj.InverseTransformPoint(rightHandRootInPlaneLocalPos) + planeObj.InverseTransformPoint(handDisplacement);

        Vector3 lastHandPosInPlaneCoordinates = anchorObj.TransformPoint(lastPositionAnchor);//transform to world coordinates using the rightHandAnchor transform
        lastHandPosInPlaneCoordinates = planeObj.InverseTransformPoint(lastHandPosInPlaneCoordinates);//now transform to calibrated plane coordinates

        Vector3 finalHandPos = new Vector3(rightHandRootInPlaneLocalPos.x, rightHandRootInPlaneLocalPos.y, lastHandPosInPlaneCoordinates.z);//still in calibrated plane coordinates
        finalHandPos = planeObj.TransformPoint(finalHandPos);//transform to world coordinates again

        finalHandPos = anchorObj.InverseTransformPoint(finalHandPos);

        return finalHandPos;
    }

    public static Vector3 calculateHandPositionWithGain(GameObject anchorObj, GameObject initialReachObject, Vector3 trackerObjPos, float gain)
    {
        if (!anchorObj || initialReachObject)
            return Vector3.negativeInfinity;

        Vector3 handInInitReachObjLocalCoordinates = anchorObj.transform.parent.TransformPoint(trackerObjPos);

        //then transform to initReachObj local coordinates (the origin is in the initreachObj)
        handInInitReachObjLocalCoordinates = initialReachObject.transform.parent.InverseTransformPoint(handInInitReachObjLocalCoordinates);

        /*Vector3 handPosWithGain = new Vector3(auxPos.x,
                                      auxPos.y,
                                      (auxPos.z - initialReachObject.tr) * currentGain);*/

        /*Vector3 handPosWithGain = new Vector3(rightHandPosTmp.x,
                                      rightHandPosTmp.y,
                                      rightHandPosTmp.z * currentGain );*/

        //now apply the gain
        Vector3 handPosWithGain = new Vector3(handInInitReachObjLocalCoordinates.x,
                                      handInInitReachObjLocalCoordinates.y,
                                      handInInitReachObjLocalCoordinates.z * gain);



        handPosWithGain = initialReachObject.transform.parent.TransformPoint(handPosWithGain);

        return handPosWithGain;
    }

    public static double EffectiveAmplitude(Vector3 initialTargetPosition, Vector3 finalTargetPosition, double currentFinalProjection, double lastFinalProjection)
    {
        return (finalTargetPosition - initialTargetPosition).magnitude + currentFinalProjection + lastFinalProjection;
    }


    public static double Projected3DPointCoordinate(Vector3 initialTargetPosition, Vector3 finalTargetPosition, Vector3 realInteractionPoint)
    {
        // Returns the realInteractionPoint projected into the line defined by initial and final target positions, considering that the origin
        // of the coordinates is at the finalTargetPosition and that the coordinates are negative if they are in the same side of the initialTargetPosition point

        // This formula was obtained trigonometrically.
        // A positive value indicates an overshoot occurred, whereas a negative value indicates an undershoot occurred.
        float distanceRealToFinal = Vector3.Distance(realInteractionPoint, finalTargetPosition);
        float distanceRealToInitial = Vector3.Distance(realInteractionPoint, initialTargetPosition);
        float distanceInitialToFinal = Vector3.Distance(initialTargetPosition, finalTargetPosition);
        return (Mathf.Pow(distanceRealToInitial, 2) - (Mathf.Pow(distanceRealToFinal, 2) + Mathf.Pow(distanceInitialToFinal, 2))) / (2 * distanceInitialToFinal);
    }


    public static double ComputeStandardDeviation(this IEnumerable<double> values)
    {
        int numSamples = values.Count();
        if (numSamples > 1)
        {
            double avg = values.Average();
            return Math.Sqrt(values.Sum(v => Math.Pow(v - avg, 2)) / (numSamples - 1));
        }
        return double.NaN;
    }

    public static double EffectiveWidthForStdevValue(double stdev)
    {
        return Math.Sqrt(2 * Math.PI * Math.Exp(1)) * stdev;
    }

    public static double IndexOfDifficulty(double targetWidth, double targetsDistance)
    {
        return Math.Log((targetsDistance / targetWidth + 1), 2);
    }

}
