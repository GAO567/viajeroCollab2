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

    List<Color> listColors;

    [SerializeField]
    Material mat;

    // Start is called before the first frame update
    void Start()
    {
        listColors = new List<Color>();
        listColors.Add(new Color(1.0f, 0.0f, 0.0f));
        listColors.Add(new Color(0.0f, 1.0f, 0.0f));
        listColors.Add(new Color(0.0f, 0.0f, 1.0f));
        listColors.Add(new Color(1.0f, 1.0f, 0.0f));
        listColors.Add(new Color(0.0f, 1.0f, 1.0f));
        //taskManager = GameObject.Find("TaskManager").GetComponent<TaskManager>();
        generatePuzzle();
    }


    void generatePuzzle()
    {
        GameObject partsRoot = new GameObject("partsRoot");
        GameObject distractorRoot = new GameObject("distractorRoot");

        GameObject objAux = new GameObject(""); 
        objAux.transform.position = dominantPlayerPos.transform.position;
        objAux.transform.rotation = dominantPlayerPos.transform.rotation;
        List<int> auxIndex = new List<int>();
        for(int i = 0; i < numberPieces; i++)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.GetComponent<MeshRenderer>().material = mat;
            obj.GetComponent<MeshRenderer>().material.color = new Color(0.0f, 0.0f, 1.0f);
            obj.transform.localScale = new Vector3(0.06f, 0.06f, 0.06f);
            obj.name = "piece" + i;
            obj.transform.parent = partsRoot.transform;
            parts.Add(obj);

            obj.GetComponent<Photon.Pun.PhotonTransformView>().m_SynchronizePosition = true;
            obj.GetComponent<Photon.Pun.PhotonTransformView>().m_SynchronizeRotation = true;
            obj.GetComponent<Photon.Pun.PhotonTransformView>().m_SynchronizeScale = true;
            //auxIndex.Add(i);
        }

        var random = new System.Random();
        for (int i=0;i< numberDistractors; i++)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //obj.AddComponent<M>().color = new Color(1.0f, 0.0f, 0.0f);
            obj.GetComponent<MeshRenderer>().material.color = new Color(1.0f, 0.0f, 0.0f);
            obj.transform.localScale = new Vector3(0.04f, 0.04f, 0.04f);
            obj.name = "distractor" + i;
            obj.transform.parent = distractorRoot.transform;
            obj.AddComponent<Photon.Pun.PhotonTransformView>();
            obj.GetComponent<Photon.Pun.PhotonTransformView>().m_SynchronizePosition = true;
            obj.GetComponent<Photon.Pun.PhotonTransformView>().m_SynchronizeRotation = true;
            obj.GetComponent<Photon.Pun.PhotonTransformView>().m_SynchronizeScale = true;

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
            objAux.transform.localEulerAngles = new Vector3(0, f, 0);

            GameObject obj = parts[auxIndex[countIndexArray]];

            obj.transform.position = objAux.transform.TransformPoint(new Vector3(UnityEngine.Random.Range(0.1f,0.2f), UnityEngine.Random.Range(0.0f, 0.25f), UnityEngine.Random.Range(0.7f,1.8f)));//generate y according to proxemics and z randomly

            countIndexArray++;
        }

        Destroy(objAux);
        if (taskManager)
        {
            
        }



    }


    void generatePuzzle(List<GameObject> distractors, List<GameObject> parts, GameObject rootObject, CollabType type)
    {

    }


    void generatePuzzle(List<GameObject> distractors, List<GameObject> parts, GameObject rootObject)
    {
        List<int> auxIndex = new List<int>();
        var random = new System.Random();
        numberDistractors = distractors.Count;
        numberPieces = parts.Count;

        GameObject objAux = new GameObject("");
        objAux.transform.position = rootObject.transform.position;
        objAux.transform.rotation = rootObject.transform.rotation;
        for (int i = 0; i < (numberDistractors + numberPieces); i++)
        {
            auxIndex.Add(i);
        }

        for(int i = 0;i < distractors.Count; i++)
        {
            parts.Add(parts[i]);
        }

        auxIndex = auxIndex.OrderBy(x => random.Next()).ToList();
        float piecesPerQuadrant = (numberPieces + numberDistractors) / 3.0f;
        float angleIncrement = 90.0f / piecesPerQuadrant;

        //left Quadrant
        int countIndexArray = 0;
        for (float f = -135; f < 135.0f; f += angleIncrement)
        {
            objAux.transform.localEulerAngles = new Vector3(0, f, 0);

            GameObject obj = parts[auxIndex[countIndexArray]];

            obj.transform.position = objAux.transform.TransformPoint(new Vector3(UnityEngine.Random.Range(0.1f, 0.2f), UnityEngine.Random.Range(0.0f, 0.25f), UnityEngine.Random.Range(0.3f, 0.5f)));//generate y according to proxemics and z randomly

            countIndexArray++;
        }

        Destroy(objAux);
        if (taskManager)
        {

        }


    }

    // Update is called once per frame
    void Update()
    {
        
    }





}
