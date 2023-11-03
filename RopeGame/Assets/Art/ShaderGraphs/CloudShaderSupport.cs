using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudShaderSupport : MonoBehaviour
{
    public Material cloudMaterial; // Assign your caustics material in the Inspector

    void Update()
    {
        // Check if the sun exists in the scene
        if (RenderSettings.sun != null)
        {
            
            cloudMaterial.SetVector("_SunDirection", RenderSettings.sun.transform.forward);
            //cloudMaterial.SetVector("_ViewDirection", Camera.current.transform.forward);
        }
        else
        {
            Debug.LogError("Sun (main light) not found in the scene.");
        }
    }
}
