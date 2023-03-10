using System.Collections;
using System.Collections.Generic;

using System;
using UnityEngine;
using System.Linq;
using System.Security.Cryptography;

using OculusSampleFramework;
using System.IO;
using UnityEngine.UI;


public class PuzzleGenerator : MonoBehaviour
{
    [SerializeField]
    int numberPieces = 12;

    [SerializeField]
    int numberDistractors = 15;

    List<GameObject> parts = new List<GameObject>();
    List<GameObject> distractors = new List<GameObject>();

    [SerializeField] 
    GameObject dominantPlayerPos;

    GameObject cloneDominantPlayer;

    TaskManager taskManager;

    // Start is called before the first frame update
    void Start()
    {
        //taskManager = GameObject.Find("TaskManager").GetComponent<TaskManager>();
        generatePuzzle();
    }


    void generatePuzzle()
    {
        GameObject partsRoot = new GameObject("partsRoot");
        GameObject distractorRoot = new GameObject("distractorRoot");
        List<int> auxIndex = new List<int>();
        for(int i = 0; i < numberPieces; i++)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.transform.localScale = new Vector3(0.04f, 0.04f, 0.04f);
            obj.name = "piece" + i;
            obj.transform.parent = partsRoot.transform;
            parts.Add(obj);
            //auxIndex.Add(i);
        }

        var random = new System.Random();
        for (int i=0;i< numberDistractors; i++)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            obj.transform.localScale = new Vector3(0.04f, 0.04f, 0.04f);
            obj.name = "distractor" + i;
            obj.transform.parent = distractorRoot.transform;
            parts.Add(obj);
        }

        for(int i =0;i < (numberDistractors + numberPieces); i++)
        {
            auxIndex.Add(i);
        }

        auxIndex = auxIndex.OrderBy(x => random.Next()).ToList();
        float piecesPerQuadrant = (numberPieces + numberDistractors) / 3.0f;
        float angleIncrement = 90.0f / piecesPerQuadrant;

        //left Quadrant
        int countIndexArray = 0; 
        for(float f = -135;  f < 135.0f ; f += angleIncrement)
        {
            dominantPlayerPos.transform.localEulerAngles = new Vector3(0, f, 0);

            GameObject obj = parts[auxIndex[countIndexArray]];

            obj.transform.position = dominantPlayerPos.transform.TransformPoint(new Vector3(UnityEngine.Random.Range(0.1f,0.2f), UnityEngine.Random.Range(0.0f, 0.25f), UnityEngine.Random.Range(0.3f,0.5f)));//generate y according to proxemics and z randomly

            countIndexArray++;
        }

        if (taskManager)
        {

        }



    }

    // Update is called once per frame
    void Update()
    {
        
    }





}
