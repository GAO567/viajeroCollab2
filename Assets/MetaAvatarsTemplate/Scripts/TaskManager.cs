using System.Collections;
using System.Collections.Generic;

using System;
using UnityEngine;
using System.Linq;
using System.Security.Cryptography;


public enum CollabType
{
    FacetoFaceIntersect, FaceToFaceNoIntersect, SideBySide, AngledFaceToFace, CoupledView
};


public class TaskManager : MonoBehaviour
{
   

    public int userId = 0;
    public CollabType collabType = CollabType.FacetoFaceIntersect;
    [SerializeField] GameObject Player1Area;
    [SerializeField] GameObject Player2Area;
    [SerializeField] GameObject userHead;
    [SerializeField] List<GameObject> puzzleObjects;
    [SerializeField] GameObject rootPossiblePositionsForPuzzle;
    List<GameObject> listPossiblePositionsForPuzzle;


    List<GameObject> objectPartsForThisTask;

    int currentTask = 0;

    List<float[]> conditionsByUserId;
    //List<TaskCondition[]> taskConditionsByUserId;
    public float angleBetween;
    private int currentPlayer;

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
        calculateAngle();


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
        if(currentPlayer == 1)
        {
            currentPlayer = 0;
        }
        else
        {
            currentPlayer = 1;
        }
        currentTask++;
        ShufflePossiblePositionsArray();
        initializePartsForTask();
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

   
}
