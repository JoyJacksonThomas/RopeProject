#region Defines
#if UNITY_2020 || UNITY_2021 || UNITY_2022 || UNITY_2023 || UNITY_2024 || UNITY_2025
#define UNITY_2020_PLUS
#endif
#if UNITY_2019 || UNITY_2020_PLUS
#define UNITY_2019_PLUS
#endif
#if UNITY_2018 || UNITY_2019_PLUS
#define UNITY_2018_PLUS
#endif
#if UNITY_2017 || UNITY_2018_PLUS
#define UNITY_2017_PLUS
#endif
#if UNITY_5 || UNITY_2017_PLUS
#define UNITY_5_PLUS
#endif
#if UNITY_5_1 || UNITY_5_2 || UNITY_5_3_OR_NEWER || UNITY_2017_PLUS
#define UNITY_5_1_PLUS
#endif
#if UNITY_5_2 || UNITY_5_3_OR_NEWER || UNITY_2017_PLUS
#define UNITY_5_2_PLUS
#endif
#if UNITY_5_3_OR_NEWER || UNITY_2017_PLUS
#define UNITY_5_3_PLUS
#endif
#if UNITY_5_4_OR_NEWER || UNITY_2017_PLUS
#define UNITY_5_4_PLUS
#endif
#if UNITY_5_5_OR_NEWER || UNITY_2017_PLUS
#define UNITY_5_5_PLUS
#endif
#if UNITY_5_6_OR_NEWER || UNITY_2017_PLUS
#define UNITY_5_6_PLUS
#endif
#if UNITY_5_7_OR_NEWER || UNITY_2017_PLUS
#define UNITY_5_7_PLUS
#endif
#if UNITY_5_8_OR_NEWER || UNITY_2017_PLUS
#define UNITY_5_8_PLUS
#endif
#if UNITY_5_9_OR_NEWER || UNITY_2017_PLUS
#define UNITY_5_9_PLUS
#endif
#pragma warning disable 0219
#pragma warning disable 0618
#pragma warning disable 0649
#pragma warning disable 0067
#endregion


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;
using Rewired.ControllerExtensions;
using Microsoft.Win32;

public class SwingingPlayer : MonoBehaviour
{
    Rigidbody rb;

    public Camera myCamera;
    public TPS_CameraController cameraController;
    public float MaxMoveSpeed = 1f;
    public float TimeToStop = 1f;

    public float MoveAcceleration = 1f;
    public float GroundedStopSpeed = 1f;
    public float Sensitivity = 2f;

    public float JumpForce, TallerJumpForce;

    public MovementState moveState = MovementState.DEFAULT;

    //public float RisingMass;
    //public float FallingMass;

    public float horizontal, vertical, mouseX, mouseY;

    public Transform SwingRootTrans;
    public Rigidbody RootRB;
    public float SwingRotationSpeed;
    public ObiRope rope;
    public float VelocityMinimumForTurning;
    public MeshRenderer meshRender;
    public AnimationCurve rotationCurve;
    public AnimationCurve rotationCurve2;
    public float ConstantDownForce;
    public Transform TestTrans;

    float speedWhenRopeIsDownwards = 1f;
    Vector3 eulerRotationAtSwingPeak;
    Vector3 lookTowardsAtSwingPeak;

    public LayerMask groundLayers;

    public int playerId = 0;
    private Rewired.Player player { get { return Rewired.ReInput.players.GetPlayer(playerId); } }


    public PlayerExtendableGrapplingHook Hook;

    public int numVelsForAverage;
    Vector3 avgVel = Vector3.zero;

