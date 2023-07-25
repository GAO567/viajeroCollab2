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
        processCSV();
    }

    // Update is called once per frame
    void Update()
    {
        
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

                    System.IO.File.AppendAllText(basePath + "reportTimeGazeViolationNoDominant2.csv", stringToSave);
                    stringToSave = "";
                }
            }



            }

            Debug.Log("terminou");





        
    }

}
