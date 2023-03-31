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

    List<GameObject> blueprintObjs = new List<GameObject>();

    int currentPhotonId = 200;

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

        //generateBlueprint(new Vector3(0, 0, 0), 6, 4, 3, 0.09f, null);
        //generatePuzzle(false, true,dominantPlayerPos);
    }


    public List<GameObject> generatePuzzle(bool usePrimitives,bool generate,GameObject headObj)
    {
        GameObject partsRoot = new GameObject("partsRoot");
        GameObject distractorRoot = GameObject.Find("distractorRoot");

        GameObject objAux = new GameObject(""); 
        objAux.transform.position = headObj.transform.position;
        objAux.transform.rotation = headObj.transform.rotation;
        List<int> auxIndex = new List<int>();
        if (usePrimitives)
        { 
            for(int i = 0; i < numberPieces; i++)
            {
                GameObject obj = Photon.Pun.PhotonNetwork.Instantiate("Distractor", new Vector3(), new Quaternion());
                    //GameObject.CreatePrimitive(PrimitiveType.Cube);
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
                GameObject obj = Photon.Pun.PhotonNetwork.Instantiate("DistractorCube", new Vector3(), new Quaternion());
                //obj.AddComponent<M>().color = new Color(1.0f, 0.0f, 0.0f);
                //obj.GetComponent<MeshRenderer>().material.color = new Color(1.0f, 0.0f, 0.0f);
                obj.transform.localScale = new Vector3(0.06f, 0.06f, 0.06f);
                obj.name = "distractor" + i;
                obj.transform.parent = distractorRoot.transform;

                Photon.Pun.PhotonView view = obj.GetComponent<Photon.Pun.PhotonView>();
                view.ViewID = currentPhotonId++;
                view.OwnershipTransfer = Photon.Pun.OwnershipOption.Takeover;
                view.Synchronization = Photon.Pun.ViewSynchronization.ReliableDeltaCompressed;
               

                /*SphereCollider coll = obj.AddComponent<SphereCollider>();
                Rigidbody body = obj.AddComponent<Rigidbody>();

                coll.isTrigger = true;
                body.isKinematic = true;
                body.useGravity = false;*/

                Photon.Pun.PhotonTransformView tView = obj.AddComponent<Photon.Pun.PhotonTransformView>();
                //Photon.Pun.PhotonRigidbodyView rView = obj.AddComponent<Photon.Pun.PhotonRigidbodyView>();
                tView.m_SynchronizeScale = true;
                /* obj.AddComponent<Photon.Pun.PhotonTransformView>();
                 obj.GetComponent<Photon.Pun.PhotonTransformView>().m_SynchronizePosition = true;
                 obj.GetComponent<Photon.Pun.PhotonTransformView>().m_SynchronizeRotation = true;
                 obj.GetComponent<Photon.Pun.PhotonTransformView>().m_SynchronizeScale = true;*/

                parts.Add(obj);
            }
        }
        for (int i =0;i < (numberDistractors + numberPieces); i++)
        {
            auxIndex.Add(i);
        }

        List<GameObject> arrayLeftQuadrant = new List<GameObject>();
        List<GameObject> arrayFrontQuadrant = new List<GameObject>();
        List<GameObject> arrayRightQuadrant = new List<GameObject>();

        List<GameObject> piecesOfthePuzzle = new List<GameObject>();
        List<GameObject> distractorsPuzzle = new List<GameObject>();

        List<int> auxIndexPieces = new List<int>();
        List<int> auxIndexDistractors = new List<int>();

        for(int i = 0; i < numberPieces; i++)
        {
            piecesOfthePuzzle.Add(parts[i]);
        }
        for(int i = 0;i < numberDistractors; i++)
        {
            distractorsPuzzle.Add(parts[i + numberPieces ]);
        }

        piecesOfthePuzzle = Utils.ShuffleArray(piecesOfthePuzzle);
        distractorsPuzzle = Utils.ShuffleArray(distractorsPuzzle);

        /*
        auxIndex = auxIndex.OrderBy(x => random.Next()).ToList();
        auxIndexPieces = auxIndexPieces.OrderBy(x => random.Next()).ToList();
        auxIndexDistractors = auxIndexDistractors.OrderBy(x => random.Next()).ToList();

        List<GameObject> piecesSorted = piecesOfthePuzzle;
        List<GameObject> distractorsSorted = distractorsPuzzle;

        for(int i = 0; i < piecesOfthePuzzle.Count; i++)
        {
            piecesSorted[i] = piecesOfthePuzzle[auxIndexPieces[i]];
        }

        for(int i = 0; i < distractorsPuzzle.Count; i++)
        {
            distractorsSorted[i] = distractorsPuzzle[auxIndexDistractors[i]];
        }

        List<GameObject> arrays = piecesSorted;
        arrays.AddRange(distractorsSorted);//this array has 0...numberPieces-1 <- puzzle pieces AND numberPieces...numberPieces+numberDistractors-1 distractors
        for(int i = 0; i < (numberDistractors + numberPieces); i += 3)
        {
             
        }*///Photon.Pun.PhotonNetwork.


        float piecesPerQuadrant = (numberPieces + numberDistractors) / 3.0f;
        float angleIncrement = 90.0f / piecesPerQuadrant;

        List<GameObject> sortedParts = new List<GameObject>();
        for(int i = 0; i < numberPieces/3.0f; i++)
        {
            sortedParts.Add(piecesOfthePuzzle[i]);
        }

        for(int i = 0; i < numberDistractors/3; i++)
        {
            sortedParts.Add(distractorsPuzzle[i]);
        }//first quadrant

        for(int i = numberPieces/3;i < 2 * numberPieces / 3; i++)
        {
            sortedParts.Add(piecesOfthePuzzle[i]);
        }

        for (int i = numberDistractors/3; i < 2* numberDistractors / 3; i++)
        {
            sortedParts.Add(distractorsPuzzle[i]);
        }//second quadrant

        for (int i = 2* numberPieces / 3; i <  numberPieces ; i++)
        {
            sortedParts.Add(piecesOfthePuzzle[i]);
        }

        for (int i = 2* numberDistractors / 3; i < numberDistractors ; i++)
        {
            sortedParts.Add(distractorsPuzzle[i]);
        }//third quadrant





        //left Quadrant
        int countIndexArray = 0;
        float initialAngle = objAux.transform.localEulerAngles.y;
        for(float f = -135;  f < 135.0f ; f += angleIncrement)
        {
            objAux.transform.localEulerAngles = new Vector3(0, f + initialAngle, 0);
            //print("countIndexArray" + countIndexArray);
            GameObject obj = sortedParts[countIndexArray];// parts[auxIndex[countIndexArray]];

            obj.transform.position = objAux.transform.TransformPoint(new Vector3(UnityEngine.Random.Range(0.1f,0.2f), UnityEngine.Random.Range(0.0f, 0.25f), UnityEngine.Random.Range(1.3f,2.0f)));//generate y according to proxemics and z randomly

            countIndexArray++;
        }
       
        Destroy(objAux);
        return sortedParts;
    }



    public List<GameObject> generateBlueprint(Vector3 offset, int width,int height, int depth, float sizeCube, GameObject root)
    {
        GameObject obj = GameObject.Find("rootObjects");
        bool jaTavaCriado = false;

        if (obj==null)
        {
            parts = new List<GameObject>();
            obj = new GameObject("rootObjects");
        }
        else
        {
            jaTavaCriado = true;
        }

        if (root)
        {
            rootForObjects.transform.position = root.transform.position;
            rootForObjects.transform.rotation = root.transform.rotation;
        }

        if (rootForObjects && !jaTavaCriado)
        {
            obj.transform.position = rootForObjects.transform.position;
            obj.transform.rotation = rootForObjects.transform.rotation;
            obj.transform.parent = rootForObjects.transform.parent;

            for(int i = 0; i < rootForObjects.transform.childCount; i++)
            {
                blueprintObjs.Add(rootForObjects.transform.GetChild(i).gameObject);

                Material mat = rootForObjects.transform.GetChild(i).gameObject.GetComponent<MeshRenderer>().material;
                mat.color = new Color(1,1,1,0.3f);
                string str = rootForObjects.transform.GetChild(i).gameObject.name + "_root";

                GameObject duplicate = Photon.Pun.PhotonNetwork.Instantiate(str, new Vector3(), Quaternion.identity); //.Instantiate(rootForObjects.transform.GetChild(i).gameObject);
                
                //mat = duplicate.GetComponent<MeshRenderer>().material;
                //mat.color = new Color(1, 1, 1, 1.0f);
                duplicate.transform.parent = obj.transform;
                //duplicate.AddComponent<Photon.Pun.PhotonView>().ViewID = currentPhotonId++;
                /*
                Photon.Pun.PhotonView view = duplicate.GetComponent<Photon.Pun.PhotonView>();
                Photon.Pun.PhotonTransformView tView = duplicate.AddComponent<Photon.Pun.PhotonTransformView>();
                tView.m_SynchronizeScale = true;


                view.ViewID = currentPhotonId++;
                tView.m_SynchronizeScale = true;

                view = rootForObjects.transform.GetChild(i).gameObject.AddComponent<Photon.Pun.PhotonView>();
                tView = rootForObjects.transform.GetChild(i).gameObject.AddComponent<Photon.Pun.PhotonTransformView>();
                tView.m_SynchronizeScale = true;
                
                view.ViewID = currentPhotonId++;*/
                parts.Add(duplicate);
                

                //mat.shader.
            }
        }/*
        else if(objs.Count == 0)
        {
            for(int i = 0; i < obj.transform.childCount; i++)
            {
                Material mat = obj.transform.GetChild(i).gameObject.GetComponent<MeshRenderer>().material;
                mat.color = new Color(1, 1, 1, 1);
                objs.Add(obj.transform.GetChild(i).gameObject);
                parts.Add(obj.transform.GetChild(i).gameObject);
            }
        }*/
        //parts = rj;

        List<int> auxIndex = new List<int>();
        var random = new System.Random();
        Vector3[] array = new Vector3[width * height * depth];
        GridCell[] arrayCells = new GridCell[width * height * depth];
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                for(int z = 0; z < depth; z++)
                {
                    Vector3 auxVec3 = Vector3.zero;
                    array[i* height*depth + j * depth + z] = auxVec3 = new Vector3((sizeCube / 2.0f) + (sizeCube * i), (sizeCube / 2.0f) + (sizeCube * j), (sizeCube / 2.0f) + (sizeCube * z));
                    arrayCells[i * height * depth + j * depth + z] = new GridCell(i, j, z, width, height, depth, auxVec3);
                    array[i * height * depth + j * depth + z] += offset;
                    arrayCells[i * height * depth + j * depth + z].pos += offset;
                    auxIndex.Add(i * height * depth + j * depth + z); 
                }
                /*
                array[numberColumns * i + j] = new Vector3((sizeCube / 2.0f) + (sizeCube * i), (sizeCube / 2.0f) + (sizeCube * i), 0.04f);
                array[numberColumns * i + j] += offset;*/

            }
        }
        auxIndex = auxIndex.OrderBy(x => random.Next()).ToList();
        //Vector3[] auxList = array;
        //GridCell[] auxListCells = arrayCells;

        /*for(int i = 0; i < arrayCells.Length; i++)
        {
            array[i] = auxList[auxIndex[i]];
            arrayCells[i] = auxListCells[auxIndex[i]];
        }*/

        int a = 0;
        Dictionary<int, List<GridCell>> dictionaryPerDepth = new Dictionary<int, List<GridCell>>();
        for(int i = 0; i < blueprintObjs.Count; i++)
        {
            blueprintObjs[i].transform.localPosition = arrayCells[auxIndex[i]].pos;
            arrayCells[auxIndex[i]].filled = true;
            int myHeight = arrayCells[auxIndex[i]].z;
            if (dictionaryPerDepth.ContainsKey(myHeight))
            {
                dictionaryPerDepth[myHeight].Add(arrayCells[auxIndex[i]]);
            }
            else
            {
                dictionaryPerDepth[myHeight] = new List<GridCell>();
                dictionaryPerDepth[myHeight].Add(arrayCells[auxIndex[i]]);
            }
        }

        List<GridCell> positionsBlueprint = new List<GridCell>();
        int changesCount = 0;
        for(int i = 0; i < depth; i++)
        {
            for(int j = 0; j < dictionaryPerDepth[i].Count;j++)
            {
                GridCell cell = dictionaryPerDepth[i][j];
                GridCell auxCell = Utils.nextAvailableCellInPuzzleGrid(arrayCells, cell); 
                if(cell.id != auxCell.id)
                {
                    cell.filled = false;
                    auxCell.filled = true;
                    positionsBlueprint.Add(auxCell);
                    dictionaryPerDepth[i].Remove(cell);
                    dictionaryPerDepth[auxCell.z].Add(auxCell);
                    changesCount++;
                    print("cell " + cell.id + "="+ cell.pos.ToString() + ", " + "aux Cell " + auxCell.id + "" + auxCell.pos.ToString());
                }
                else
                {
                    positionsBlueprint.Add(cell);
                }
            }
        }

        for(int i = 0; i < positionsBlueprint.Count; i++)
        {
            blueprintObjs[i].transform.localPosition = positionsBlueprint[i].pos;
        }

        

        return blueprintObjs;
    }

    int convertFrom3DToSingleIndex(int x, int y, int z,int width, int height, int depth)
    {
        return x * height * depth + y * depth + z;
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
