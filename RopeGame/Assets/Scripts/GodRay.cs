using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GodRay : MonoBehaviour
{
    public Light Sun;
    public Vector3 OffsetRotation;
    // Start is called before the first frame update
    void Start()
    {
        


    }

    // Update is called once per frame
    void Update()
    {
        Quaternion rot = Sun.transform.rotation * Quaternion.Euler(OffsetRotation); ;
        
        transform.rotation = rot;
    }
}
