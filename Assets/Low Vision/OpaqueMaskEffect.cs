//  Haley Adams. 12.8.2020
// This code is related to an answer a user provided in the Unity forums at:
// http://forum.unity3d.com/threads/circular-fade-in-out-shader.344816/
// Previously "ScreenTransitionImageEffect"
// I have modified the code to create a scalable scotoma for a macular degeneration simulation


using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class OpaqueMaskEffect : MonoBehaviour
{
    /// Provides a shader property that is set in the inspector
    /// and a material instantiated from the shader
    public Shader shader;
    PlatformDefines HMD;

    [Range(0, 2.0f)]
    public float maskValue;
    public float maskSpread;
    public bool maskInvert;

    public Color maskColor = Color.black;
    public Texture2D maskTexture;

    // Make mask and display FOV visibile - and adjustable? for naive users and tinkerers
    // 60 degrees is default for HFA 30-2 
    // consider breaking into x and y comps
    private float maskFOV = 60f; 
    public float x_scale;
    public float y_scale;
    public float x_trans;
    public float y_trans;
    

    private Material m_Material;
    bool isLeftEye;



    Material material
    {
        get
        {
            if (m_Material == null)
            {
                m_Material = new Material(shader);
                m_Material.hideFlags = HideFlags.HideAndDontSave;
            }
            return m_Material;
        }
    }

    // On Awake: Determine if rendering to left or right eye
    private void Awake()
    {
        if (GetComponent<Camera>().stereoTargetEye == StereoTargetEyeMask.Left)
                isLeftEye = true;
        else { isLeftEye = false; } 

    }

    // On Start: Get screen shader and platform dimensions 
    void Start()
    {

        shader = Shader.Find("Custom/OpaqueMask");

        // Disable the image effect if the shader can't 
        // run on the users graphics card 
        if (shader == null || !shader.isSupported)
            enabled = false;

        // Grab platform/HMD information from the Game Manager GO 
        //int num = PlatformDefines.HMD_id;  
        GameObject gameManager = GameObject.FindGameObjectWithTag("Manager");
        HMD = gameManager.GetComponent<PlatformDefines>();
        SetScaleFactor();
    }

    

    void OnDisable()
    {
        if (m_Material)
        {
            DestroyImmediate(m_Material);
        }
    }


    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!enabled)
        {
            Graphics.Blit(source, destination);
            return;
        }

        material.SetColor("_MaskColor", maskColor);
        material.SetFloat("_MaskValue", maskValue);
        material.SetFloat("_MaskSpread", maskSpread);
        material.SetTexture("_MainTex", source);
        material.SetTexture("_MaskTex", maskTexture);

        material.SetFloat("_XScale", x_scale);
        material.SetFloat("_YScale", y_scale);
        material.SetFloat("_XTrans", x_trans);
        material.SetFloat("_YTrans", y_trans);

        if (material.IsKeywordEnabled("INVERT_MASK") != maskInvert)
        {
            if (maskInvert)
                material.EnableKeyword("INVERT_MASK");
            else
                material.DisableKeyword("INVERT_MASK");
        }

        Graphics.Blit(source, destination, material);
    }



    public void SetMask(int num)
    {
        string texImage = "";
        switch (num)
        {
            case 0:
                texImage = "_black";
                break;
             case 1:
                texImage = (isLeftEye) ? "central_scotoma_simulated_left_clamp30_gaussian" : "central_scotoma_simulated_right_clamp30_gaussian";
                break;
            case 2:
                texImage = (isLeftEye) ? "peripheral_loss_left_approx" : "peripheral_loss_right_approx";
                break;               
            // case 1:
            //     texImage = (isLeftEye) ? "homonymous_hemianopia_incomplete_left_clamp30_gaussian" : "homonymous_hemianopia_incomplete_right_clamp30_gaussian";
            //     break;
            //case 2:
            //    texImage = (isLeftEye) ? "homonymous_hemianopia_complete_left_clamp30_gaussian" : "homonymous_hemianopia_complete_right_clamp30_gaussian";
            //    break;
            //case 3:
            //    texImage = (isLeftEye) ? "bitemporal_loss_simulated_left_clamp30_gaussian" : "bitemporal_loss_simulated_right_clamp30_gaussian";
            //    break;
            // case 4:
            //    texImage = (isLeftEye) ? "central_scotoma_simulated_left_clamp30_gaussian" : "central_scotoma_simulated_right_clamp30_gaussian";
            //    break;
            default:
                texImage = "_black";
                break;
        }

        maskTexture = Resources.Load(texImage, typeof(Texture2D)) as Texture2D;
        //Debug.Log (maskTexture);
    }


    void SetScaleFactor()
    {
        // Because I use HVFA data and the 30-2 protocol... I need to make sure my texture map
        // is displayed at the center 60 degrees FoV 
        // Consider how you would make this function generalizable for any input
        // --> ex: as to input FoV 
        float x_pixel_count = HMD.myHMD.screen_dimension_x * (maskFOV / HMD.myHMD.fov_x);
        float y_pixel_count = HMD.myHMD.screen_dimension_y * (maskFOV / HMD.myHMD.fov_y);
        x_scale = HMD.myHMD.fov_x / maskFOV;
        y_scale = HMD.myHMD.fov_y / maskFOV;

        x_trans = (HMD.myHMD.screen_dimension_x - x_pixel_count) / 2;
        y_trans = (HMD.myHMD.screen_dimension_y - y_pixel_count) / 2;
        x_trans /= HMD.myHMD.screen_dimension_x;
        y_trans /= HMD.myHMD.screen_dimension_y;
        x_trans *= x_scale;
        y_trans *= y_scale;

        Debug.Log("pixelcnt:  " + x_pixel_count + ", " + y_pixel_count);
        Debug.Log("scale:  " + x_scale + ", " + y_scale);        
        //Debug.Log ("trans:  " + x_trans + ", " +  y_trans);
    }

}
