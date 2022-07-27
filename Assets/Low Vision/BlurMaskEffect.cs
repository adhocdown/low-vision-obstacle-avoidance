using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class BlurMaskEffect : MonoBehaviour
{
    // Variables for Platform Defines 
    PlatformDefines HMD;

    // Variables for Blur  
    [Range(0, 2)]
    public int downsample = 1;
    [Range(0.0f, 10.0f)]
    public float blurSize = 3.0f;
    [Range(0, 4)]
    public int blurIterations = 2;

    // Variables for Mask Materials 
    public Shader shader;
    public Material blurMaterial;
    public Color maskColor = Color.black;
    public Texture2D maskTexture;

    // Screen position variables 
    bool isLeftEye;
    public float x_scale;
    public float y_scale;
    public float x_trans;
    public float y_trans;

    void Awake()
    {
        //blurMaterial = new Material(Shader.Find("Custom/FastMaskBlur"));
        blurMaterial = new Material(shader);

        if (GetComponent<Camera>().stereoTargetEye == StereoTargetEyeMask.Left)
            isLeftEye = true;
        else { isLeftEye = false; }
    }

    private void Start()
    {
        // Grab platform/HMD information from the Game Manager GO 
        GameObject gameManager = GameObject.FindGameObjectWithTag("Manager");
        HMD = gameManager.GetComponentInParent<PlatformDefines>();
        Debug.Log(HMD);

        // scale 
        SetScaleFactor();
    }

    // This function handles the screen shader 
    public void OnRenderImage(RenderTexture source, RenderTexture destination)
    {

        // No blur
        if (blurIterations == 0 && blurSize == 0 && downsample == 0)
        {
            Graphics.Blit(source, destination);
            return;
        }

        // Yes blur 
        float widthMod = 1.0f / (1.0f * (1 << downsample));
        blurMaterial.SetVector("_Parameter", new Vector4(blurSize * widthMod, -blurSize * widthMod, 0.0f, 0.0f));
        source.filterMode = FilterMode.Bilinear;

        // rt = downsample screenshot  
        int rtW = source.width >> downsample;
        int rtH = source.height >> downsample;
        RenderTexture rt = RenderTexture.GetTemporary(rtW, rtH, 0, source.format);

        // Blit to rt
        rt.filterMode = FilterMode.Bilinear;
        Graphics.Blit(source, rt);

        for (int i = 0; i < blurIterations; i++)
        {

            float iterationOffs = (i * 1.0f);
            blurMaterial.SetVector("_Parameter", new Vector4(blurSize * widthMod + iterationOffs, -blurSize * widthMod - iterationOffs, 0.0f, 0.0f));

            // vertical blur
            RenderTexture rt2 = RenderTexture.GetTemporary(rtW, rtH, 0, source.format);
            rt2.filterMode = FilterMode.Bilinear;
            Graphics.Blit(rt, rt2, blurMaterial, 1);
            RenderTexture.ReleaseTemporary(rt);
            rt = rt2;

            // horizontal blur
            rt2 = RenderTexture.GetTemporary(rtW, rtH, 0, source.format);
            rt2.filterMode = FilterMode.Bilinear;
            Graphics.Blit(rt, rt2, blurMaterial, 2);
            RenderTexture.ReleaseTemporary(rt);
            rt = rt2;
        }

        Graphics.Blit(rt, destination);
        RenderTexture.ReleaseTemporary(rt);
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
                texImage = (isLeftEye) ? "homonymous_hemianopia_incomplete_left_clamp30_gaussian" : "homonymous_hemianopia_incomplete_right_clamp30_gaussian";
                break;
            case 2:
                texImage = (isLeftEye) ? "homonymous_hemianopia_complete_left_clamp30_gaussian" : "homonymous_hemianopia_complete_right_clamp30_gaussian";
                break;
            case 3:
                texImage = (isLeftEye) ? "bitemporal_loss_simulated_left_clamp30_gaussian" : "bitemporal_loss_simulated_right_clamp30_gaussian";
                break;
            case 4:
                texImage = (isLeftEye) ? "central_scotoma_simulated_left_clamp30_gaussian" : "central_scotoma_simulated_right_clamp30_gaussian";
                break;
            default:
                texImage = "_black";
                break;
        }
        // reassign maskTexture to appropriate scotoma 
        maskTexture = Resources.Load(texImage, typeof(Texture2D)) as Texture2D;
        SetMaskTexture(texImage);
        //Debug.Log (maskTexture);
    }

    public void SetMaskTexture(string imgName)
    {
        maskTexture = Resources.Load(imgName, typeof(Texture2D)) as Texture2D;
        blurMaterial.SetTexture("_MaskTex", maskTexture);
    }

    // Function: Scale scotoma to match pixel count and FOV of VR head-mounted display
    // Set parameters for shader (translation, scale, mask texture)
    void SetScaleFactor()
    {
        float x_pixel_count = HMD.myHMD.screen_dimension_x * (60 / HMD.myHMD.fov_x);
        float y_pixel_count = HMD.myHMD.screen_dimension_y * (60 / HMD.myHMD.fov_y);
        x_scale = HMD.myHMD.fov_x / 60;
        y_scale = HMD.myHMD.fov_y / 60;

        Debug.Log("pixelcnt:  " + x_pixel_count + ", " + y_pixel_count);
        Debug.Log("scale:  " + x_scale + ", " + y_scale);

        x_trans = (HMD.myHMD.screen_dimension_x - x_pixel_count) / 2;
        y_trans = (HMD.myHMD.screen_dimension_y - y_pixel_count) / 2;
        x_trans /= HMD.myHMD.screen_dimension_x;
        y_trans /= HMD.myHMD.screen_dimension_y;
        x_trans *= x_scale;
        y_trans *= y_scale;

        Debug.Log("trans:  " + x_trans + ", " + y_trans);

        // set parameters for shader
        blurMaterial.SetColor("_MaskColor", maskColor);
        blurMaterial.SetFloat("_XScale", x_scale);
        blurMaterial.SetFloat("_YScale", y_scale);
        blurMaterial.SetFloat("_XTrans", x_trans);
        blurMaterial.SetFloat("_YTrans", y_trans);
        blurMaterial.SetTexture("_MaskTex", maskTexture);

    }
}
