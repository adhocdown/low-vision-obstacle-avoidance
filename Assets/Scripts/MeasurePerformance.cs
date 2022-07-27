using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeasurePerformance : MonoBehaviour
{
    //////////////////////////////////////////////////////////////
    // Performance Measures Include: 
    //
    // (1) Angular Error Measurements:
    //      (1a) player angle (realAngle)  
    //      (1b) perfect anle (perfectAngle)
    //      (1c) angular error (Error)
    // (2) Distance Error Measurements:
    //      (2a) player position
    //      (2b) target pole position
    //      (2c) Euclidean distance between player and goal target
    // (3) Time Measurments (See TrialManager.cs)
    //      (3a) Travel time thru path
    //      (3b) angle Response time (heading angle)
    //      (3c) Distance Response time (walk to target)
    // (4) Obstacle Collisions 
    //
    // 

    ////////////////////////////////////////////////////////////////
    // Peformance Functions Include: 
    //
    // (1) Angle Measurement Functions
    //      CalculateAngleError(int)        // Calculate angular error 
    //      GetAngles(int)                  // Send measures to DataManager as string 

    // (2) Distance Measurement Functions
    //      GetDistanceMeasures             // Calculate distance b/w player and target.
    //                                      // Send measures to DataManager as string


    public Transform TargetsParent;
    public List<Transform> Targets;
    Camera m_MainCamera;


    private void Start()
    {
        m_MainCamera = Camera.main;

        // Get all target poles via parent transform  
        TargetsParent = GameObject.FindGameObjectWithTag("Manager").transform;
        Targets = new List<Transform>(); // Create new targets list / clear out any existing list 
        foreach (Transform child in TargetsParent)
        {
            Targets.Add(child);
            //CheckTargetPositions(child); 
        }
    }

    // Perform a visual check of target object positions at start. spawn cubes at object center
    // note: Do not trust the transform position within the inspector alone. 
    // create a temp collider and see if it is aligned with the object's center in scene 
    /*private void CheckTargetPositions(Transform target)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = target.position;
        cube.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
    }*/


    ///////////////////////////////////////////////////////////////////////////////
    // DISTANCE ERROR FUNCTION(S)

    // return distance measures as string to send to DataManager.cs
    // the start index here is the END of the path. 
    // the end index is the ORIGIN POST / GOAL of the path.
    // since the task requires people to walk from the end of the path to the origin 
    public string GetDistanceMeasures(int pathOrigin, int pathEnd, Vector3 playerPositionAtPathEnd)
    {
        // Step 1: Get Real & Perfect Vector Information. They're normalized. 
        Vector3 player_position = m_MainCamera.transform.position;

        Vector3 origin_position = Targets[pathOrigin].position;
        Vector3 end_position = Targets[pathEnd].position;

        // Step 2: Compute Distance. Use vector maths. Ignore Y. 
        // Fun Fact: this is the same as the "backend" implementation for Vector2.Distance in Unity (https://docs.unity3d.com/ScriptReference/Vector2.Distance.html)

        // How far the player actually walked (distance)
        Vector3 vector_between_player_start_end = player_position - playerPositionAtPathEnd;
        vector_between_player_start_end.y = 0;
        float distance_between_player_start_end = vector_between_player_start_end.magnitude;

        // How far they should have walked (distance from path end to origin)
        Vector3 vector_between_targets = origin_position - end_position;
        vector_between_targets.y = 0;
        float distance_between_targets = vector_between_targets.magnitude;

        // Distance between where they player walked and where they should have ended up (path origin) 
        Vector3 vector_to_target = origin_position - player_position;
        vector_to_target.y = 0;
        float distance_to_target = vector_to_target.magnitude;

        // Record Distance Measures 
        // (player_start), (player_end), (correct_start), (correct_end),
        // player_distance, correct_distance, distance_error
        string measures =
            playerPositionAtPathEnd + ", " + player_position + ", " + end_position + ", " + origin_position + ", "
            + distance_between_player_start_end + ", " + distance_between_targets + ", " + distance_to_target;
        return measures;
    }

    ///////////////////////////////////////////////////////////////////////////////
    // ANGULAR ERROR FUNCTION(S)

    // Get signed error along the y axis (left-right error) 
    float CalculateAngleError(float angle1, float angle2)
    {
        float temp;

        // Difference on Y Axis  (- err left, + err right)
        if (angle1 > angle2)
            temp = -(angle1 - angle2);
        else
            temp = angle2 - angle1;


        return temp;
    }

    // Return angle measures as string to send to DataManager.cs
    public string GetAngleMeasures(int pathOrigin, int pathEnd)
    {
        // Step 1: Get Real & Perfect Vector Information. They're normalized. 
        Vector3 realVec = m_MainCamera.transform.forward;
        Vector3 perfectVec = Vector3.Normalize(Targets[pathEnd].position - Targets[pathOrigin].position); // direction vector  

        // Step 2: Compute Angles. 		
        float YangleA = (Mathf.Atan2(realVec.x, realVec.z) * Mathf.Rad2Deg);
        float YangleB = (Mathf.Atan2(perfectVec.x, perfectVec.z) * Mathf.Rad2Deg);
        if (YangleA < 0) YangleA += 360f;
        if (YangleB < 0) YangleB += 360f;


        // Record End Angles
        float realAngle = YangleA;
        float perfectAngle = YangleB;
        float Error = CalculateAngleError(perfectAngle, realAngle);

        string measures = realAngle + ", " + perfectAngle + ", " + Error;
        return measures;
    }


}
