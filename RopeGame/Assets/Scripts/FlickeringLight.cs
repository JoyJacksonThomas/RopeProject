using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlickeringLight : MonoBehaviour
{
    public float MinimumIntensity;
    public float Period;
    public bool ShrinkGameObject;
    public GameObject LightObject;

    Light pointLight;

    float startingIntensity;
    float intensityLerp;
    float randomTimeOffset;
    float currentIntensity;
    float minimumObjectSize;


    // Start is called before the first frame update
    void Start()
    {
        pointLight = GetComponent<Light>();
        startingIntensity = pointLight.intensity;
        randomTimeOffset = Random.Range(0, 100);
        minimumObjectSize = MinimumIntensity / startingIntensity;
    }

    // Update is called once per frame
    void Update()
    {
        intensityLerp = .5f * Mathf.Sin(Time.time * Period + randomTimeOffset) + .5f;
        currentIntensity = Mathf.Lerp(MinimumIntensity, startingIntensity, intensityLerp);
        pointLight.intensity = currentIntensity;

        if ( ShrinkGameObject )
        {
            float scale = Mathf.Lerp(minimumObjectSize, 1, intensityLerp);
            LightObject.transform.localScale = new Vector3(scale, scale, scale);
        }
    }
}
