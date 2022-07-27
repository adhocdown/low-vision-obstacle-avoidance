﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateSkybox : MonoBehaviour
{
    public float speed; 

    void Update()
    {
        RenderSettings.skybox.SetFloat("_Rotation", (speed * Time.time));
    }
}
