using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillBoard : MonoBehaviour
{
    public Camera mainCamera;

    public Vector3 Offset;
    private void Start()
    {
        mainCamera = Camera.main;
    }
    void LateUpdate()
    {
        transform.LookAt(mainCamera.transform);
        transform.Rotate(Offset);
    }
}