    Queue<Vector3> velocityHistory;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        velocityHistory = new Queue<Vector3>();
    }

    // Update is called once per frame
    void Update()
    {
        input();
        

    }

    private void FixedUpdate()
    {
        if (velocityHistory.Count > numVelsForAverage)
        {
            avgVel -= velocityHistory.Dequeue();
        }
        velocityHistory.Enqueue(rb.velocity);

        avgVel += rb.velocity;

        moveSwingingWithoutHingeJoints();
    }


    void moveSwingingWithoutHingeJoints()
    {
        rb.AddForce((myCamera.transform.right * horizontal +
           new Vector3(myCamera.transform.forward.x, 0, myCamera.transform.forward.z).normalized * vertical) * MoveAcceleration,
           ForceMode.Acceleration);

        Vector3 hookPoint = rope.GetParticlePosition(rope.activeParticleCount - 1);
        rb.AddForce((transform.position - hookPoint).normalized * ConstantDownForce, ForceMode.Acceleration);

        Vector3 actualAvgVel = avgVel / velocityHistory.Count;
        Vector3 velocityFlatPlane = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        if (velocityFlatPlane.sqrMagnitude > 0)
        {

            Vector3 toHookPoint = new Vector3(transform.position.x - hookPoint.x, transform.position.y - hookPoint.y, transform.position.z - hookPoint.z);

            Vector3 forwardFlatPlane = transform.forward;
            forwardFlatPlane.y = 0;
            forwardFlatPlane.Normalize();
        
            float VecAndForwardLikeness = Vector3.Dot(forwardFlatPlane, velocityFlatPlane.normalized);
        
            if (VecAndForwardLikeness < 0)
            {
                //velocityFlatPlane = -velocityFlatPlane;
            }
            transform.up = -GetRopeDownVector(9);
            Quaternion fromRot = Quaternion.LookRotation(forwardFlatPlane, Vector3.up);
            Quaternion toRot = Quaternion.LookRotation(velocityFlatPlane, Vector3.up);

            float dotProd = Vector3.Dot(toHookPoint.normalized, Vector3.down);

            
            if (dotProd > .95f && rb.velocity.magnitude > 1f)
                speedWhenRopeIsDownwards = rb.velocity.magnitude;

            float rotationSpeedModifier = Mathf.Clamp(velocityFlatPlane.magnitude / speedWhenRopeIsDownwards, 0, 1);

            
            rotationSpeedModifier = rotationCurve2.Evaluate(rotationSpeedModifier);

            Quaternion finalRot = Quaternion.RotateTowards(fromRot, toRot, SwingRotationSpeed * rotationSpeedModifier);

            transform.Rotate(finalRot.eulerAngles, Space.Self);

            //if (rotationSpeedModifier > .04f)
            //{
            //    meshRender.material.color = Color.blue;
            //    transform.Rotate(finalRot.eulerAngles, Space.Self);
            //    eulerRotationAtSwingPeak = transform.eulerAngles;
            //    lookTowardsAtSwingPeak = hookPoint;
            //}
            //else
            //{
            //    meshRender.material.color = Color.red;
            //
            //    Vector3 toLookPointFlatPlane = lookTowardsAtSwingPeak - transform.position;
            //    
            //    toLookPointFlatPlane.y = 0;
            //    float forwardLookPointLikeness = Vector3.Dot(forwardFlatPlane, toLookPointFlatPlane.normalized);
            //
            //    if (forwardLookPointLikeness < 0)
            //    {
            //        toLookPointFlatPlane = -toLookPointFlatPlane;
            //    }
            //
            //    finalRot = Quaternion.LookRotation(toLookPointFlatPlane, Vector3.up);
            //    finalRot = Quaternion.RotateTowards(fromRot, finalRot, SwingRotationSpeed*.3f);
            //    //transform.Rotate(eulerRotationAtSwingPeak, Space.Self);
            //    transform.Rotate(finalRot.eulerAngles, Space.Self);
            //}
        }

        cameraController.AddRotation(-mouseY, mouseX, 0, Sensitivity);
    }

    void moveSwingingWithoutHingeJoints_2()
    {
        rb.AddForce((myCamera.transform.right * horizontal +
           new Vector3(myCamera.transform.forward.x, 0, myCamera.transform.forward.z).normalized * vertical) * MoveAcceleration,
           ForceMode.Acceleration);

        Vector3 velocityFlatPlane = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        if (velocityFlatPlane.sqrMagnitude > 0)
        {
            Vector3 hookPoint = rope.GetParticlePosition(rope.activeParticleCount - 1);
            Vector3 toHookPoint = new Vector3(transform.position.x - hookPoint.x, transform.position.y - hookPoint.y, transform.position.z - hookPoint.z);
            Vector3 toHookPointFlatPlane = new Vector3(transform.position.x - hookPoint.x, 0, transform.position.z - hookPoint.z);

            meshRender.material.color = Color.blue;

            Vector3 forwardFlatPlane = transform.forward;
            forwardFlatPlane.y = 0;
            forwardFlatPlane.Normalize();


            transform.up = -GetRopeDownVector(9);
            Quaternion fromRot = Quaternion.LookRotation(forwardFlatPlane, Vector3.up);
            Quaternion toRot = Quaternion.LookRotation(toHookPointFlatPlane, Vector3.up);

            float dotProd = Vector3.Dot(toHookPoint.normalized, Vector3.down);


            float lerpVal = rotationCurve.Evaluate(1 - dotProd);

            //toRot = Quaternion.RotateTowards(fromRot, toRot, SwingRotationSpeed);
            toRot = Quaternion.Lerp(fromRot, toRot, lerpVal);


            transform.Rotate(toRot.eulerAngles, Space.Self);
        }
        else
        {
            meshRender.material.color = Color.red;
        }

        cameraController.AddRotation(-mouseY, mouseX, 0, Sensitivity);
    }

    void input()
    {
        horizontal = player.GetAxis("MoveHorizontal");
        vertical = player.GetAxis("MoveVertical");



        mouseX = player.GetAxis("LookHorizontal");
        mouseY = player.GetAxis("LookVertical");

    }

    Vector3 GetRopeDownVector(int numParticlesToCalculate)
    {
        Vector3 down = Vector3.down;

        numParticlesToCalculate = Mathf.Clamp(numParticlesToCalculate, 0, rope.activeParticleCount - 1);

        for (int i = 0; i < numParticlesToCalculate; i++)
        {
            down += rope.GetParticlePosition(rope.activeParticleCount - 2 - i) - rope.GetParticlePosition(rope.activeParticleCount - 1 - i);
        }

        down /= numParticlesToCalculate;

        return down;
    }

    Vector3 getContactPointUpRope()
    {
        Vector3 contact = Vector3.zero;

        //contact = 

        return contact;
    }
}
