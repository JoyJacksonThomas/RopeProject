using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct SkyVariables
{
    public Material SkyMat;
    public float EulerX;
}

public class DayNightCycleScipt : MonoBehaviour
{
    public SkyVariables[] SkyVars;
    public Light SunLight;
    Transform SunTrans;
    public Material SkyBoxMaterial;
    public Material CloudMaterial;
    float eulerX;
    public Vector3 SunForward;
    public float LerpVal;


    // Start is called before the first frame update
    void Start()
    {
        SunTrans = SunLight.transform;
    }

    // Update is called once per frame
    void Update()
    {
        eulerX = SunTrans.eulerAngles.x;
        SunForward = SunTrans.forward;

        if (SunForward.y <= 0 && SunTrans.forward.z <= 0)
        {
            eulerX = Mathf.Abs(eulerX % 90 - 90) + 90 ;
        }
        else if (SunForward.y >= 0 && SunTrans.forward.z <= 0)
        {
            eulerX = Mathf.Abs(eulerX % 90 - 90) + 180;
        }

       

        LerpMaterialProperties();
    }

    void LerpMaterialProperties()
    {
        for(int i = 0; i < SkyVars.Length - 1; i++)
        {
            if (eulerX >= SkyVars[i].EulerX && eulerX <= SkyVars[i + 1].EulerX)
            {
                LerpVal = (eulerX - SkyVars[i].EulerX) / (SkyVars[i + 1].EulerX - SkyVars[i].EulerX);
                SkyBoxMaterial.Lerp(SkyVars[i].SkyMat, SkyVars[i + 1].SkyMat, LerpVal);
                
                Color sunColor = SkyBoxMaterial.GetColor("_SunColor");
                Color skyColor = SkyBoxMaterial.GetColor("_SkyColor");
                Color horizonColor = SkyBoxMaterial.GetColor("_HorizonColor");

                SunLight.color = sunColor;
                
                Color cloudSunColor = CloudMaterial.GetColor("_SunColor");
                Color cloudSkyColor = CloudMaterial.GetColor("_SkyColor");
                Color cloudHorizonColor = CloudMaterial.GetColor("_HorizonColor");


                // CLOUD HORIZON COLOR
                float horizonHue, horizonSaturation, horizonValue, dummyH, dummyS, dummyV;   
                Color.RGBToHSV(horizonColor, out horizonHue, out dummyS, out dummyV);
                Color.RGBToHSV(cloudHorizonColor, out dummyH, out horizonSaturation, out horizonValue);

                horizonSaturation = Mathf.Abs(Vector3.Dot(SunTrans.forward, Vector3.forward)) * .38f;

                cloudHorizonColor = Color.HSVToRGB(horizonHue, horizonSaturation, horizonValue);

                CloudMaterial.SetColor("_HorizonColor", cloudHorizonColor);

                // CLOUD SKY COLOR
                Color.RGBToHSV(skyColor, out horizonHue, out dummyS, out dummyV);
                Color.RGBToHSV(cloudSkyColor, out dummyH, out horizonSaturation, out horizonValue);

                //horizonSaturation = Mathf.Abs(Vector3.Dot(SunTrans.forward, Vector3.forward)) * .38f;

                cloudSkyColor = Color.HSVToRGB(horizonHue, horizonSaturation, horizonValue);

                CloudMaterial.SetColor("_SkyColor", cloudSkyColor);
            }
        }
    }
}
