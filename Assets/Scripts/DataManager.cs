using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

using UnityEngine.SceneManagement;
using System.IO;
using System;

public class DataManager : MonoBehaviour
{
    // References to other Scripts. Keep DataManaer & TrialManager on the same GO. 
    private TrialManager MyTrialManager;
    public MeasurePerformance MyPerformanceMeasures;

    // UI / Filewriter vars
    public string filename;
    public GameObject StartUI;
    public List<GameObject> inputFields;

    // Participant info to write
    string subjectId;
    string height;
    string age;
    string gender;
    // Experiment info to write
    string scotomaCondition;
    string assistiveCondition;

    // HALEY - CHANGE THIS
    // References to left and right eye cameras. Used to set low vision simulation shaders.  
    public Transform lefteye;
    public Transform righteye;



    void Start()
    {
        // Get AngleMeasurement script reference from player
        MyTrialManager = GetComponent<TrialManager>();
        MyPerformanceMeasures = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<MeasurePerformance>();

        // Get UI gameojbect reference
        StartUI = GameObject.FindGameObjectWithTag("UI");

        // Set empty participant info.  Set with UI later. 
        subjectId = "";
        age = "0";
        height = "";
        gender = "";
        scotomaCondition = "0";
        assistiveCondition = "0";
    }


    // Set filename and path.
    // Create file if it doesn't exist. Toss error if file does exist. Don't let the user accidently overwrite existing files ;)
    public void CreateFile()
    {
        GetParticipantInfoFromUI(); 
        filename = Application.dataPath + "/Data/" + "PATHEXP_ID" + subjectId + "_VFL" + scotomaCondition + "_ASSIST" + assistiveCondition + ".txt"; // use subj & exp ids
        Debug.Log(filename);

        if (!File.Exists(filename))
        {
            // Create a file to write to 
            using (StreamWriter sw = File.CreateText(filename))
            {
                // Write header for Experiment Information 
                sw.WriteLine("SubjectID, ScotomaCondition, AssistiveCondition, Height, Gender, Age");
                sw.WriteLine(subjectId + ", " + scotomaCondition + ", " + assistiveCondition + ", " + height + ", " + gender + ", " + age);

                // Write second header for Data Labels 
                sw.WriteLine("Trial, [PathArray], [PathDistanceArray], " +
                    "RealAngleY, PerfectAngleY, ErrorY, " +     // angle measures
                    "(PlayerStartPos), (PlayerEndPos), (PathEndPos), (PathOriginPos), DistancePlayerWalked, DistanceFromEndToOrigin, DistanceError, " +    // distance measures          
                    "TravelTime, ResponseTime");	            // time measures 
                sw.Close();
            }
        }
        else
        {
            Debug.LogError("ERR: FILE EXISTS");
            Application.Quit();
        }
        StartUI.SetActive(false);        // Hide UI (during the experiment)
    }




    //////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Get the text inputs from the UI input fields 
    //////////////////////////////////////////////////////////////////////////////////////////////////////////


    // Manually assign the correct GameObjects via the Inspector 
    public void GetParticipantInfoFromUI()
    {
        subjectId = inputFields[0].GetComponent<InputField>().text;
        gender = inputFields[1].GetComponent<InputField>().text; 
        height = inputFields[2].GetComponent<InputField>().text;
        age = inputFields[3].GetComponent<InputField>().text;
    }

    // Select Scotoma Condition based on dropdown option index
    // option.value = an integer
    public void SetScotomaCondition(Dropdown option)
    {
        scotomaCondition = "" + option.value; // see what this outputs? says integer.. 
        // 0,1,2
        Debug.Log("Scotoma: " + scotomaCondition);

        ////////////////////////////////////////////////////////////////////////////////
        // HALEY - UPDATE THIS 
        ////////////////////////////////////////////////////////////////////////////////
        lefteye.GetComponent<OpaqueMaskEffect>().SetMask(option.value);
        righteye.GetComponent<OpaqueMaskEffect>().SetMask(option.value);        
    }

    // Select Assisitive Condition based on dropdown option index
    // option.value = an integer
    public void SetAssistiveCondition(Dropdown option)
    {
        assistiveCondition = "" + option.value; 
        Debug.Log("Assisitve: " + assistiveCondition);

        ////////////////////////////////////////////////////////////////////////////////
        // @Sreynit.. Add your code to switch between "accessibility conditions" here !! 
        //
        //
        //
        ////////////////////////////////////////////////////////////////////////////////
    }




    //////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Get and write data when each trial is completed 
    //////////////////////////////////////////////////////////////////////////////////////////////////////////

    // WriteAngleData() is called at the end of the 1st response phase in TrialManager.cs
    // WriteDistanceTimeData() is called at the end of the 2nd response phase in TrialManager.cs


    // Write variables for trial (trial number, path array, path distance array)
    public void WriteTrialVariables()
    {
        // et current trial number and path information 
        int cur_trial = MyTrialManager.trial_count;
        int[] myPathArray = MyTrialManager.GetPathArray();
        float[] myPathDistanceArray = MyTrialManager.GetPathDistanceArray();

        // Write Trial Num, Target Index Array Length, & Target Index Array
        using (StreamWriter sw = File.AppendText(filename))
        {
            // Write current trial number
            sw.Write(cur_trial + ", [");

            // Write PathArray
            sw.Write(myPathArray[0]);
            for (int i = 1; i < myPathArray.Length; i++)
                sw.Write("," + myPathArray[i]);
            sw.Write("], [");

            // Write PathDistanceArray
            sw.Write(myPathDistanceArray[0]);
            for (int i = 1; i < myPathDistanceArray.Length; i++)
                sw.Write("," + myPathDistanceArray[i]);
            sw.Write("], ");
            sw.Close();
        }      
    }

    // For each trial, write angle measures. Called by TrialManager.cs
    public void WriteAngleData(int pathOrigin, int pathEnd)
    {
        string angle_measures = MyPerformanceMeasures.GetAngleMeasures(pathOrigin, pathEnd);

        using (StreamWriter sw = File.AppendText(filename))
        {
            sw.Write("" + angle_measures + ", ");
        }
    }

    // For each trial, write distance and time measures. Called by TrialManager.cs
    public void WriteDistanceTimeData(int pathOrigin, int pathEnd, Vector3 previousPlayerPosition)
    {
        int target_id = MyTrialManager.GetTargetIndex();
        string distance_measures = MyPerformanceMeasures.GetDistanceMeasures(pathOrigin, pathEnd, previousPlayerPosition);
        string time_measures = MyTrialManager.GetTimeMeasures();

        using (StreamWriter sw = File.AppendText(filename))
        {
            sw.Write("" + distance_measures + ", "
                + time_measures);
            sw.Write("\n");
        }

    }
    


}


