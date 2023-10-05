using UnityEngine;

public class CausticShaderSupport : MonoBehaviour
{
    public Material causticsMaterial; // Assign your caustics material in the Inspector

    void Update()
    {
        // Check if the sun exists in the scene
        if (RenderSettings.sun != null)
        {
            var sunMatrix = RenderSettings.sun.transform.localToWorldMatrix;
            causticsMaterial.SetMatrix("_MainLightDirection", sunMatrix);
        }
        else
        {
            Debug.LogError("Sun (main light) not found in the scene.");
        }
    }
}