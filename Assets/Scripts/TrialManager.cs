using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TrialManager : MonoBehaviour
{
    // References to other Scripts. Keep DataManaer & TrialManager on the same GO. 
    private DataManager MyDataManager;


    // References to Game Objects and their properties. 
    public Transform Room;
    public List<Transform> Targets;
    public Material[] TargetMaterials;

    // References for Path construction. 
    /*  Paths for each trial are constructed by connecting 3 target poles. Paths are randomized for each trial. 
    /*  PathArray - stores 3 target pole indices (ints). */

    public int[] PathArray;             // The current path array. 
    private float[] PathDistanceArray;  // The distance between a pole and the next pole.


    // References for game/experiment progression logic 
    public int totalTrialCount = 6;     // Total number of trials to complete. Set value here. 
    public int trial_count;         // The current trial number (count) 
    public int path_index;          // The current index in the PathArray [0,1,2].
    public int response_phase;      // There are two response phases (1)=angle (2)=distance ((0)=none)

    private bool is_gameover;       // Flag to indicate all trials are finished
    public bool can_progress;       // Flag to indicate game has been paused


    // Time measurment variables for experiment  
    public float travel_time;
    public float angle_response_time;
    public float distance_response_time;
    float timer;

    // Used to caluclate distance walked by player. Store position at end of path (before walking back to path origin) 
    Vector3 playerPositionAtPathEnd;

    void Start()
    {
        // Set DataManager and Targets List
        MyDataManager = GetComponent<DataManager>();
        foreach (Transform child in transform)
            Targets.Add(child);

        // Set first PathArray
        PathArray = new int[] { 99, 99, 99 };
        RandomizePath();
        path_index = 0;

        // Set game logic flags, counters, timers 
        is_gameover = false;
        can_progress = false;
        trial_count = 0;
        response_phase = 0;
        timer = 0f;

        Debug.Log("Press Return to start the experiment.");
    }
    

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;


        if (Input.anyKeyDown && is_gameover)
        {
            Debug.Log("Game over! Exit program.");
            return;
        }

        // If RETURN is pressed AND a trial is over (the participant was finished the homing task)
        if (Input.GetKeyDown(KeyCode.Return) && !can_progress)
        {
            RecordResponses(); 
        }

        // If TAB is pressed we are in the middle of a trial
        if (Input.GetKeyDown(KeyCode.Tab) && can_progress)
        {
            WalkThePath(); 
        }
    }

    
    void RecordResponses()
    {
        
        // if response phase (1), then measure angles (before walk to target)
        if (response_phase == 1)
        {
                Debug.Log("Angle Recorded. Now, walk to the green pole (the path origin). Press Return when done.");
                angle_response_time = timer;
                MyDataManager.WriteAngleData(PathArray[0], PathArray[2]);
                playerPositionAtPathEnd = Camera.main.transform.position; // Used to calculate distance player walked from path end to origin
                timer = 0f;
                response_phase += 1; 
        }

        // else if response phase (2), then measure distances (after walk to target)
        else if (response_phase == 2)
        {
            Debug.Log("Distance Recorded. Trial Complete! Press Tab to continue.");

            // Record data
            distance_response_time = timer;
            MyDataManager.WriteDistanceTimeData(PathArray[0], PathArray[2], playerPositionAtPathEnd);

            // Update trial counters and reset experiment variables
            trial_count++;
            response_phase = 0;

            if (trial_count == totalTrialCount)
            {
                can_progress = false;
                is_gameover = true;
            }        
        }
        
        if (response_phase == 0 && !is_gameover)
        {
            Debug.Log("Press Tab to start trial # " + (trial_count+1));
            //ShowRoom(true);           //@Sreynit show room obstacles (instead of the room) 
            RandomizePath();
            path_index = 0;
            can_progress = true;
            MyDataManager.WriteTrialVariables();
        }
        
    }

    void WalkThePath()
    {
        // If TAB is pressed we are in the middle of a trial
        if (can_progress)
        {
            if (path_index == 0) // show true target (red pole) 
            {
                Debug.Log("First target. Press Tab to show next target.");
                timer = 0f;
                ShowCurrentTarget();
                path_index++;
            }
            else if (path_index <= 2) // show intermediate targets (green poles)
            {
                // may need cur_target index to calculate angles. if not, remove it. 
                Debug.Log("You have arrived. Press Tab to continue.");
                ShowCurrentTarget();
                path_index++;
            }
            else if (path_index == 3) // hide room. begin assessment 
            {
                Debug.Log("Stop moving. Now, turn to face the green post (the path origin). " +
                    "Press Return when done.");
                //ShowRoom(false);      // @Sreynit hide obstacles (instead of the room)! 
                ShowAllTargets(false);

                travel_time = timer;
                timer = 0f;
                response_phase = 1;     // begin angle measurement phase
                can_progress = false;   // but stop trial progression until after responses are finished
                path_index++;           // make path_index > 3 to prevent re-entering input loops (sloppy)
            }
        }
    }

    // return time measures as string to send to DataManager.cs
    // travel time, response time 
    public string GetTimeMeasures()
    {                
        string measures = travel_time + ", " + angle_response_time + ", " + distance_response_time;
        return measures;
    }

    // The PathArray is an array that stored taret pole indices
    // Randomize these indices so that there are no repeated values
    // and so that the distance between poles along the path is > 2m
    void RandomizePath()
    {
        // Temporary variables. 
        int first, second, third;
        float length1, length2, length3;

        // Set temporary target indices and path lengths to zero. 
        first = second = third = 0;
        length1 = length2 = length3 = 0f;

        // Prevent first pole from spawning at the same position as the previous trial's first pole
        // In case the participant is standing at this location already at the end of the prev trial! 
        int previous_first = PathArray[0];


        // Randomly generate pole index positions until all three are asigned unique values
        // and the distance between consecutive poles is < 1 meters
        while ((first == second || first == third || second == third)
            || (first == previous_first)
            || (length1 <= 1 || length2 <= 1 || length3 <= 1))
        {
            // Randomly generate indices
            first = UnityEngine.Random.Range(0, 11);
            second = UnityEngine.Random.Range(0, 11);
            third = UnityEngine.Random.Range(0, 11);

            // Get absolute distance between poles 1-2, 2-3, and 3-1
            length1 = Mathf.Abs(Vector3.Distance(Targets[first].position, Targets[second].position));
            length2 = Mathf.Abs(Vector3.Distance(Targets[second].position, Targets[third].position));
            length3 = Mathf.Abs(Vector3.Distance(Targets[third].position, Targets[first].position));
        }

        // Store the randomized indices. Store the distance between poles for each segment of the path
        PathArray = new int[] { first, second, third };
        PathDistanceArray = new float[] { length1, length2, length3 };
    }

    // To prevent null reference exceptions, we enable/disable the mesh renderer component
    // of target objects (as opposed to SetActive() on the game object) 
    void ShowCurrentTarget()
    {
        int i = 0;
        foreach (Transform target in Targets)
        {
            if (i == PathArray[path_index])
            {
                target.GetComponent<Renderer>().enabled = true;
                if (path_index == 0)
                    target.GetComponent<Renderer>().material = TargetMaterials[0];
                else if (path_index == 1)
                    target.GetComponent<Renderer>().material = TargetMaterials[1];
                else
                    target.GetComponent<Renderer>().material = TargetMaterials[2];
            }
            else
                target.GetComponent<Renderer>().enabled = false;
            i++;
        }
    }

    // Shows or Hides all of the target poles. We use this to hide all poles. 
    // Do not show all the targets. Task becomes to easy. Lessons learned (Fall 2021). 
    void ShowAllTargets(bool isVisible)
    {
        foreach (Transform target in Targets)
        {
            target.GetComponent<Renderer>().enabled = isVisible;
            target.GetComponent<Renderer>().material = TargetMaterials[0];
        }
    }


    // Show or Hide Room: by usin SetActive() on children:
    // (0) Room Basics" and (1) "Room Details"
    void ShowRoom(bool isVisible)
    {
        Room.GetChild(0).gameObject.SetActive(isVisible);
        Room.GetChild(1).gameObject.SetActive(isVisible);
    }


    public int GetTargetIndex()
    {
        return PathArray[0];
    }

    public int[] GetTargetArray()
    {
        return PathArray;
    }

    public int GetPathSegmentLength()
    {
        return PathArray.Length;
    }

    public int[] GetPathArray()
    {
        return PathArray;
    }

    public float[] GetPathDistanceArray()
    {
        return PathDistanceArray;
    }

    // Uses Knuth shuffle like everybody else *shrugs* 
    // Pure random without constraints. Consecutive poles can appear next to each other 
    // We used this for preliminary data collection (Fall 2021). We do not use if for the actual experiment. 
    void ShufflePathArray()
    {
        for (int i = 0; i < PathArray.Length; i++)
        {
            int temp = PathArray[i];
            int rand_i = UnityEngine.Random.Range(i, PathArray.Length);
            PathArray[i] = PathArray[rand_i];
            PathArray[rand_i] = temp;
        }
    }


    // No longer in use. Create list of targets for an iteration of the experiment
    // Pure random approach with variable path length. (Preliminary Data Fall 2021)
    // PathArray was originally an array of different path lengths (e.g., 2, 4, 6)
    // and it was used to determine path length. THEN, a new target array / path array was generated
    void UpdateTargetList()
    {
        int path_length = PathArray[trial_count];

        // Clear previous array. Allocate memory for new array. 
        Array.Clear(PathArray, 0, PathArray.Length);
        PathArray = new int[path_length];

        // Get length of path segment based on current trial 
        for (int i = 0; i < path_length; i++)
        {
            int temp = UnityEngine.Random.Range(0, 12);
            while (Array.Exists(PathArray, element => element == temp))
                temp = UnityEngine.Random.Range(0, 12);
            PathArray[i] = temp;
        }
    }
}
