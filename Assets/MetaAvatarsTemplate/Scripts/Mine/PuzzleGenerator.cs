using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        for(int i = 0; i < numberPieces; i++)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            obj.name = "piece" + i;
            obj.transform.parent = partsRoot.transform;
            parts.Add(obj);
        }

        for(int i=0;i< numberDistractors; i++)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            obj.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            obj.name = "distractor" + i;
            obj.transform.parent = distractorRoot.transform;
            parts.Add(obj);
        }

        float piecesPerQuadrant = (numberPieces + numberDistractors) / 3.0f;
        float angleIncrement = 90.0f / piecesPerQuadrant;

        //left Quadrant
        int countIndexArray = 0; 
        for(float f = -135;  f < 135.0f ; f += angleIncrement)
        {
            dominantPlayerPos.transform.localEulerAngles = new Vector3(0, f, 0);

            GameObject obj = parts[countIndexArray];

            obj.transform.position = dominantPlayerPos.transform.TransformPoint(new Vector3(0, 0.4f, 0.5f));//generate y according to proxemics and z randomly

            countIndexArray++;
        }





    }

    // Update is called once per frame
    void Update()
    {
        
    }





}
