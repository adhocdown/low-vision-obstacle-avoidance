using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR; 

// Detect what XR devices is being used. 
// For the current display: select dimension, fov, etc. to inform screen shaders
public class PlatformDefines : MonoBehaviour
{
    // using enum here to improve type safety and readability 
    public enum Type
    {
        Editor,
        OculusCV1,
        Vive,
        VivePro,
        VarjoXR3,
    }

    // Variables to reference in shader scripts 
    public struct HMD_DATA
    {
        public int screen_dimension_x;
        public int screen_dimension_y;
        public float fov_x;     // monocular fov (fov per eye)
        public float fov_y;
        public float pixel_density;
        public float scale_factor; // may delete later

        public void SetPixelDensity()
        {   // Note: not entirely accurate. verical pixel density may differ. 
            pixel_density = screen_dimension_x / fov_x;
        }

    };


    public HMD_DATA myHMD = new HMD_DATA();
    public static UnityEngine.XR.InputDevice device; //? device = null;
    public static int hmd_id;
    // Unity names for XR Devices 
    const string model_rift_cv1 = "Oculus Rift CV1";
    const string model_vive_1 = "Vive MV";
    const string model_vive_pro = "Vive Pro";
    const string model_varjo_xr3 = "Varjo XR3"; // Has thsi been tested with the new XR library? 


    // On Awake():  Detect device/build settings and save indicator 
    void Awake()
    {

        string model = device.name ?? "";         // see if device has name. assign to device name if true	
     
        #if UNITY_STANDALONE_WIN
            if (model == model_rift_cv1)
                hmd_id = (int)Type.OculusCV1;
            else if (model == model_vive_1)
                hmd_id = (int)Type.Vive;
            else if (model == model_vive_pro)
                hmd_id = (int)Type.VivePro;
             else if (model == model_varjo_xr3)
                  hmd_id = (int)Type.VarjoXR3;
        else
                hmd_id = (int)Type.Editor;
        #endif


        Debug.Log("Device Name: " + model);
        Debug.Log ("HMD Id    : " + hmd_id); 

        // Detect hmd. save fov data.
        // Detect screen w/h 
        // Get factor 
        // Use this for initialization

        // set field of view data here
        // grab vertical and horizontal FOV. 
        // grab screen resolution (no need do but may help efficiency)

        myHMD = new HMD_DATA();

        // FOV information taken from doc-0k.org/?p=1414
        switch (hmd_id)
        {
            case (int)Type.OculusCV1:
                Debug.Log("Oculus CV1 connected");
                myHMD.screen_dimension_x = 1080;
                myHMD.screen_dimension_y = 1200;
                myHMD.fov_x = 84;
                myHMD.fov_y = 93;
                myHMD.SetPixelDensity();
                break;
            case (int)Type.Vive:
                Debug.Log("Vive MV connected");
                myHMD.screen_dimension_x = 1080;
                myHMD.screen_dimension_y = 1200;
                myHMD.fov_x = 100;
                myHMD.fov_y = 110;
                myHMD.SetPixelDensity();
                //myHMD.scale_factor = 0;					
                break;
            case (int)Type.VivePro:
                Debug.Log("Vive Pro connected");
                // https://risa2000.github.io/hmdgdb/
                // HTC Vive Pro Advertises 100 x 110
                // HMDQ reports 98.755* x 107.72* 
                myHMD.screen_dimension_x = 1440;
                myHMD.screen_dimension_y = 1600;
                myHMD.fov_x = 98.755f;   // We use per eye FOV and resolution here
                myHMD.fov_y = 107.72f;
                myHMD.SetPixelDensity();
                //myHMD.scale_factor = 0;					
                break;
            case (int)Type.VarjoXR3:
                // From Varjo - https://varjo.com/products/xr-3/ 
                // ull Frame Bionic Display with human-eye resolution.
                // Focus area (27° x 27°) at 70 PPD uOLED, 1920 x 1920 px per eye
                // Peripheral area at over 30 PPD LCD, 2880 x 2720 px per eye
                // Colors: 99 % sRGB, 93 % DCI - P3
                Debug.Log("Varjo XR3 (VR mode?) connected"); 
                myHMD.screen_dimension_x = 1920; // 70 ppd. 1920x1920 ppe 
                myHMD.screen_dimension_y = 1920; 
                myHMD.fov_x = 99f;   // Use HMDQ to find these... 
                myHMD.fov_y = 99f;
                myHMD.SetPixelDensity();
                //myHMD.scale_factor = 0;					
                break;
            default:
                Debug.Log("No hmd detected [Editor] Assumes specs of HTC Vive Pro");
                myHMD.screen_dimension_x = 1440;
                myHMD.screen_dimension_y = 1600;
                myHMD.fov_x = 98.755f;   
                myHMD.fov_y = 107.72f;
                myHMD.SetPixelDensity();
                break;
        }
        Debug.Log("After switch statement");
    }



}
