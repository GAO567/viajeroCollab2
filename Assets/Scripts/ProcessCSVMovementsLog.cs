using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public class ProcessCSVMovementsLog : MonoBehaviour
{

    private string fileName;
    private string filePath;

    // Start is called before the first frame update
    void Start()
    {
        //processTotalTIme();
        processGazeViolationsWithDominantAndNonDominant();
        //processCSV2();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    void processForHeatmap()
    {
        GameObject rootObjects = new GameObject("RootAreas");

        GameObject Player1Area = new GameObject("Player1Area");
        GameObject Player2Area = new GameObject("Player2Area");

        Player1Area.transform.parent = rootObjects.transform;
        Player2Area.transform.parent = rootObjects.transform;

        rootObjects.transform.localPosition = new Vector3(0, 0.471f, -0.372f);

        Player1Area.transform.localPosition = new Vector3(0, 0, 0);
        Player2Area.transform.localPosition = new Vector3(0, 0, 0.49f);//size 0.5,1,0.8f
        Player2Area.transform.localEulerAngles = new Vector3(0, 180, 0);

        Vector3 boundsSize = new Vector3(0.5f, 1, 0.8f);

        GameObject head = new GameObject("head");
        GameObject rightHand = new GameObject("rightHand");
        GameObject leftHand = new GameObject("leftHand");
        GameObject centerBounds = new GameObject("centerBounds");

        Vector3 sizeBounds = new Vector3(1.0f, 0.5f, 0.5f);

        float timeOutsideBounds = 0;
        float lastFrameTimestamp = 0;

        
        string basePath = Directory.GetCurrentDirectory() + "\\LogFiles\\";
        // Read the file and display it line by line.  
        string fileName = "allIn.csv";
        string header = "userId,dominant,timestamp,condition," + Utils.vecNameToString("centerBoundsPos") + "," + Utils.vecNameToString("centerBoundsRot") + "," + Utils.vecNameToString("headPos") + "," + Utils.vecNameToString("headRot") +
                                    "," + Utils.vecNameToString("rightHandPos") + "," + Utils.vecNameToString("rightHandRot") + "," + Utils.vecNameToString("leftHandPos") + "," + Utils.vecNameToString("leftHandRot") + "," +
                                    Utils.vecNameToString("projectedHeadAngle") + "," + "isViolatingBoundary\n";
        /*public string headerPlayer1Interaction = "userId,currentTask,dominantPlayer," + Utils.vecNameToString("centerBoundsPos") + "," + Utils.vecNameToString("centerAreaRot") + "," +
                                                Utils.vecNameToString("headPos") + "," + Utils.vecNameToString("headRot") + "," + Utils.vecNameToString("rightHandPos") + Utils.vecNameToString("rightHandRot") + "," +
                                                Utils.vecNameToString("leftHandPos") + Utils.vecNameToString("leftHandRot") + "partId," + Utils.vecNameToString("objInteractedPos") + "," + Utils.vecNameToString("objInteractedRot") + "\n";

        public string player1InteractionStr = "userId,dominantPlayer,timestamp,collabType,currentTask,dominantPlayer,isViolatingBoundary," + Utils.vecNameToString("Player1AreaCenter") + "," + Utils.vecNameToString("Player1AreaRot")
                                + Utils.vecNameToString("headPosP1") + "," + Utils.vecNameToString("headRotP1") + "," + Utils.vecNameToString("rightHandPosP1") + Utils.vecNameToString("rightHandRotP1")
                                 + Utils.vecNameToString("leftHandPosP1") + "," + Utils.vecNameToString("leftHandRotP1") + "\n";*/
        string stringToSave = header;
        string strAngled = header;
        string strF2FIntersect = header;
        string strF2FNo = header;
        string strCoupled = header;
        string strSide = header;
        bool lastWasFromAPressedEvent = false;
        bool isDominant = false;

        //stringToSave += "HEEADER";

        string[] dirs = Directory.GetDirectories(basePath, "G*", SearchOption.TopDirectoryOnly);

        GameObject gObj = new GameObject("test");
        
        //Console.WriteLine("The number of directories starting with p is {0}.", dirs.Length);
        foreach (string currentDir in dirs)
        {

            string[] userP = currentDir.Split('\\');//user count
            string user = userP[userP.Length - 1];

            string[] conditionsFromAUserDir = Directory.GetDirectories(currentDir);


            foreach (string conditionDir in conditionsFromAUserDir)
            {
                string[] files = Directory.GetFiles(conditionDir, "M*");
                foreach (string fileStr in files)
                {
                    System.IO.StreamReader filee = new System.IO.StreamReader(fileStr);

                    string[] splitStr = fileStr.Split('_');


                    //string passiveHaptics = splitStr[splitStr.Length - 2];
                    //string orientation = splitStr[splitStr.Length - 1];

                    //orientation = orientation.Split('.')[0];//remove extension

                    Vector3 calibratedPlanePos = Vector3.zero;
                    Vector3 calibratedPlaneAngles = Vector3.zero;



                    string line = "";
                    string condition = "";
                    string part = "";
                    string dominant = "";

                    while ((line = filee.ReadLine()) != null)
                    {

                        if (line.Split(',')[0].Contains("u"))
                        {
                            //do nothing - ignore headers
                        }
                        else
                        {
                            int currentTask =0 ;
                            
                            int countRows = 30;
                            string[] lineArray = line.Split(',');
                            try
                            {
                                int.TryParse(lineArray[4], out currentTask);

                            }
                            catch (Exception ex)
                            {
                                currentTask = 0;
                            }
                            if (lineArray.Length < 30)
                            {

                            }
                            else if(currentTask >=2)
                            {
                                dominant = lineArray[5];
                                
                                user = lineArray[0];
                                condition = lineArray[2];
                                part = lineArray[1];
                                bool.TryParse(lineArray[5], out isDominant);

                                centerBounds.transform.position = Utils.stringToVector3(lineArray[7] + "," + lineArray[8] + "," + lineArray[9], ',');
                                centerBounds.transform.eulerAngles = Utils.stringToVector3(lineArray[10] + "," + lineArray[11] + "," + lineArray[12], ',');

                                head.transform.position = Utils.stringToVector3(lineArray[13] + "," + lineArray[14] + "," + lineArray[15], ',');
                                head.transform.eulerAngles = Utils.stringToVector3(lineArray[16] + "," + lineArray[17] + "," + lineArray[18], ',');

                                string emaranhado = lineArray[24];
                                string tmp1 = "";
                                string tmp2 = "";
                                if (emaranhado.Contains("-"))
                                {
                                    tmp1 = emaranhado.Split('-')[0];
                                    tmp2 = emaranhado.Split('-')[1];
                                }
                                else
                                {
                                    int indexOf = emaranhado.LastIndexOf('.');
                                    //emaranhado = emaranhado.Substring(0, indexOf - 1);


                                    //indexOf += 6;
                                    try
                                    {
                                        tmp1 = emaranhado.Substring(0, indexOf - 1);
                                        tmp2 = emaranhado.Substring(indexOf);
                                        // emaranhado = emaranhado.Substring(0, emaranhado.Length - 2);
                                    }
                                    catch (Exception ex)
                                    {
                                        print("bla");
                                    }
                                    //print(emaranhado.Length + "," + emaranhado[emaranhado.Length - 1] + "["+indexOf + ","+ (emaranhado.Length-1)+ "]");

                                }
                                rightHand.transform.position = Utils.stringToVector3(lineArray[19] + "," + lineArray[20] + "," + lineArray[21], ',');
                                rightHand.transform.eulerAngles = Utils.stringToVector3(lineArray[22] + "," + lineArray[23] + "," + tmp1, ',');

                                leftHand.transform.position = Utils.stringToVector3(tmp2 + "," + lineArray[25] + "," + lineArray[26], ',');
                                leftHand.transform.eulerAngles = Utils.stringToVector3(lineArray[27] + "," + lineArray[28] + "," + lineArray[29], ',');

                                head.transform.parent = centerBounds.transform;
                                float currentTimestamp = 0;

                                float.TryParse(lineArray[3], out currentTimestamp);


                                rightHand.transform.parent = centerBounds.transform;
                                leftHand.transform.parent = centerBounds.transform;
                                


                                Vector3 headLocal = new Vector3(Mathf.Abs(head.transform.localPosition.x), Mathf.Abs(head.transform.localPosition.y), Mathf.Abs(head.transform.localPosition.z));
                                Vector3 rightHandLocal = new Vector3(Mathf.Abs(rightHand.transform.localPosition.x), Mathf.Abs(rightHand.transform.localPosition.y), Mathf.Abs(rightHand.transform.localPosition.z));
                                Vector3 leftHandLocal = new Vector3(Mathf.Abs(leftHand.transform.localPosition.x), Mathf.Abs(leftHand.transform.localPosition.y), Mathf.Abs(leftHand.transform.localPosition.z));



                                bool isViolatingBoundary = false;

                                if (!isDominant)
                                {
                                    if (Utils.IsViolatingBoundary(rightHandLocal, boundsSize) || Utils.IsViolatingBoundary(leftHandLocal, boundsSize) || Utils.IsViolatingBoundary(headLocal, boundsSize))
                                    {
                                        lastFrameTimestamp = currentTimestamp;
                                        isViolatingBoundary = true;
                                        //timeOutsideBounds += lastFrameTimestamp;
                                    }
                                    else
                                    {
                                        //currentTimestamp =
                                        isViolatingBoundary = false;
                                        timeOutsideBounds += (currentTimestamp - lastFrameTimestamp);
                                        lastFrameTimestamp = currentTimestamp;
                                    }

                                }
                                else
                                {
                                    lastFrameTimestamp = 0;
                                }
                                

                                dominant = part;
                                int userId = 0;
                                int.TryParse(user, out userId);
                                if (userId % 2 == 0)
                                {
                                    part = "P2";
                                }
                                else
                                    part = "P1";
                                /*
                                if(condition == "Angled90")
                                //if (condition == "SideBySide")
                                {
                                    centerBounds.transform.position = Utils.stringToVector3(lineArray[7] + "," + lineArray[8] + "," + lineArray[9], ',');
                                    if (part == "P2")
                                        centerBounds.transform.localEulerAngles = new Vector3(0, 270, 0);
                                    else
                                        centerBounds.transform.localEulerAngles = new Vector3(0, 0, 0);
                                    head.transform.position = Utils.stringToVector3(lineArray[13] + "," + lineArray[14] + "," + lineArray[15], ',');
                                    head.transform.eulerAngles = Utils.stringToVector3(lineArray[16] + "," + lineArray[17] + "," + lineArray[18], ',');
                                    head.transform.parent = centerBounds.transform;
                                    Vector3 auxHeadPos = head.transform.position;
                                    Vector3 auxHeadRot = head.transform.eulerAngles;
                                    head.transform.position = new Vector3(0, 0, 0);
                                    if(part == "P1")
                                        head.transform.localEulerAngles = new Vector3(0, head.transform.localEulerAngles.y, 0);
                                    else
                                        head.transform.localEulerAngles = new Vector3(0, head.transform.localEulerAngles.y -90.0f, 0);

                                    gObj.transform.parent = head.transform;
                                    gObj.transform.eulerAngles = Vector3.zero;
                                    gObj.transform.localPosition = Vector3.zero;
                                    gObj.transform.localPosition = new Vector3(0, 0, 0.5f);
                                    gObj.transform.parent = null;
                                    //head.transform.position = auxHeadPos;
                                    //head.transform.eulerAngles = auxHeadRot;

                                    
                                        strAngled += user + "," + dominant + "," + currentTimestamp + "," + condition + "," + Utils.vector3ToString(centerBounds.transform.position) + "," + Utils.vector3ToString(centerBounds.transform.eulerAngles) + "," +
                                     Utils.vector3ToString(head.transform.localPosition) + "," + Utils.vector3ToString(head.transform.localEulerAngles) + "," +
                                                    Utils.vector3ToString(rightHand.transform.localPosition) + "," + Utils.vector3ToString(rightHand.transform.localEulerAngles) + "," +
                                                    Utils.vector3ToString(leftHand.transform.localPosition) + "," + Utils.vector3ToString(leftHand.transform.localEulerAngles) + "," +
                                                    Utils.vector3ToString(gObj.transform.localPosition) + "," + isViolatingBoundary + "\n";


        

                                    if (strAngled.Length > 400)
                                    {
                                        System.IO.File.AppendAllText(basePath + "reportForHeatmap_Angled90.csv", strAngled);
                                        strAngled = "";
                                    }

                                }*/
                                if (condition == "SideBySide")
                                //if (condition == "SideBySide")
                                {
                                    centerBounds.transform.position = Utils.stringToVector3(lineArray[7] + "," + lineArray[8] + "," + lineArray[9], ',');
                                    centerBounds.transform.eulerAngles = Utils.stringToVector3(lineArray[10] + "," + lineArray[11] + "," + lineArray[12], ',');

                                    head.transform.position = Utils.stringToVector3(lineArray[13] + "," + lineArray[14] + "," + lineArray[15], ',');
                                    head.transform.eulerAngles = Utils.stringToVector3(lineArray[16] + "," + lineArray[17] + "," + lineArray[18], ',');
                                    head.transform.parent = centerBounds.transform;
                                    Vector3 auxHeadPos = head.transform.position;
                                    Vector3 auxHeadRot = head.transform.eulerAngles;
                                    head.transform.position = new Vector3(0, 0, 0);
                                    head.transform.localEulerAngles = new Vector3(0, head.transform.localEulerAngles.y, 0);

                                    gObj.transform.parent = head.transform;
                                    gObj.transform.eulerAngles = Vector3.zero;
                                    gObj.transform.localPosition = Vector3.zero;
                                    gObj.transform.localPosition = new Vector3(0, 0, 0.5f);
                                    gObj.transform.parent = null;
                                    head.transform.position = auxHeadPos;
                                    head.transform.eulerAngles = auxHeadRot;

                                    
                                        strSide += user + "," + dominant + "," + currentTimestamp + "," + condition + "," + Utils.vector3ToString(centerBounds.transform.position) + "," + Utils.vector3ToString(centerBounds.transform.eulerAngles) + "," +
                                     Utils.vector3ToString(head.transform.localPosition) + "," + Utils.vector3ToString(head.transform.localEulerAngles) + "," +
                                                    Utils.vector3ToString(rightHand.transform.localPosition) + "," + Utils.vector3ToString(rightHand.transform.localEulerAngles) + "," +
                                                    Utils.vector3ToString(leftHand.transform.localPosition) + "," + Utils.vector3ToString(leftHand.transform.localEulerAngles) + "," +
                                                    Utils.vector3ToString(gObj.transform.localPosition) + "," + isViolatingBoundary + "\n";
                                    
                                    if (strSide.Length > 400)
                                    {
                                        System.IO.File.AppendAllText(basePath + "reportForHeatmap_SideBySide.csv", strSide);
                                        strSide = "";
                                    }

                                }/*
                                else if (condition == "FacetoFaceIntersect")
                                //if (condition == "SideBySide")
                                {
                                    centerBounds.transform.position = Utils.stringToVector3(lineArray[7] + "," + lineArray[8] + "," + lineArray[9], ',');
                                    if (part == "P2")
                                        centerBounds.transform.localEulerAngles = new Vector3(0, 180, 0);
                                    else
                                        centerBounds.transform.localEulerAngles = new Vector3(0, 0, 0);



                                    head.transform.position = Utils.stringToVector3(lineArray[13] + "," + lineArray[14] + "," + lineArray[15], ',');
                                    head.transform.eulerAngles = Utils.stringToVector3(lineArray[16] + "," + lineArray[17] + "," + lineArray[18], ',');
                                    head.transform.parent = centerBounds.transform;
                                    Vector3 auxHeadPos = head.transform.position;
                                    Vector3 auxHeadRot = head.transform.eulerAngles;
                                    head.transform.position = new Vector3(0, 0, 0);

                                    if (part == "P1")
                                        head.transform.localEulerAngles = new Vector3(0, head.transform.localEulerAngles.y, 0);
                                    else
                                        head.transform.localEulerAngles = new Vector3(0, head.transform.localEulerAngles.y - 180, 0);
                                    gObj.transform.parent = head.transform;
                                    gObj.transform.eulerAngles = Vector3.zero;
                                    gObj.transform.localPosition = Vector3.zero;
                                    gObj.transform.localPosition = new Vector3(0, 0, 0.5f);
                                    gObj.transform.parent = null; 
                                    
                                    head.transform.position = auxHeadPos;
                                    head.transform.eulerAngles = auxHeadRot;

                                    
                                        strF2FIntersect += user + "," + dominant + "," + currentTimestamp + "," + condition + "," + Utils.vector3ToString(centerBounds.transform.position) + "," + Utils.vector3ToString(centerBounds.transform.eulerAngles) + "," +
                                     Utils.vector3ToString(head.transform.localPosition) + "," + Utils.vector3ToString(head.transform.localEulerAngles) + "," +
                                                    Utils.vector3ToString(rightHand.transform.localPosition) + "," + Utils.vector3ToString(rightHand.transform.localEulerAngles) + "," +
                                                    Utils.vector3ToString(leftHand.transform.localPosition) + "," + Utils.vector3ToString(leftHand.transform.localEulerAngles) + "," +
                                                    Utils.vector3ToString(gObj.transform.localPosition) + "," + isViolatingBoundary + "\n";
                                    
                                    if (strF2FIntersect.Length > 400)
                                    {
                                        System.IO.File.AppendAllText(basePath + "reportForHeatmap_FacetoFaceIntersect.csv", strF2FIntersect);
                                        strF2FIntersect = "";
                                    }

                                }
                                if (condition == "FaceToFaceNoIntersect")
                                {
                                    centerBounds.transform.position = Utils.stringToVector3(lineArray[7] + "," + lineArray[8] + "," + lineArray[9], ',');
                                    if (part == "P2")
                                        centerBounds.transform.localEulerAngles = new Vector3(0, 180, 0);
                                    else
                                        centerBounds.transform.localEulerAngles = new Vector3(0, 0, 0);
                                    head.transform.position = Utils.stringToVector3(lineArray[13] + "," + lineArray[14] + "," + lineArray[15], ',');
                                    head.transform.eulerAngles = Utils.stringToVector3(lineArray[16] + "," + lineArray[17] + "," + lineArray[18], ',');
                                    head.transform.parent = centerBounds.transform;
                                    Vector3 centerBoundsPos = centerBounds.transform.position;
                                    Vector3 auxHeadPos = head.transform.position;
                                    Vector3 auxHeadRot = head.transform.eulerAngles;
                                    head.transform.position = new Vector3(0, 0, 0);
                                    

                                    head.transform.localEulerAngles = new Vector3(0, head.transform.localEulerAngles.y, 0);
                                    gObj.transform.parent = head.transform;
                                    if (head.transform.localEulerAngles.y > 90)
                                    {
                                        string str = ""+head.transform.localEulerAngles.y;
                                    }
                                    if (part == "P1")
                                        head.transform.localEulerAngles = new Vector3(0, head.transform.localEulerAngles.y, 0);
                                    else
                                        head.transform.localEulerAngles = new Vector3(0, head.transform.localEulerAngles.y - 180, 0);
                                    
                                    
                                    gObj.transform.localEulerAngles = Vector3.zero;
                                    //gObj.transform.localPosition = Vector3.zero;

                                    gObj.transform.localPosition = new Vector3(0, 0, 0.5f);
                                    gObj.transform.parent = null; 
                                    
                                    head.transform.position = auxHeadPos;
                                    head.transform.eulerAngles = auxHeadRot;

                                   
                                     strF2FNo += user + "," + dominant + "," + currentTimestamp + "," + condition + "," + Utils.vector3ToString(centerBounds.transform.position) + "," + Utils.vector3ToString(centerBounds.transform.eulerAngles) + "," +
                                     Utils.vector3ToString(head.transform.localPosition) + "," + Utils.vector3ToString(head.transform.localEulerAngles) + "," +
                                                    Utils.vector3ToString(rightHand.transform.localPosition) + "," + Utils.vector3ToString(rightHand.transform.localEulerAngles) + "," +
                                                    Utils.vector3ToString(leftHand.transform.localPosition) + "," + Utils.vector3ToString(leftHand.transform.localEulerAngles) + "," +
                                                    Utils.vector3ToString(gObj.transform.localPosition) + "\n";
                                    
                                    if (strF2FNo.Length > 400)
                                    {
                                        System.IO.File.AppendAllText(basePath + "reportForHeatmap_FacetoFaceNoIntersect.csv", strF2FNo);
                                        strF2FNo = "";
                                    }

                                }
                                else if (condition == "CoupledView")
                                {
                                    centerBounds.transform.position = Utils.stringToVector3(lineArray[7] + "," + lineArray[8] + "," + lineArray[9], ',');
                                    centerBounds.transform.eulerAngles = Utils.stringToVector3(lineArray[10] + "," + lineArray[11] + "," + lineArray[12], ',');

                                    head.transform.position = Utils.stringToVector3(lineArray[13] + "," + lineArray[14] + "," + lineArray[15], ',');
                                    head.transform.eulerAngles = Utils.stringToVector3(lineArray[16] + "," + lineArray[17] + "," + lineArray[18], ',');
                                    head.transform.parent = centerBounds.transform;
                                    Vector3 auxHeadPos = head.transform.position;
                                    Vector3 auxHeadRot = head.transform.eulerAngles;
                                    head.transform.localPosition = new Vector3(0, 0, 0);
                                    head.transform.localEulerAngles = new Vector3(0, head.transform.localEulerAngles.y, 0);

                                    gObj.transform.parent = head.transform;
                                    gObj.transform.eulerAngles = Vector3.zero;
                                    gObj.transform.localPosition = Vector3.zero;
                                    gObj.transform.localPosition = new Vector3(0, 0, 0.5f);
                                    gObj.transform.parent = null;
                                    head.transform.position = auxHeadPos;
                                    head.transform.eulerAngles = auxHeadRot;

                                    if (head.transform.localEulerAngles.y > 90 && head.transform.localEulerAngles.y < 270)
                                    {

                                    }
                                    else
                                    {
                                        strCoupled += user + "," + dominant + "," + currentTimestamp + "," + condition + "," + Utils.vector3ToString(centerBounds.transform.position) + "," + Utils.vector3ToString(centerBounds.transform.eulerAngles) + "," +
                                     Utils.vector3ToString(head.transform.localPosition) + "," + Utils.vector3ToString(head.transform.localEulerAngles) + "," +
                                                    Utils.vector3ToString(rightHand.transform.localPosition) + "," + Utils.vector3ToString(rightHand.transform.localEulerAngles) + "," +
                                                    Utils.vector3ToString(leftHand.transform.localPosition) + "," + Utils.vector3ToString(leftHand.transform.localEulerAngles) + "," +
                                                    Utils.vector3ToString(gObj.transform.localPosition) + "," + isViolatingBoundary + "\n";
                                    }
                                    if (strCoupled.Length > 400)
                                    {
                                        System.IO.File.AppendAllText(basePath + "reportForHeatmap_CoupledView.csv", strCoupled);
                                        strCoupled = "";
                                    }

                                }*/

                            }

                        }


                    }
                    //stringToSave += user + "," + condition + "," + dominant + "," + timeOutsideBounds + "\n";
                    
                    //stringToSave += (user + "," + passiveHaptics + "," + orientation + "," + Utils.vector3ToString(calibratedPlanePos) + "," + Utils.vector3ToString(calibratedPlaneAngles) + "\n");

                    //System.IO.File.AppendAllText(basePath + "reportTimeGazeViolationNoDominantew.csv", stringToSave);
                }
            }



        }

        Debug.Log("terminou");


    }


    void processTotalTIme()
    {
        GameObject rootObjects = new GameObject("RootAreas");

        GameObject Player1Area = new GameObject("Player1Area");
        GameObject Player2Area = new GameObject("Player2Area");

        Player1Area.transform.parent = rootObjects.transform;
        Player2Area.transform.parent = rootObjects.transform;

        rootObjects.transform.localPosition = new Vector3(0, 0.471f, -0.372f);

        Player1Area.transform.localPosition = new Vector3(0, 0, 0);
        Player2Area.transform.localPosition = new Vector3(0, 0, 0.49f);//size 0.5,1,0.8f
        Player2Area.transform.localEulerAngles = new Vector3(0, 180, 0);

        Vector3 boundsSize = new Vector3(0.5f, 1, 0.8f);

        GameObject head = new GameObject("head");
        GameObject rightHand = new GameObject("rightHand");
        GameObject leftHand = new GameObject("leftHand");
        GameObject centerBounds = new GameObject("centerBounds");

        Vector3 sizeBounds = new Vector3(1.0f, 0.5f, 0.5f);

        float timeOutsideBounds = 0;
        float lastFrameTimestamp = 0;

        string line = "";
        string condition = "";
        string part = "";
        string dominant = "";

        string basePath = Directory.GetCurrentDirectory() + "\\LogFiles\\";
        // Read the file and display it line by line.  
        string fileName = "allIn.csv";
        //string header = "UserId,collaborationType,dominantPlayer,currentTask,NameBlueprintObj,blueprintPosX,blueprintPosY,blueprintPosZ,NameUserPlacedObj,UserPlacedPosX,UserPlacedPosY,UserPlacedPosZ,AbsoluteDistance,RelativeDistanceX,RelativeDistanceY,RelativeDistanceZ,correctObject\n";
        string header = "groupID,userId,collabType,trialNumber,dominantPlayer,puzzleId,centerPosAreaX,centerPosAreaY, centerPosAreaZ, centerRotAreaX, centerRotAreaY, centerRotAreaZ, Angled90, boundsSizeX, boundsSizeY, boundsSizeZ, numberOfBoundViolationsP1, timeOutsideBoundsP1, numberOfBoundViolationsP2, timeOutsideBoundsP2, totalTime, isTraining\n";

        string stringToSave = header;
        bool lastWasFromAPressedEvent = false;

        //stringToSave += "HEEADER";

        string[] dirs = Directory.GetDirectories(basePath, "G*", SearchOption.TopDirectoryOnly);

        //int[] countAccuracyArray = new int[30];

        //Console.WriteLine("The number of directories starting with p is {0}.", dirs.Length);
        foreach (string currentDir in dirs)
        {

            string[] userP = currentDir.Split('\\');//user count
            string user = userP[userP.Length - 1];

            string[] conditionsFromAUserDir = Directory.GetDirectories(currentDir);

            int lastTaskID = -1;
            int countAccuracy = 0;

            foreach (string conditionDir in conditionsFromAUserDir)
            {
                string[] files = Directory.GetFiles(conditionDir, "TaskReport_*");
                foreach (string fileStr in files)
                {
                    System.IO.StreamReader filee = new System.IO.StreamReader(fileStr);

                    string[] splitStr = fileStr.Split('_');


                    //string passiveHaptics = splitStr[splitStr.Length - 2];
                    //string orientation = splitStr[splitStr.Length - 1];

                    //orientation = orientation.Split('.')[0];//remove extension

                    Vector3 calibratedPlanePos = Vector3.zero;
                    Vector3 calibratedPlaneAngles = Vector3.zero;


                    int numberCollisionsDominant = 0;
                    int numberCollisionsNotDominant = 0;
                    float timeCollisionsDominant = 0;
                    float timeCollisionsNotDominant = 0;
                    float distanceDominant = 0;
                    float distanceNotDominant = 0;
                    int taskID = 0;

                    while ((line = filee.ReadLine()) != null)
                    {

                        if (line.Split(',')[0].Contains("u"))
                        {
                            //do nothing - ignore headers
                        }
                        else
                        {

                            int countRows = 30;
                            string[] lineArray = line.Split(',');

                            //int.TryParse(lineArray,out t
                            //userId collabType  currentTask bPart   distance timeElapsed


                            int.TryParse(lineArray[4], out taskID);
                            

                                user = lineArray[0];
                                condition = lineArray[1];
                                dominant = "";// lineArray[2];
                                float distance = 0;
                                float timeElapsed = 0;

                                float.TryParse(lineArray[19], out distance);
                                float.TryParse(lineArray[5], out timeElapsed);
                                int userID = 0;
                                int.TryParse(lineArray[0], out userID);
                                bool isDominant = false;

                                int currentTask = (userID % 2);

                            int groupID = 0;
                            int.TryParse(user, out groupID);

                            if (groupID % 2 == 0)
                            {
                                groupID = groupID / 2;
                            }
                            else
                            {
                                groupID = (groupID + 1) / 2;
                            }
                            stringToSave += groupID + "," + line + "\n";// "," + taskID + "," + condition + "," + numberCollisionsDominant + "," + timeCollisionsDominant + "," + distanceDominant + "," + numberCollisionsNotDominant + "," +

                        }

                        
                    }

                    
                    //    timeCollisionsNotDominant + "," + distanceNotDominant + "\n";
                    System.IO.File.AppendAllText(basePath + "reportTime.csv", stringToSave);

                    //stringToSave += (user + "," + passiveHaptics + "," + orientation + "," + Utils.vector3ToString(calibratedPlanePos) + "," + Utils.vector3ToString(calibratedPlaneAngles) + "\n");

                    //System.IO.File.AppendAllText(basePath + "reportTimeGazeViolationNoDominantew.csv", stringToSave);
                    stringToSave = "";
                }
            }



        }

        Debug.Log("terminou");

    }

    void processCollisions2()
    {
        GameObject rootObjects = new GameObject("RootAreas");

        GameObject Player1Area = new GameObject("Player1Area");
        GameObject Player2Area = new GameObject("Player2Area");

        Player1Area.transform.parent = rootObjects.transform;
        Player2Area.transform.parent = rootObjects.transform;

        rootObjects.transform.localPosition = new Vector3(0, 0.471f, -0.372f);

        Player1Area.transform.localPosition = new Vector3(0, 0, 0);
        Player2Area.transform.localPosition = new Vector3(0, 0, 0.49f);//size 0.5,1,0.8f
        Player2Area.transform.localEulerAngles = new Vector3(0, 180, 0);

        Vector3 boundsSize = new Vector3(0.5f, 1, 0.8f);

        GameObject head = new GameObject("head");
        GameObject rightHand = new GameObject("rightHand");
        GameObject leftHand = new GameObject("leftHand");
        GameObject centerBounds = new GameObject("centerBounds");

        Vector3 sizeBounds = new Vector3(1.0f, 0.5f, 0.5f);

        float timeOutsideBounds = 0;
        float lastFrameTimestamp = 0;

        string line = "";
        string condition = "";
        string part = "";
        string dominant = "";

        string basePath = Directory.GetCurrentDirectory() + "\\LogFiles\\";
        // Read the file and display it line by line.  
        string fileName = "allIn.csv";
        //string header = "UserId,collaborationType,dominantPlayer,currentTask,NameBlueprintObj,blueprintPosX,blueprintPosY,blueprintPosZ,NameUserPlacedObj,UserPlacedPosX,UserPlacedPosY,UserPlacedPosZ,AbsoluteDistance,RelativeDistanceX,RelativeDistanceY,RelativeDistanceZ,correctObject\n";
        string header = "User,taskID,numberCollisionsDominant,timeCollisionsDominant,distanceDominant,numberCollisionsNotDominant,timeCollisionsNotDominant,distanceNotDominant\n";
        string stringToSave = header;
        bool lastWasFromAPressedEvent = false;

        //stringToSave += "HEEADER";

        string[] dirs = Directory.GetDirectories(basePath, "G*", SearchOption.TopDirectoryOnly);

        //int[] countAccuracyArray = new int[30];

        //Console.WriteLine("The number of directories starting with p is {0}.", dirs.Length);
        foreach (string currentDir in dirs)
        {

            string[] userP = currentDir.Split('\\');//user count
            string user = userP[userP.Length - 1];

            string[] conditionsFromAUserDir = Directory.GetDirectories(currentDir);

            int lastTaskID = -1;
            int countAccuracy = 0;

            foreach (string conditionDir in conditionsFromAUserDir)
            {
                string[] files = Directory.GetFiles(conditionDir, "TaskReportCollisions*");
                foreach (string fileStr in files)
                {
                    System.IO.StreamReader filee = new System.IO.StreamReader(fileStr);

                    string[] splitStr = fileStr.Split('_');


                    //string passiveHaptics = splitStr[splitStr.Length - 2];
                    //string orientation = splitStr[splitStr.Length - 1];

                    //orientation = orientation.Split('.')[0];//remove extension

                    Vector3 calibratedPlanePos = Vector3.zero;
                    Vector3 calibratedPlaneAngles = Vector3.zero;

                   
                    int numberCollisionsDominantTask2 = 0;
                    int numberCollisionsNotDominantTask2 = 0;
                    float timeCollisionsDominantTask2 = 0;
                    float timeCollisionsNotDominantTask2 = 0;
                    float distanceDominantTask2 = 0;
                    float distanceNotDominantTask2 = 0;

                    int numberCollisionsDominantTask3 = 0;
                    int numberCollisionsNotDominantTask3 = 0;
                    float timeCollisionsDominantTask3 = 0;
                    float timeCollisionsNotDominantTask3 = 0;
                    float distanceDominantTask3 = 0;
                    float distanceNotDominantTask3 = 0;
                    int taskID = 0;

                    while ((line = filee.ReadLine()) != null)
                    {

                        if (line.Split(',')[0].Contains("U"))
                        {
                            //do nothing - ignore headers
                        }
                        else
                        {

                            int countRows = 30;
                            string[] lineArray = line.Split(',');
                            
                            //int.TryParse(lineArray,out t
                            //userId collabType  currentTask bPart   distance timeElapsed


                            int.TryParse(lineArray[2], out taskID);
                            if (taskID >= 2)
                            {

                                user = lineArray[0];
                                condition = lineArray[1];
                                dominant = "";// lineArray[2];
                                float distance = 0;
                                float timeElapsed = 0;

                                float.TryParse(lineArray[4], out distance);
                                float.TryParse(lineArray[5], out timeElapsed);
                                int userID = 0;
                                int.TryParse(lineArray[0], out userID);
                                bool isDominant = false;

                                int currentTask = (userID % 2);

                                if (taskID == 2 && currentTask == 1)
                                    isDominant = true;// dominant = "TRUE";
                                else if (taskID == 3 && currentTask == 0)
                                    isDominant = true;
                                else
                                    isDominant = false;
                                if(taskID == 2)
                                {
                                    if (isDominant)
                                    {
                                        distanceDominantTask2 += distance;
                                        timeCollisionsDominantTask2 += timeElapsed;
                                        numberCollisionsDominantTask2++;
                                    }
                                    else
                                    {
                                        distanceNotDominantTask2 += distance;
                                        timeCollisionsNotDominantTask2 += timeElapsed;
                                        numberCollisionsNotDominantTask2++;
                                    }
                                }
                                if (taskID == 3)
                                {
                                    if (isDominant)
                                    {
                                        distanceDominantTask3 += distance;
                                        timeCollisionsDominantTask3 += timeElapsed;
                                        numberCollisionsDominantTask3++;
                                    }
                                    else
                                    {
                                        distanceNotDominantTask3 += distance;
                                        timeCollisionsNotDominantTask3 += timeElapsed;
                                        numberCollisionsNotDominantTask3++;
                                    }
                                }
                            }
                                
                            }


                        


                    }
                    int groupID = 0;
                    int.TryParse(user, out groupID);

                    if(groupID %2 == 0)
                    {
                        groupID = groupID / 2;
                    }
                    else
                    {
                        groupID = (groupID + 1) / 2;
                    }

                    stringToSave += groupID + "," + 2 + "," + condition + ","+numberCollisionsDominantTask2 + "," + timeCollisionsDominantTask2 + "," + distanceDominantTask2 + "," + numberCollisionsNotDominantTask2 + "," +
                        timeCollisionsNotDominantTask2 + "," + distanceNotDominantTask2 + "\n";

                    stringToSave += groupID + "," + 3 + "," + condition + "," + numberCollisionsDominantTask3 + "," + timeCollisionsDominantTask3 + "," + distanceDominantTask3 + "," + numberCollisionsNotDominantTask3 + "," +
                        timeCollisionsNotDominantTask3 + "," + distanceNotDominantTask3 + "\n";
                    System.IO.File.AppendAllText(basePath + "reportCollision.csv", stringToSave);

                    //stringToSave += (user + "," + passiveHaptics + "," + orientation + "," + Utils.vector3ToString(calibratedPlanePos) + "," + Utils.vector3ToString(calibratedPlaneAngles) + "\n");

                    //System.IO.File.AppendAllText(basePath + "reportTimeGazeViolationNoDominantew.csv", stringToSave);
                    stringToSave = "";
                }
            }



        }

        Debug.Log("terminou");


    }

    void processAccuracy()
    {
        GameObject rootObjects = new GameObject("RootAreas");

        GameObject Player1Area = new GameObject("Player1Area");
        GameObject Player2Area = new GameObject("Player2Area");

        Player1Area.transform.parent = rootObjects.transform;
        Player2Area.transform.parent = rootObjects.transform;

        rootObjects.transform.localPosition = new Vector3(0, 0.471f, -0.372f);

        Player1Area.transform.localPosition = new Vector3(0, 0, 0);
        Player2Area.transform.localPosition = new Vector3(0, 0, 0.49f);//size 0.5,1,0.8f
        Player2Area.transform.localEulerAngles = new Vector3(0, 180, 0);

        Vector3 boundsSize = new Vector3(0.5f, 1, 0.8f);

        GameObject head = new GameObject("head");
        GameObject rightHand = new GameObject("rightHand");
        GameObject leftHand = new GameObject("leftHand");
        GameObject centerBounds = new GameObject("centerBounds");

        Vector3 sizeBounds = new Vector3(1.0f, 0.5f, 0.5f);

        float timeOutsideBounds = 0;
        float lastFrameTimestamp = 0;

        string line = "";
        string condition = "";
        string part = "";
        string dominant = "";

        string basePath = Directory.GetCurrentDirectory() + "\\LogFiles\\";
        // Read the file and display it line by line.  
        string fileName = "allIn.csv";
        //string header = "UserId,collaborationType,dominantPlayer,currentTask,NameBlueprintObj,blueprintPosX,blueprintPosY,blueprintPosZ,NameUserPlacedObj,UserPlacedPosX,UserPlacedPosY,UserPlacedPosZ,AbsoluteDistance,RelativeDistanceX,RelativeDistanceY,RelativeDistanceZ,correctObject\n";
        string header = "UserId,condition,dominant,numberAccuratePuzzles\n";
            //; "Id,Condition,Dominant,TimeInsideGazeBounds,TimeSecondzone,TimeOutsideGazeBounds,TotalTime,PercentageInsideGazeBounds,PercentageSecondZone,PercentageOutsideGazeBounds\n";
        /*public string headerPlayer1Interaction = "userId,currentTask,dominantPlayer," + Utils.vecNameToString("centerBoundsPos") + "," + Utils.vecNameToString("centerAreaRot") + "," +
                                                Utils.vecNameToString("headPos") + "," + Utils.vecNameToString("headRot") + "," + Utils.vecNameToString("rightHandPos") + Utils.vecNameToString("rightHandRot") + "," +
                                                Utils.vecNameToString("leftHandPos") + Utils.vecNameToString("leftHandRot") + "partId," + Utils.vecNameToString("objInteractedPos") + "," + Utils.vecNameToString("objInteractedRot") + "\n";

        public string player1InteractionStr = "userId,dominantPlayer,timestamp,collabType,currentTask,dominantPlayer,isViolatingBoundary," + Utils.vecNameToString("Player1AreaCenter") + "," + Utils.vecNameToString("Player1AreaRot")
                                + Utils.vecNameToString("headPosP1") + "," + Utils.vecNameToString("headRotP1") + "," + Utils.vecNameToString("rightHandPosP1") + Utils.vecNameToString("rightHandRotP1")
                                 + Utils.vecNameToString("leftHandPosP1") + "," + Utils.vecNameToString("leftHandRotP1") + "\n";*/
        string stringToSave = header;
        bool lastWasFromAPressedEvent = false;

        //stringToSave += "HEEADER";

        string[] dirs = Directory.GetDirectories(basePath, "G*", SearchOption.TopDirectoryOnly);

        //int[] countAccuracyArray = new int[30];

        //Console.WriteLine("The number of directories starting with p is {0}.", dirs.Length);
        foreach (string currentDir in dirs)
        {

            string[] userP = currentDir.Split('\\');//user count
            string user = userP[userP.Length - 1];

            string[] conditionsFromAUserDir = Directory.GetDirectories(currentDir);

            int lastTaskID = -1;
            int countAccuracy = 0;

            foreach (string conditionDir in conditionsFromAUserDir)
            {
                string[] files = Directory.GetFiles(conditionDir, "TaskReportAccuracy*");
                foreach (string fileStr in files)
                {
                    System.IO.StreamReader filee = new System.IO.StreamReader(fileStr);

                    string[] splitStr = fileStr.Split('_');


                    //string passiveHaptics = splitStr[splitStr.Length - 2];
                    //string orientation = splitStr[splitStr.Length - 1];

                    //orientation = orientation.Split('.')[0];//remove extension

                    Vector3 calibratedPlanePos = Vector3.zero;
                    Vector3 calibratedPlaneAngles = Vector3.zero;




                    while ((line = filee.ReadLine()) != null)
                    {

                        if (line.Split(',')[0].Contains("U"))
                        {
                            //do nothing - ignore headers
                        }
                        else
                        {

                            int countRows = 30;
                            string[] lineArray = line.Split(',');
                            int taskID = 0;

                            
                            int.TryParse(lineArray[3], out taskID);
                            if(taskID >= 2)
                            {

                                user = lineArray[0];
                                condition = lineArray[1];
                                dominant = lineArray[2];
                                bool isDominant = false;
                                if (taskID == 2 && dominant == "P1")
                                    isDominant = true;// dominant = "TRUE";
                                else if (taskID == 3 && dominant == "P2")
                                    isDominant = true;
                                else
                                    isDominant = false;

                                
                                    if (lastTaskID == taskID)
                                    {
                                        bool stuff = false;
                                        bool.TryParse(lineArray[lineArray.Length - 1], out stuff);
                                        if (stuff == true)
                                        {
                                            countAccuracy++;
                                        }
                                    }
                                    else if(lastTaskID > 0)
                                    {
                                        string lastDominant = "";
                                        if (dominant == "P1")
                                            lastDominant = "P2";
                                        else
                                            lastDominant = "P1";
                                        stringToSave += user + "," + condition + "," + lastTaskID + "," + lastDominant + "," + countAccuracy + "\n";
                                        countAccuracy = 0;
                                        bool stuff = false;
                                        bool.TryParse(lineArray[lineArray.Length - 1], out stuff);
                                        if (stuff == true)
                                        {
                                            countAccuracy++;
                                        }
                                    }
                                

                                lastTaskID = taskID;
                            }

                            
                        }


                    }
                    if(lastTaskID >= 2)
                    {
                        stringToSave += user + "," + condition + "," + lastTaskID + "," + dominant + "," + countAccuracy + "\n";
                    }
                    //stringToSave += user + "," + condition + "," + dominant + "," + timeOutsideBounds + "\n";
                    System.IO.File.AppendAllText(basePath + "reportAccuracy.csv", stringToSave);

                    //stringToSave += (user + "," + passiveHaptics + "," + orientation + "," + Utils.vector3ToString(calibratedPlanePos) + "," + Utils.vector3ToString(calibratedPlaneAngles) + "\n");

                    //System.IO.File.AppendAllText(basePath + "reportTimeGazeViolationNoDominantew.csv", stringToSave);
                    stringToSave = "";
                }
            }



        }

        Debug.Log("terminou");


    }


    void processCollisions()
    {
        GameObject rootObjects = new GameObject("RootAreas");

        GameObject Player1Area = new GameObject("Player1Area");
        GameObject Player2Area = new GameObject("Player2Area");

        Player1Area.transform.parent = rootObjects.transform;
        Player2Area.transform.parent = rootObjects.transform;

        rootObjects.transform.localPosition = new Vector3(0, 0.471f, -0.372f);

        Player1Area.transform.localPosition = new Vector3(0, 0, 0);
        Player2Area.transform.localPosition = new Vector3(0, 0, 0.49f);//size 0.5,1,0.8f
        Player2Area.transform.localEulerAngles = new Vector3(0, 180, 0);

        Vector3 boundsSize = new Vector3(0.5f, 1, 0.8f);

        GameObject head = new GameObject("head");
        GameObject rightHand = new GameObject("rightHand");
        GameObject leftHand = new GameObject("leftHand");
        GameObject centerBounds = new GameObject("centerBounds");

        Vector3 sizeBounds = new Vector3(1.0f, 0.5f, 0.5f);

        float timeOutsideBounds = 0;
        float lastFrameTimestamp = 0;


        string basePath = Directory.GetCurrentDirectory() + "\\LogFiles\\";
        // Read the file and display it line by line.  
        string fileName = "allIn.csv";
        string header = "Id,Condition,Dominant,TimeInsideGazeBounds,TimeSecondzone,TimeOutsideGazeBounds,TotalTime,PercentageInsideGazeBounds,PercentageSecondZone,PercentageOutsideGazeBounds\n";
        /*public string headerPlayer1Interaction = "userId,currentTask,dominantPlayer," + Utils.vecNameToString("centerBoundsPos") + "," + Utils.vecNameToString("centerAreaRot") + "," +
                                                Utils.vecNameToString("headPos") + "," + Utils.vecNameToString("headRot") + "," + Utils.vecNameToString("rightHandPos") + Utils.vecNameToString("rightHandRot") + "," +
                                                Utils.vecNameToString("leftHandPos") + Utils.vecNameToString("leftHandRot") + "partId," + Utils.vecNameToString("objInteractedPos") + "," + Utils.vecNameToString("objInteractedRot") + "\n";

        public string player1InteractionStr = "userId,dominantPlayer,timestamp,collabType,currentTask,dominantPlayer,isViolatingBoundary," + Utils.vecNameToString("Player1AreaCenter") + "," + Utils.vecNameToString("Player1AreaRot")
                                + Utils.vecNameToString("headPosP1") + "," + Utils.vecNameToString("headRotP1") + "," + Utils.vecNameToString("rightHandPosP1") + Utils.vecNameToString("rightHandRotP1")
                                 + Utils.vecNameToString("leftHandPosP1") + "," + Utils.vecNameToString("leftHandRotP1") + "\n";*/
        string stringToSave = header;
        bool lastWasFromAPressedEvent = false;

        stringToSave += "HEEADER";

        string[] dirs = Directory.GetDirectories(basePath, "G*", SearchOption.TopDirectoryOnly);

        //Console.WriteLine("The number of directories starting with p is {0}.", dirs.Length);
        foreach (string currentDir in dirs)
        {

            string[] userP = currentDir.Split('\\');//user count
            string user = userP[userP.Length - 1];

            string[] conditionsFromAUserDir = Directory.GetDirectories(currentDir);

           
            foreach (string conditionDir in conditionsFromAUserDir)
            {
                string[] files = Directory.GetFiles(conditionDir, "M*");
                foreach (string fileStr in files)
                {
                    System.IO.StreamReader filee = new System.IO.StreamReader(fileStr);

                    string[] splitStr = fileStr.Split('_');


                    //string passiveHaptics = splitStr[splitStr.Length - 2];
                    //string orientation = splitStr[splitStr.Length - 1];

                    //orientation = orientation.Split('.')[0];//remove extension

                    Vector3 calibratedPlanePos = Vector3.zero;
                    Vector3 calibratedPlaneAngles = Vector3.zero;



                    string line = "";
                    string condition = "";
                    string part = "";
                    string dominant = "";
                    timeOutsideBounds = 0; 
                    while ((line = filee.ReadLine()) != null)
                    {

                        if (line.Split(',')[0].Contains("u"))
                        {
                            //do nothing - ignore headers
                        }
                        else
                        {

                            int countRows = 30;
                            string[] lineArray = line.Split(',');
                            if (lineArray.Length < 30)
                            {

                            }
                            else
                            {
                                dominant = lineArray[5];
                                bool isDominant = false;
                                Boolean.TryParse(lineArray[5], out isDominant);

                                user = lineArray[0];
                                condition = lineArray[2];
                                part = lineArray[1];

                                centerBounds.transform.position = Utils.stringToVector3(lineArray[7] + "," + lineArray[8] + "," + lineArray[9], ',');
                                centerBounds.transform.eulerAngles = Utils.stringToVector3(lineArray[10] + "," + lineArray[11] + "," + lineArray[12], ',');

                                head.transform.position = Utils.stringToVector3(lineArray[13] + "," + lineArray[14] + "," + lineArray[15], ',');
                                head.transform.eulerAngles = Utils.stringToVector3(lineArray[16] + "," + lineArray[17] + "," + lineArray[18], ',');

                                string emaranhado = lineArray[24];
                                string tmp1 = "";
                                string tmp2 = "";
                                if (emaranhado.Contains("-"))
                                {
                                    tmp1 = emaranhado.Split('-')[0];
                                    tmp2 = emaranhado.Split('-')[1];
                                }
                                else
                                {
                                    int indexOf = emaranhado.LastIndexOf('.');
                                    //emaranhado = emaranhado.Substring(0, indexOf - 1);

                                    
                                    //indexOf += 6;
                                    try { 
                                    tmp1 = emaranhado.Substring(0, indexOf - 1);
                                    tmp2 = emaranhado.Substring(indexOf);
                                       // emaranhado = emaranhado.Substring(0, emaranhado.Length - 2);
                                    }
                                    catch(Exception ex)
                                    {
                                        print("bla");
                                    }
                                    //print(emaranhado.Length + "," + emaranhado[emaranhado.Length - 1] + "["+indexOf + ","+ (emaranhado.Length-1)+ "]");

                                }
                                rightHand.transform.position = Utils.stringToVector3(lineArray[19] + "," + lineArray[20] + "," + lineArray[21], ',');
                                rightHand.transform.eulerAngles = Utils.stringToVector3(lineArray[22] + "," + lineArray[23] + "," + tmp1, ',');

                                leftHand.transform.position = Utils.stringToVector3(tmp2 + "," + lineArray[25] + "," + lineArray[26], ',');
                                leftHand.transform.eulerAngles = Utils.stringToVector3(lineArray[27] + "," + lineArray[28] + "," + lineArray[29], ',');

                                head.transform.parent = centerBounds.transform;
                                float currentTimestamp = 0;

                                float.TryParse(lineArray[3], out currentTimestamp);

                                Vector3 headLocal = new Vector3(Mathf.Abs(head.transform.localPosition.x), Mathf.Abs(head.transform.localPosition.y), Mathf.Abs(head.transform.localPosition.z)); 
                                Vector3 rightHandLocal = new Vector3(Mathf.Abs(rightHand.transform.localPosition.x), Mathf.Abs(rightHand.transform.localPosition.y), Mathf.Abs(rightHand.transform.localPosition.z));
                                Vector3 leftHandLocal = new Vector3(Mathf.Abs(leftHand.transform.localPosition.x), Mathf.Abs(leftHand.transform.localPosition.y), Mathf.Abs(leftHand.transform.localPosition.z));
                                if (!isDominant)
                                {
                                    if (Utils.IsViolatingBoundary(rightHandLocal, boundsSize) || Utils.IsViolatingBoundary(leftHandLocal, boundsSize) || Utils.IsViolatingBoundary(headLocal, boundsSize))
                                    {
                                        lastFrameTimestamp = currentTimestamp;
                                        //timeOutsideBounds += lastFrameTimestamp;
                                    }
                                    else
                                    {
                                        //currentTimestamp = 
                                        timeOutsideBounds += (currentTimestamp - lastFrameTimestamp);
                                        lastFrameTimestamp = currentTimestamp;
                                    }
                                   
                                }/*
                                else
                                {
                                    lastFrameTimestamp = 0;
                                }*/
                            }

                        }


                    }
                    stringToSave += user + "," + condition + "," + dominant + "," + timeOutsideBounds + "\n";
                    System.IO.File.AppendAllText(basePath + "reportBoundaryViolation2.csv", stringToSave);
                   
                    //stringToSave += (user + "," + passiveHaptics + "," + orientation + "," + Utils.vector3ToString(calibratedPlanePos) + "," + Utils.vector3ToString(calibratedPlaneAngles) + "\n");

                    //System.IO.File.AppendAllText(basePath + "reportTimeGazeViolationNoDominantew.csv", stringToSave);
                    stringToSave = "";
                }
            }



        }

        Debug.Log("terminou");



    }




    void processGazeViolationsWithDominantAndNonDominant()
    {
        GameObject head = new GameObject("head");
        GameObject rightHand = new GameObject("rightHand");
        GameObject leftHand = new GameObject("leftHand");
        GameObject centerBounds = new GameObject("centerBounds");

        Vector3 sizeBounds = new Vector3(1.0f, 0.5f, 0.5f);


        string basePath = Directory.GetCurrentDirectory() + "\\LogFiles\\";
        // Read the file and display it line by line.  
        string fileName = "allIn.csv";
        string header = "Id,Condition,Dominant,TimeInsideGazeBounds,TimeSecondzone,TimeOutsideGazeBounds,TotalTime,PercentageInsideGazeBounds,PercentageSecondZone,PercentageOutsideGazeBounds\n";
        /*public string headerPlayer1Interaction = "userId,currentTask,dominantPlayer," + Utils.vecNameToString("centerBoundsPos") + "," + Utils.vecNameToString("centerAreaRot") + "," +
                                                Utils.vecNameToString("headPos") + "," + Utils.vecNameToString("headRot") + "," + Utils.vecNameToString("rightHandPos") + Utils.vecNameToString("rightHandRot") + "," +
                                                Utils.vecNameToString("leftHandPos") + Utils.vecNameToString("leftHandRot") + "partId," + Utils.vecNameToString("objInteractedPos") + "," + Utils.vecNameToString("objInteractedRot") + "\n";

        public string player1InteractionStr = "userId,dominantPlayer,timestamp,collabType,currentTask,dominantPlayer,isViolatingBoundary," + Utils.vecNameToString("Player1AreaCenter") + "," + Utils.vecNameToString("Player1AreaRot")
                                + Utils.vecNameToString("headPosP1") + "," + Utils.vecNameToString("headRotP1") + "," + Utils.vecNameToString("rightHandPosP1") + Utils.vecNameToString("rightHandRotP1")
                                 + Utils.vecNameToString("leftHandPosP1") + "," + Utils.vecNameToString("leftHandRotP1") + "\n";*/
        string stringToSave = header;
        bool lastWasFromAPressedEvent = false;



        string[] dirs = Directory.GetDirectories(basePath, "G*", SearchOption.TopDirectoryOnly);

        //Console.WriteLine("The number of directories starting with p is {0}.", dirs.Length);
        foreach (string currentDir in dirs)
        {

            string[] userP = currentDir.Split('\\');//user count
            string user = userP[userP.Length - 1];

            string[] conditionsFromAUserDir = Directory.GetDirectories(currentDir);

            foreach (string conditionDir in conditionsFromAUserDir)
            {
                string[] files = Directory.GetFiles(conditionDir, "M*");
                foreach (string fileStr in files)
                {
                    System.IO.StreamReader filee = new System.IO.StreamReader(fileStr);

                    string[] splitStr = fileStr.Split('_');


                    //string passiveHaptics = splitStr[splitStr.Length - 2];
                    //string orientation = splitStr[splitStr.Length - 1];

                    //orientation = orientation.Split('.')[0];//remove extension

                    Vector3 calibratedPlanePos = Vector3.zero;
                    Vector3 calibratedPlaneAngles = Vector3.zero;



                    string line = "";
                    string condition = "";
                    string part = "";
                    string dominant = "";
                    int count = 0;
                    float lastFrameInsideNonDominant = 0;
                    float totalTimeNonDominant = 0;
                    float lastFrameSecondZoneNonDominant = 0;
                    float totalTimeSecondZoneNonDominant = 0;
                    float lastFrameOutsideNonDominant = 0;
                    float totalTimeOutsideNonDominant = 0;
                    int countInsideNonDominant = 0;
                    int countSecondZoneNonDominant = 0;
                    int countOutsideNonDominant = 0;

                    float lastFrameInsideDominant = 0;
                    float totalTimeDominant = 0;
                    float lastFrameSecondZoneDominant = 0;
                    float totalTimeSecondZoneDominant = 0;
                    float lastFrameOutsideDominant = 0;
                    float totalTimeOutsideDominant = 0;
                    int countInsideDominant = 0;
                    int countSecondZoneDominant = 0;
                    int countOutsideDominant = 0;


                    while ((line = filee.ReadLine()) != null)
                    {

                        if (line.Split(',')[0].Contains("u"))
                        {
                            //do nothing - ignore headers
                        }
                        else
                        {

                            int countRows = 30;
                            string[] lineArray = line.Split(',');
                            if (lineArray.Length < 30)
                            {

                            }
                            else
                            {
                                dominant = lineArray[5];
                                bool isDominant = false;
                                Boolean.TryParse(lineArray[5], out isDominant);

                                user = lineArray[0];
                                condition = lineArray[2];
                                part = lineArray[1];

                                centerBounds.transform.position = Utils.stringToVector3(lineArray[7] + "," + lineArray[8] + "," + lineArray[9], ',');
                                centerBounds.transform.eulerAngles = Utils.stringToVector3(lineArray[10] + "," + lineArray[11] + "," + lineArray[12], ',');

                                head.transform.position = Utils.stringToVector3(lineArray[13] + "," + lineArray[14] + "," + lineArray[15], ',');
                                head.transform.eulerAngles = Utils.stringToVector3(lineArray[16] + "," + lineArray[17] + "," + lineArray[18], ',');

                                head.transform.parent = centerBounds.transform;
                                Vector3 headRot = head.transform.localEulerAngles;
                                if (headRot.y > 90)
                                {
                                    headRot = new Vector3(head.transform.localEulerAngles.x, head.transform.localEulerAngles.y - 360.0f, head.transform.localEulerAngles.z);
                                }

                                if (!isDominant)
                                {
                                    if (headRot.y <= 30 && headRot.y >= -30)
                                    {
                                        if (lastFrameInsideNonDominant == 0)
                                        {
                                            float.TryParse(lineArray[3], out lastFrameInsideNonDominant);
                                            countInsideNonDominant++;
                                        }
                                        else
                                        {
                                            float currentTimestamp = 0;
                                            float.TryParse(lineArray[3], out currentTimestamp);
                                            totalTimeNonDominant += (currentTimestamp - lastFrameInsideNonDominant);
                                            lastFrameInsideNonDominant = currentTimestamp;
                                        }
                                    }
                                    else
                                    {
                                        lastFrameInsideNonDominant = 0;
                                    }

                                }


                                if (!isDominant)
                                {

                                    if ((headRot.y > 30 && headRot.y <= 60))
                                    {
                                        if (lastFrameSecondZoneNonDominant == 0)
                                        {
                                            float.TryParse(lineArray[3], out lastFrameSecondZoneNonDominant);
                                            countSecondZoneNonDominant++;
                                        }
                                        else
                                        {
                                            float currentTimestamp = 0;
                                            float.TryParse(lineArray[3], out currentTimestamp);
                                            totalTimeSecondZoneNonDominant += (currentTimestamp - lastFrameSecondZoneNonDominant);
                                            lastFrameSecondZoneNonDominant = currentTimestamp;

                                        }
                                    }
                                    else if ((headRot.y < -30 && headRot.y >= -60))
                                    {
                                        if (lastFrameSecondZoneNonDominant == 0)
                                        {
                                            float.TryParse(lineArray[3], out lastFrameSecondZoneNonDominant);
                                            countSecondZoneNonDominant++;
                                        }
                                        else
                                        {
                                            float currentTimestamp = 0;
                                            float.TryParse(lineArray[3], out currentTimestamp);
                                            totalTimeSecondZoneNonDominant += (currentTimestamp - lastFrameSecondZoneNonDominant);
                                            lastFrameSecondZoneNonDominant = currentTimestamp;
                                        }
                                    }

                                    else
                                    {
                                        lastFrameSecondZoneNonDominant = 0;
                                    }


                                }
                                if (!isDominant)
                                {

                                    if ((headRot.y > 60 && headRot.y <= 90.0f))
                                    {
                                        if (lastFrameOutsideNonDominant == 0)
                                        {
                                            float.TryParse(lineArray[3], out lastFrameOutsideNonDominant);
                                            countOutsideNonDominant++;
                                        }
                                        else
                                        {
                                            float currentTimestamp = 0;
                                            float.TryParse(lineArray[3], out currentTimestamp);
                                            totalTimeOutsideNonDominant += (currentTimestamp - lastFrameOutsideNonDominant);
                                            lastFrameOutsideNonDominant = currentTimestamp;
                                        }
                                    }
                                    else if ((headRot.y < -60 && headRot.y >= -90.0f))
                                    {
                                        if (lastFrameOutsideNonDominant == 0)
                                        {
                                            float.TryParse(lineArray[3], out lastFrameOutsideNonDominant);
                                            countOutsideNonDominant++;
                                        }
                                        else
                                        {
                                            float currentTimestamp = 0;
                                            float.TryParse(lineArray[3], out currentTimestamp);
                                            totalTimeOutsideNonDominant += (currentTimestamp - lastFrameOutsideNonDominant);
                                            lastFrameOutsideNonDominant = currentTimestamp;
                                        }
                                    }
                                    else
                                    {
                                        lastFrameOutsideNonDominant = 0;
                                    }


                                }

                                //now dominant
                                if (isDominant)
                                {
                                    if (headRot.y <= 30 && headRot.y >= -30)
                                    {
                                        if (lastFrameInsideDominant == 0)
                                        {
                                            float.TryParse(lineArray[3], out lastFrameInsideDominant);
                                            countInsideDominant++;
                                        }
                                        else
                                        {
                                            float currentTimestamp = 0;
                                            float.TryParse(lineArray[3], out currentTimestamp);
                                            totalTimeDominant += (currentTimestamp - lastFrameInsideDominant);
                                            lastFrameInsideDominant = currentTimestamp;
                                        }
                                    }
                                    else
                                    {
                                        lastFrameInsideDominant = 0;
                                    }

                                }


                                if (isDominant)
                                {

                                    if ((headRot.y > 30 && headRot.y <= 60))
                                    {
                                        if (lastFrameSecondZoneDominant == 0)
                                        {
                                            float.TryParse(lineArray[3], out lastFrameSecondZoneNonDominant);
                                            countSecondZoneNonDominant++;
                                        }
                                        else
                                        {
                                            float currentTimestamp = 0;
                                            float.TryParse(lineArray[3], out currentTimestamp);
                                            totalTimeSecondZoneDominant += (currentTimestamp - lastFrameSecondZoneDominant);
                                            lastFrameSecondZoneDominant = currentTimestamp;

                                        }
                                    }
                                    else if ((headRot.y < -30 && headRot.y >= -60))
                                    {
                                        if (lastFrameSecondZoneDominant == 0)
                                        {
                                            float.TryParse(lineArray[3], out lastFrameSecondZoneDominant);
                                            countSecondZoneDominant++;
                                        }
                                        else
                                        {
                                            float currentTimestamp = 0;
                                            float.TryParse(lineArray[3], out currentTimestamp);
                                            totalTimeSecondZoneDominant += (currentTimestamp - lastFrameSecondZoneDominant);
                                            lastFrameSecondZoneDominant = currentTimestamp;
                                        }
                                    }

                                    else
                                    {
                                        lastFrameSecondZoneDominant = 0;
                                    }


                                }
                                if (isDominant)
                                {

                                    if ((headRot.y > 60 && headRot.y <= 90.0f))
                                    {
                                        if (lastFrameOutsideDominant == 0)
                                        {
                                            float.TryParse(lineArray[3], out lastFrameOutsideDominant);
                                            countOutsideDominant++;
                                        }
                                        else
                                        {
                                            float currentTimestamp = 0;
                                            float.TryParse(lineArray[3], out currentTimestamp);
                                            totalTimeOutsideDominant += (currentTimestamp - lastFrameOutsideDominant);
                                            lastFrameOutsideDominant = currentTimestamp;
                                        }
                                    }
                                    else if ((headRot.y < -60 && headRot.y >= -90.0f))
                                    {
                                        if (lastFrameOutsideDominant == 0)
                                        {
                                            float.TryParse(lineArray[3], out lastFrameOutsideDominant);
                                            countOutsideDominant++;
                                        }
                                        else
                                        {
                                            float currentTimestamp = 0;
                                            float.TryParse(lineArray[3], out currentTimestamp);
                                            totalTimeOutsideDominant += (currentTimestamp - lastFrameOutsideDominant);
                                            lastFrameOutsideDominant = currentTimestamp;
                                        }
                                    }
                                    else
                                    {
                                        lastFrameOutsideDominant = 0;
                                    }


                                }



                                if (!isDominant)
                                {
                                    if (head.transform.localEulerAngles.y > 90)
                                    {
                                        string str = "condition," + head.transform.localEulerAngles.y;
                                    }
                                    else if (head.transform.localEulerAngles.y < -90)
                                    {
                                        string str = "condition," + head.transform.localEulerAngles.y;
                                    }
                                }

                                string emaranhado = lineArray[24];
                                string tmp1 = "";
                                string tmp2 = "";
                                /*if (emaranhado.Contains("-"))
                                {
                                    tmp1 = emaranhado.Split('-')[0];
                                    tmp2 = emaranhado.Split('-')[1];
                                }
                                else
                                {
                                    int indexOf = emaranhado.LastIndexOf('.');
                                    emaranhado = emaranhado.Substring(0, indexOf - 1);

                                    tmp2 = emaranhado.Substring(0);
                                    emaranhado = emaranhado.Substring(0, emaranhado.Length - 2);
                                    //indexOf += 6;

                                    tmp1 = emaranhado.Substring(0, indexOf - 1);
                                    //print(emaranhado.Length + "," + emaranhado[emaranhado.Length - 1] + "["+indexOf + ","+ (emaranhado.Length-1)+ "]");

                                    tmp2 = emaranhado.Substring(indexOf);
                                }*/

                            }

                        }


                    }

                    float tTimeNonDominant = totalTimeNonDominant + totalTimeSecondZoneNonDominant + totalTimeOutsideNonDominant;
                    float tTimeDominant = totalTimeDominant + totalTimeSecondZoneDominant + totalTimeOutsideDominant;
                    //if (dominant == "TRUE")

                    stringToSave += (user + "," + condition + "," + "nondominant" + "," + totalTimeNonDominant + "," + totalTimeSecondZoneNonDominant + "," + totalTimeOutsideNonDominant + "," + (tTimeNonDominant) + "," +
                    (totalTimeNonDominant / (tTimeNonDominant)) * 100.0f) + "," + ((totalTimeSecondZoneNonDominant) / tTimeNonDominant) * 100.0f + "," + (totalTimeOutsideNonDominant / (tTimeNonDominant)) * 100.0f + "," + countSecondZoneNonDominant + "," + countOutsideNonDominant + "\n";

                    stringToSave += (user + "," + condition + "," + "dominant" + "," + totalTimeDominant + "," + totalTimeSecondZoneDominant + "," + totalTimeOutsideDominant + "," + (tTimeDominant) + "," +
                    (totalTimeDominant / (tTimeDominant)) * 100.0f) + "," + ((totalTimeSecondZoneDominant) / tTimeNonDominant) * 100.0f + "," + (totalTimeOutsideDominant / (tTimeDominant)) * 100.0f + "," + countSecondZoneDominant + "," + countOutsideDominant + "\n";

                    totalTimeNonDominant = 0;
                    totalTimeSecondZoneNonDominant = 0;
                    totalTimeOutsideNonDominant = 0;
                    lastFrameSecondZoneNonDominant = 0;
                    lastFrameInsideNonDominant = 0;
                    lastFrameOutsideNonDominant = 0;


                    totalTimeDominant = 0;
                    totalTimeSecondZoneDominant = 0;
                    totalTimeOutsideDominant = 0;
                    lastFrameSecondZoneDominant = 0;
                    lastFrameInsideDominant = 0;
                    lastFrameOutsideDominant = 0;
                    //stringToSave += (user + "," + passiveHaptics + "," + orientation + "," + Utils.vector3ToString(calibratedPlanePos) + "," + Utils.vector3ToString(calibratedPlaneAngles) + "\n");

                    System.IO.File.AppendAllText(basePath + "reportTimeGazeViolation_BothRoles2.csv", stringToSave);
                    stringToSave = "";
                }
            }



        }

        Debug.Log("terminou");






    }








    void processCSV2()
    {
        GameObject head = new GameObject("head");
        GameObject rightHand = new GameObject("rightHand");
        GameObject leftHand = new GameObject("leftHand");
        GameObject centerBounds = new GameObject("centerBounds");

        Vector3 sizeBounds = new Vector3(1.0f, 0.5f, 0.5f);


        string basePath = Directory.GetCurrentDirectory() + "\\LogFiles\\";
        // Read the file and display it line by line.  
        string fileName = "allIn.csv";
        string header = "Id,Condition,Dominant,TimeInsideGazeBounds,TimeSecondzone,TimeOutsideGazeBounds,TotalTime,PercentageInsideGazeBounds,PercentageSecondZone,PercentageOutsideGazeBounds\n";
        /*public string headerPlayer1Interaction = "userId,currentTask,dominantPlayer," + Utils.vecNameToString("centerBoundsPos") + "," + Utils.vecNameToString("centerAreaRot") + "," +
                                                Utils.vecNameToString("headPos") + "," + Utils.vecNameToString("headRot") + "," + Utils.vecNameToString("rightHandPos") + Utils.vecNameToString("rightHandRot") + "," +
                                                Utils.vecNameToString("leftHandPos") + Utils.vecNameToString("leftHandRot") + "partId," + Utils.vecNameToString("objInteractedPos") + "," + Utils.vecNameToString("objInteractedRot") + "\n";

        public string player1InteractionStr = "userId,dominantPlayer,timestamp,collabType,currentTask,dominantPlayer,isViolatingBoundary," + Utils.vecNameToString("Player1AreaCenter") + "," + Utils.vecNameToString("Player1AreaRot")
                                + Utils.vecNameToString("headPosP1") + "," + Utils.vecNameToString("headRotP1") + "," + Utils.vecNameToString("rightHandPosP1") + Utils.vecNameToString("rightHandRotP1")
                                 + Utils.vecNameToString("leftHandPosP1") + "," + Utils.vecNameToString("leftHandRotP1") + "\n";*/
        string stringToSave = header;
        bool lastWasFromAPressedEvent = false;



        string[] dirs = Directory.GetDirectories(basePath, "G*", SearchOption.TopDirectoryOnly);

        //Console.WriteLine("The number of directories starting with p is {0}.", dirs.Length);
        foreach (string currentDir in dirs)
        {

            string[] userP = currentDir.Split('\\');//user count
            string user = userP[userP.Length - 1];

            string[] conditionsFromAUserDir = Directory.GetDirectories(currentDir);

            foreach (string conditionDir in conditionsFromAUserDir)
            {
                string[] files = Directory.GetFiles(conditionDir, "M*");
                foreach (string fileStr in files)
                {
                    System.IO.StreamReader filee = new System.IO.StreamReader(fileStr);

                    string[] splitStr = fileStr.Split('_');


                    //string passiveHaptics = splitStr[splitStr.Length - 2];
                    //string orientation = splitStr[splitStr.Length - 1];

                    //orientation = orientation.Split('.')[0];//remove extension

                    Vector3 calibratedPlanePos = Vector3.zero;
                    Vector3 calibratedPlaneAngles = Vector3.zero;

                    

                    string line = "";
                    string condition = "";
                    string part = "";
                    string dominant = "";
                    int count = 0;
                    float lastFrameInside = 0;
                    float totalTime = 0;
                    float lastFrameSecondZone = 0;
                    float totalTimeSecondZone = 0;
                    float lastFrameOutside = 0;
                    float totalTimeOutside = 0;
                    int countInside = 0;
                    int countSecondZone = 0;
                    int countOutside = 0;
                    while ((line = filee.ReadLine()) != null)
                    {

                        if (line.Split(',')[0].Contains("u"))
                        {
                            //do nothing - ignore headers
                        }
                        else
                        {

                            int countRows = 30;
                            string[] lineArray = line.Split(',');
                            if (lineArray.Length < 30)
                            {

                            }
                            else
                            {
                                dominant = lineArray[5];
                                bool isDominant = false;
                                Boolean.TryParse(lineArray[5], out isDominant);

                                user = lineArray[0];
                                condition = lineArray[2];
                                part = lineArray[1];

                                centerBounds.transform.position = Utils.stringToVector3(lineArray[7] + "," + lineArray[8] + "," + lineArray[9], ',');
                                centerBounds.transform.eulerAngles = Utils.stringToVector3(lineArray[10] + "," + lineArray[11] + "," + lineArray[12], ',');

                                head.transform.position = Utils.stringToVector3(lineArray[13] + "," + lineArray[14] + "," + lineArray[15], ',');
                                head.transform.eulerAngles = Utils.stringToVector3(lineArray[16] + "," + lineArray[17] + "," + lineArray[18], ',');

                                head.transform.parent = centerBounds.transform;
                                Vector3 headRot = head.transform.localEulerAngles;
                                if(headRot.y > 90)
                                {
                                    headRot = new Vector3(head.transform.localEulerAngles.x, head.transform.localEulerAngles.y -360.0f, head.transform.localEulerAngles.z);
                                }

                                if (!isDominant)
                                {
                                    if (headRot.y <= 30 && headRot.y >= -30)
                                    {
                                        if (lastFrameInside == 0)
                                        {
                                            float.TryParse(lineArray[3], out lastFrameInside);
                                            countInside++;
                                        }
                                        else
                                        {
                                            float currentTimestamp = 0;
                                            float.TryParse(lineArray[3], out currentTimestamp);
                                            totalTime += (currentTimestamp - lastFrameInside);
                                            lastFrameInside = currentTimestamp;
                                        }
                                    }
                                    else
                                    {
                                        lastFrameInside = 0;
                                    }

                                }
                                

                                if (!isDominant)
                                {

                                    if ((headRot.y > 30 && headRot.y <= 60) )
                                    {
                                        if (lastFrameSecondZone == 0)
                                        {
                                            float.TryParse(lineArray[3], out lastFrameSecondZone);
                                            countSecondZone++;
                                        }
                                        else
                                        {
                                            float currentTimestamp = 0;
                                            float.TryParse(lineArray[3], out currentTimestamp);
                                            totalTimeSecondZone += (currentTimestamp - lastFrameSecondZone);
                                            lastFrameSecondZone = currentTimestamp;
                                            
                                        }
                                    }
                                    else if((headRot.y < -30 && headRot.y >= -60))
                                    {
                                        if (lastFrameSecondZone == 0)
                                        {
                                            float.TryParse(lineArray[3], out lastFrameSecondZone);
                                            countSecondZone++;
                                        }
                                        else
                                        {
                                            float currentTimestamp = 0;
                                            float.TryParse(lineArray[3], out currentTimestamp);
                                            totalTimeSecondZone += (currentTimestamp - lastFrameSecondZone);
                                            lastFrameSecondZone = currentTimestamp;
                                        }
                                    }

                                    else
                                    {
                                        lastFrameSecondZone = 0;
                                    }


                                }
                                if (!isDominant)
                                {

                                    if ((headRot.y > 60 && headRot.y <= 90.0f) )
                                    {
                                        if (lastFrameOutside == 0)
                                        {
                                            float.TryParse(lineArray[3], out lastFrameOutside);
                                            countOutside++;
                                        }
                                        else
                                        {
                                            float currentTimestamp = 0;
                                            float.TryParse(lineArray[3], out currentTimestamp);
                                            totalTimeOutside += (currentTimestamp - lastFrameOutside);
                                            lastFrameOutside = currentTimestamp;
                                        }
                                    }
                                    else if ((headRot.y < -60 && headRot.y >= -90.0f))
                                    {
                                        if (lastFrameOutside == 0)
                                        {
                                            float.TryParse(lineArray[3], out lastFrameOutside);
                                            countOutside++;
                                        }
                                        else
                                        {
                                            float currentTimestamp = 0;
                                            float.TryParse(lineArray[3], out currentTimestamp);
                                            totalTimeOutside += (currentTimestamp - lastFrameOutside);
                                            lastFrameOutside = currentTimestamp;
                                        }
                                    }
                                    else
                                    {
                                        lastFrameOutside = 0;
                                    }


                                }

                                if (!isDominant)
                                {
                                    if(head.transform.localEulerAngles.y > 90)
                                    {
                                        string str = "condition," + head.transform.localEulerAngles.y;
                                    }
                                    else if(head.transform.localEulerAngles.y < -90)
                                    {
                                        string str = "condition," + head.transform.localEulerAngles.y;
                                    }
                                }

                                string emaranhado = lineArray[24];
                                string tmp1 = "";
                                string tmp2 = "";
                                /*if (emaranhado.Contains("-"))
                                {
                                    tmp1 = emaranhado.Split('-')[0];
                                    tmp2 = emaranhado.Split('-')[1];
                                }
                                else
                                {
                                    int indexOf = emaranhado.LastIndexOf('.');
                                    emaranhado = emaranhado.Substring(0, indexOf - 1);

                                    tmp2 = emaranhado.Substring(0);
                                    emaranhado = emaranhado.Substring(0, emaranhado.Length - 2);
                                    //indexOf += 6;

                                    tmp1 = emaranhado.Substring(0, indexOf - 1);
                                    //print(emaranhado.Length + "," + emaranhado[emaranhado.Length - 1] + "["+indexOf + ","+ (emaranhado.Length-1)+ "]");

                                    tmp2 = emaranhado.Substring(indexOf);
                                }*/

                            }

                        }


                    }

                    float tTime = totalTime + totalTimeSecondZone + totalTimeOutside;
                    //if (dominant == "TRUE")

                    stringToSave += (user + "," + condition + "," + dominant + "," + totalTime +"," + totalTimeSecondZone+ "," + totalTimeOutside + "," + (tTime) + "," +
                    (totalTime / (tTime)) * 100.0f) + "," + ((totalTimeSecondZone)/tTime)*100.0f +  "," +  (totalTimeOutside / (tTime)) * 100.0f + ","+ countSecondZone + ","+countOutside+ "\n";

                    totalTime = 0;
                    totalTimeSecondZone = 0;
                    totalTimeOutside = 0;
                    lastFrameSecondZone = 0;
                    lastFrameInside = 0;
                    lastFrameOutside = 0;
                    //stringToSave += (user + "," + passiveHaptics + "," + orientation + "," + Utils.vector3ToString(calibratedPlanePos) + "," + Utils.vector3ToString(calibratedPlaneAngles) + "\n");

                    System.IO.File.AppendAllText(basePath + "reportTimeGazeViolation_NoDominantew21.csv", stringToSave);
                    stringToSave = "";
                }
            }



        }

        Debug.Log("terminou");






    }



    void processCSV()
    {
        GameObject head = new GameObject("head");
        GameObject rightHand = new GameObject("rightHand");
        GameObject leftHand = new GameObject("leftHand");
        GameObject centerBounds = new GameObject("centerBounds");

        Vector3 sizeBounds = new Vector3(1.0f, 0.5f, 0.5f);


        string basePath = Directory.GetCurrentDirectory() + "\\LogFiles\\";
        // Read the file and display it line by line.  
        string fileName = "allIn.csv";
        string header = "Id,Condition,Dominant,TimeInsideGazeBounds,TimeOutsideGazeBounds,TotalTime,PercentageInsideGazeBounds,PercentageOutsideGazeBounds\n";
        /*public string headerPlayer1Interaction = "userId,currentTask,dominantPlayer," + Utils.vecNameToString("centerBoundsPos") + "," + Utils.vecNameToString("centerAreaRot") + "," +
                                                Utils.vecNameToString("headPos") + "," + Utils.vecNameToString("headRot") + "," + Utils.vecNameToString("rightHandPos") + Utils.vecNameToString("rightHandRot") + "," +
                                                Utils.vecNameToString("leftHandPos") + Utils.vecNameToString("leftHandRot") + "partId," + Utils.vecNameToString("objInteractedPos") + "," + Utils.vecNameToString("objInteractedRot") + "\n";

        public string player1InteractionStr = "userId,dominantPlayer,timestamp,collabType,currentTask,dominantPlayer,isViolatingBoundary," + Utils.vecNameToString("Player1AreaCenter") + "," + Utils.vecNameToString("Player1AreaRot")
                                + Utils.vecNameToString("headPosP1") + "," + Utils.vecNameToString("headRotP1") + "," + Utils.vecNameToString("rightHandPosP1") + Utils.vecNameToString("rightHandRotP1")
                                 + Utils.vecNameToString("leftHandPosP1") + "," + Utils.vecNameToString("leftHandRotP1") + "\n";*/
        string stringToSave = header;
        bool lastWasFromAPressedEvent = false;
        
    
        
            string[] dirs = Directory.GetDirectories(basePath, "G*", SearchOption.TopDirectoryOnly);

            //Console.WriteLine("The number of directories starting with p is {0}.", dirs.Length);
            foreach (string currentDir in dirs)
            {

                string[] userP = currentDir.Split('\\');//user count
                string user = userP[userP.Length - 1];

                string[] conditionsFromAUserDir = Directory.GetDirectories(currentDir);

            foreach (string conditionDir in conditionsFromAUserDir)
            {
                string[] files = Directory.GetFiles(conditionDir, "M*");
                foreach (string fileStr in files)
                {
                    System.IO.StreamReader filee = new System.IO.StreamReader(fileStr);

                    string[] splitStr = fileStr.Split('_');


                    //string passiveHaptics = splitStr[splitStr.Length - 2];
                    //string orientation = splitStr[splitStr.Length - 1];

                    //orientation = orientation.Split('.')[0];//remove extension

                    Vector3 calibratedPlanePos = Vector3.zero;
                    Vector3 calibratedPlaneAngles = Vector3.zero;

                    string line = "";
                    string condition = "";
                    string part = "";
                    string dominant = "";
                    int count = 0;
                    float lastFrameInside = 0;
                    float totalTime = 0;
                    float lastFrameOutside = 0;
                    float totalTimeOutside = 0;
                    while ((line = filee.ReadLine()) != null)
                    {

                        if (line.Split(',')[0].Contains("u"))
                        {
                            //do nothing - ignore headers
                        }
                        else
                        {

                            int countRows = 30;
                            string[] lineArray = line.Split(',');
                            if (lineArray.Length < 30)
                            {

                            }
                            else { 
                            dominant = lineArray[5];
                            bool isDominant = false;
                            Boolean.TryParse(lineArray[5], out isDominant);

                            user = lineArray[0];
                            condition = lineArray[2];
                            part = lineArray[1];

                            centerBounds.transform.position = Utils.stringToVector3(lineArray[7] + "," + lineArray[8] + "," + lineArray[9], ',');
                            centerBounds.transform.eulerAngles = Utils.stringToVector3(lineArray[10] + "," + lineArray[11] + "," + lineArray[12], ',');

                            head.transform.position = Utils.stringToVector3(lineArray[13] + "," + lineArray[14] + "," + lineArray[15], ',');
                            head.transform.eulerAngles = Utils.stringToVector3(lineArray[16] + "," + lineArray[17] + "," + lineArray[18], ',');

                            head.transform.parent = centerBounds.transform;

                                if (!isDominant)
                                {
                                    if (head.transform.localEulerAngles.y <= 30.0f && head.transform.localEulerAngles.y >= -30.0f)
                                    {
                                        if (lastFrameInside == 0)
                                        {
                                            float.TryParse(lineArray[3], out lastFrameInside);
                                            //lastFrameInside = Time.realtimeSinceStartup;
                                        }
                                        else
                                        {
                                            float currentTimestamp = 0;
                                            float.TryParse(lineArray[3], out currentTimestamp);
                                            totalTime += (currentTimestamp - lastFrameInside);
                                            lastFrameInside = currentTimestamp;
                                        }
                                    }
                                    else 
                                    {
                                        lastFrameInside = 0;
                                    }

                                }
                                if (!isDominant)
                                {
                                     
                                        if (head.transform.localEulerAngles.y >= 30.0f || head.transform.localEulerAngles.y <= -30.0f)
                                        {
                                            if (lastFrameOutside == 0)
                                            {
                                                float.TryParse(lineArray[3], out lastFrameOutside);
                                            }
                                            else
                                            {
                                                float currentTimestamp = 0;
                                                float.TryParse(lineArray[3], out currentTimestamp);
                                                totalTimeOutside += (currentTimestamp - lastFrameOutside);
                                                lastFrameOutside = currentTimestamp;
                                            }
                                        }
                                        else
                                        {
                                            lastFrameOutside = 0;
                                        }
                                    

                                }


                                string emaranhado = lineArray[24];
                            string tmp1 = "";
                            string tmp2 = "";
                            /*if (emaranhado.Contains("-"))
                            {
                                tmp1 = emaranhado.Split('-')[0];
                                tmp2 = emaranhado.Split('-')[1];
                            }
                            else
                            {
                                int indexOf = emaranhado.LastIndexOf('.');
                                emaranhado = emaranhado.Substring(0, indexOf - 1);

                                tmp2 = emaranhado.Substring(0);
                                emaranhado = emaranhado.Substring(0, emaranhado.Length - 2);
                                //indexOf += 6;

                                tmp1 = emaranhado.Substring(0, indexOf - 1);
                                //print(emaranhado.Length + "," + emaranhado[emaranhado.Length - 1] + "["+indexOf + ","+ (emaranhado.Length-1)+ "]");

                                tmp2 = emaranhado.Substring(indexOf);
                            }*/

                        }

                        }
                        

                    }
                    //if (dominant == "TRUE")
                    stringToSave += (user + "," + condition + "," + part + "," + dominant + "," + totalTime + "," + totalTimeOutside + "," + (totalTime + totalTimeOutside) + "," +
                    (totalTime / (totalTime + totalTimeOutside)) * 100.0f )+ "," + (totalTimeOutside / (totalTime + totalTimeOutside) )* 100.0f + "\n";

                    totalTime = 0;
                    //stringToSave += (user + "," + passiveHaptics + "," + orientation + "," + Utils.vector3ToString(calibratedPlanePos) + "," + Utils.vector3ToString(calibratedPlaneAngles) + "\n");

                    System.IO.File.AppendAllText(basePath + "reportTimeGazeViolationNoDominant3.csv", stringToSave);
                    stringToSave = "";
                }
            }



            }

            Debug.Log("terminou");





        
    }

}
