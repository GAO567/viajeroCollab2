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

    [SerializeField]
    GameObject rootForObjects;

    List<GameObject> objs = new List<GameObject>();

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
        //generatePuzzle();

        generateBlueprint(new Vector3(0, 0, 0), 6, 4, 3, 0.09f, null);
        generatePuzzle(false, true,dominantPlayerPos);
    }


    public void generatePuzzle(bool usePrimitives,bool generate,GameObject headObj)
    {
        GameObject partsRoot = new GameObject("partsRoot");
        GameObject distractorRoot = new GameObject("distractorRoot");

        GameObject objAux = new GameObject(""); 
        objAux.transform.position = headObj.transform.position;
        objAux.transform.rotation = headObj.transform.rotation;
        List<int> auxIndex = new List<int>();
        if (usePrimitives)
        { 
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
        }
        var random = new System.Random();
        if (generate) { 
            for (int i=0;i< numberDistractors; i++)
            {
                GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                //obj.AddComponent<M>().color = new Color(1.0f, 0.0f, 0.0f);
                obj.GetComponent<MeshRenderer>().material.color = new Color(1.0f, 0.0f, 0.0f);
                obj.transform.localScale = new Vector3(0.06f, 0.06f, 0.06f);
                obj.name = "distractor" + i;
                obj.transform.parent = distractorRoot.transform;
                obj.AddComponent<Photon.Pun.PhotonTransformView>();
                obj.GetComponent<Photon.Pun.PhotonTransformView>().m_SynchronizePosition = true;
                obj.GetComponent<Photon.Pun.PhotonTransformView>().m_SynchronizeRotation = true;
                obj.GetComponent<Photon.Pun.PhotonTransformView>().m_SynchronizeScale = true;

                parts.Add(obj);
            }
        }
        for (int i =0;i < (numberDistractors + numberPieces); i++)
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
    }


    


    public void generatePuzzle(List<GameObject> distractors, List<GameObject> parts, GameObject rootObject,bool generate)
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



    public void generateBlueprint(Vector3 offset, int width,int height, int depth, float sizeCube, GameObject root)
    {
        GameObject obj = GameObject.Find("rootPieces");
        bool jaTavaCriado = false;

        if (obj==null)
        {
            parts = new List<GameObject>();
            obj = new GameObject("rootPieces");
        }
        else
        {
            jaTavaCriado = true;
        }

        if (root)
        {
            rootForObjects.transform.position = root.transform.position;
        }

        if (rootForObjects && !jaTavaCriado)
        {
            obj.transform.position = rootForObjects.transform.position;

            for(int i = 0; i < rootForObjects.transform.childCount; i++)
            {
                objs.Add(rootForObjects.transform.GetChild(i).gameObject);

                Material mat = rootForObjects.transform.GetChild(i).gameObject.GetComponent<MeshRenderer>().material;
                mat.color = new Color(1,1,1,0.3f);

                GameObject duplicate = GameObject.Instantiate(rootForObjects.transform.GetChild(i).gameObject);

                mat = duplicate.GetComponent<MeshRenderer>().material;
                mat.color = new Color(1, 1, 1, 1.0f);
                duplicate.transform.parent = obj.transform;
                parts.Add(duplicate);

                //mat.shader.
            }
        }
        //parts = rj;

        List<int> auxIndex = new List<int>();
        var random = new System.Random();
        Vector3[] array = new Vector3[width * height * depth];
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                for(int z = 0; z < depth; z++)
                {
                    array[i* height*depth + j * depth + z] = new Vector3((sizeCube / 2.0f) + (sizeCube * i), (sizeCube / 2.0f) + (sizeCube * j), (sizeCube / 2.0f) + (sizeCube * z));
                    array[i * height * depth + j * depth + z] += offset;
                    auxIndex.Add(i * height * depth + j * depth + z); 
                }
                /*
                array[numberColumns * i + j] = new Vector3((sizeCube / 2.0f) + (sizeCube * i), (sizeCube / 2.0f) + (sizeCube * i), 0.04f);
                array[numberColumns * i + j] += offset;*/

            }
        }
        auxIndex = auxIndex.OrderBy(x => random.Next()).ToList();
        Vector3[] auxList = array;

        for(int i = 0; i < array.Length; i++)
        {
            array[i] = auxList[auxIndex[i]]; 
        }

        int a = 0;
        for(int i = 0; i < array.Length; i++)
        {
            a = i % objs.Count;
            objs[a].transform.localPosition = array[i];
            a++;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            generateBlueprint(new Vector3(0, 0, 0), 6, 4, 3, 0.09f, null);
            generatePuzzle(false, false,dominantPlayerPos);
        }
    }





}
