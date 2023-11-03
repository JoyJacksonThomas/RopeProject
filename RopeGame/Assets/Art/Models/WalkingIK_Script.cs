using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WalkingIK_Script : MonoBehaviour
{
    public Transform LeftKneeTrans;
    public Transform RightKneeTrans;

    public Transform LeftFootTarget;
    public Transform RightFootTarget;

    public float RayDistance;
    public LayerMask Mask;

    public float OffSetY;

    public Animator Animator;
    public UnityEngine.Animations.Rigging.TwoBoneIKConstraint LeftFootConstraint;
    public UnityEngine.Animations.Rigging.TwoBoneIKConstraint RightFootConstraint;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        int layerMask = 1 << 8;

        // This would cast rays only against colliders in layer 8.
        // But instead we want to collide against everything except layer 8. The ~ operator does this, it inverts a bitmask.
        layerMask = ~layerMask;

        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(LeftKneeTrans.position, Vector3.down, out hit, RayDistance, Mask))
        {
            Debug.DrawRay(LeftKneeTrans.position, Vector3.down * hit.distance, Color.yellow);
            Debug.Log("Did Hit");

            LeftFootTarget.position = LeftKneeTrans.position + Vector3.down * (hit.distance - OffSetY);
            LeftFootTarget.up = hit.normal;

        }
        else
        {
            Debug.DrawRay(LeftKneeTrans.position, Vector3.down * RayDistance, Color.white);
            Debug.Log("Did not Hit");
        }

        if (Physics.Raycast(RightKneeTrans.position, Vector3.down, out hit, RayDistance, layerMask))
        {
            Debug.DrawRay(RightKneeTrans.position, Vector3.down * hit.distance, Color.yellow);
            Debug.Log("Did Hit");

            RightFootTarget.position = RightKneeTrans.position + Vector3.down * (hit.distance - OffSetY);
            RightFootTarget.up = hit.normal;

        }
        else
        {
            Debug.DrawRay(RightKneeTrans.position, Vector3.down * RayDistance, Color.white);
            Debug.Log("Did not Hit");
        }

        LeftFootConstraint.weight = Animator.GetFloat("IK_LeftFootWeight");
        RightFootConstraint.weight = Animator.GetFloat("IK_RightFootWeight");
    }
}
