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
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void processCSV()
    {
        string basePath = Directory.GetCurrentDirectory() + "\\LogFiles\\";
        // Read the file and display it line by line.  
        string fileName = "allIn.csv";
        string header = "Id,PassiveHaptics,Orientation,CalibratedPlanePosX,CalibratedPlanePosY,CalibratedPlanePosZ,CalibratedPlaneRotX,CalibratedPlaneRotY,CalibratedPlaneRotZ\n";

        string stringToSave = header;
        bool lastWasFromAPressedEvent = false;
        try
        {
            string[] dirs = Directory.GetDirectories(basePath, "G*", SearchOption.TopDirectoryOnly);

            //Console.WriteLine("The number of directories starting with p is {0}.", dirs.Length);
            foreach (string currentDir in dirs)
            {

                string[] userP = currentDir.Split('\\');//user count
                string user = userP[userP.Length - 1];

                string[] conditionsFromAUserDir = Directory.GetDirectories(currentDir);

                foreach (string conditionDir in conditionsFromAUserDir)
                {
                    string[] files = Directory.GetFiles(conditionDir, "h*");
                    foreach (string fileStr in files)
                    {
                        System.IO.StreamReader filee = new System.IO.StreamReader(fileStr);

                        string[] splitStr = fileStr.Split('_');

                        string passiveHaptics = splitStr[splitStr.Length - 2];
                        string orientation = splitStr[splitStr.Length - 1];

                        orientation = orientation.Split('.')[0];//remove extension

                        Vector3 calibratedPlanePos = Vector3.zero;
                        Vector3 calibratedPlaneAngles = Vector3.zero;

                        string line = "";
                        int count = 0;
                        while ((line = filee.ReadLine()) != null)
                        {

                            if (line.Split(',')[0].Contains("U"))
                            {
                                //do nothing - ignore headers
                            }
                            else
                            {
                                int countRows = 29;

                                string[] lineArray = line.Split(',');

                                if (lineArray.Length < countRows)
                                {
                                    if (lastWasFromAPressedEvent)
                                    {
                                        //do the magic here?

                                        calibratedPlanePos = Utils.stringToVector3(lineArray[21] + "," + lineArray[22] + "," + lineArray[23], ',');
                                        calibratedPlaneAngles = Utils.stringToVector3(lineArray[24] + "," + lineArray[25] + "," + lineArray[26], ',');

                                        //do somethihng



                                        string target = "";


                                        lastWasFromAPressedEvent = false;
                                    }
                                    else
                                    {
                                        //i just want to get one
                                        if (count > 0 && !lastWasFromAPressedEvent)
                                        {

                                        }
                                        lastWasFromAPressedEvent = true;


                                    }

                                    count++;

                                }
                            }
                        }



                        stringToSave += (user + "," + passiveHaptics + "," + orientation + "," + Utils.vector3ToString(calibratedPlanePos) + "," + Utils.vector3ToString(calibratedPlaneAngles) + "\n");

                        System.IO.File.AppendAllText(basePath + "allInHandMovement.csv", stringToSave);
                        stringToSave = "";

                    }
                }




            }

            Debug.Log("terminou");





        }
        catch (Exception e)
        {
            //print("The process failed: {0}", e.ToString());
            print("exception : " + e.ToString());
        }
    }

}
