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

public enum MovementState{
    DEFAULT,
    SWINGING
}
public class TPS_PlayerController : MonoBehaviour
{
    Rigidbody rb;

    public Camera myCamera;
    public TPS_CameraController cameraController;
    public float MaxMoveSpeed = 1f;
    float currentMaxMoveSpeed = 1f;
    public float TimeToStop = 1f;

    public float MoveAcceleration = 1f;
    public float GroundedStopSpeed = 1f;
    public float Sensitivity = 2f;



    public bool Grounded;
    public float JumpForce, TallerJumpForce;

    public MovementState moveState = MovementState.DEFAULT;

    //public float RisingMass;
    //public float FallingMass;

    float horizontal, vertical, mouseX, mouseY;

    bool jump;
    public float jumpPressedTime;
    public float timeForTallerJump;

    CapsuleCollider col;
    public LayerMask groundLayers;

    public int playerId = 0;
    private Rewired.Player player { get { return Rewired.ReInput.players.GetPlayer(playerId); } }

    public int ropeIndex;
    public ObiRope Rappel;
    public Transform Tracer;

    public PlayerExtendableGrapplingHook Hook;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        col = GetComponent<CapsuleCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        Grounded = IsGrounded();

        input();

        if (Grounded && Mathf.Abs(horizontal) < .1f && Mathf.Abs(vertical) < .1f)
        {
            currentMaxMoveSpeed = Mathf.MoveTowards(currentMaxMoveSpeed, 0, TimeToStop);
        }
        else
        {
            currentMaxMoveSpeed = MaxMoveSpeed;
        }


    }

    private void FixedUpdate()
    {
        switch(moveState)
        {
            case MovementState.DEFAULT: 
                moveDefault();
                break;
            case MovementState.SWINGING:
                moveSwinging();
                break;
        }
        
    }

    void moveDefault()
    {
        rb.AddForce((myCamera.transform.right * horizontal +
            new Vector3(myCamera.transform.forward.x, 0, myCamera.transform.forward.z).normalized * vertical) * MoveAcceleration,
            ForceMode.Acceleration);

        if (rb.velocity.sqrMagnitude > currentMaxMoveSpeed * currentMaxMoveSpeed)
        {
            Vector3 horizontalVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            //rb.velocity = horizontalVel.normalized * currentMaxMoveSpeed + new Vector3(0, rb.velocity.y, 0);
        }


        if (rb.velocity.x != 0 || rb.velocity.z != 0)
        {
            Quaternion toRot = Quaternion.LookRotation(new Vector3(rb.velocity.x, 0, rb.velocity.z), Vector3.up);
            toRot.eulerAngles = new Vector3(0, toRot.eulerAngles.y, 0);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRot, 15);
        }


        if (jump && Grounded)
        {
            Vector3 vel = rb.velocity;
            vel.y = 0;

            rb.velocity = vel;

            rb.AddForce(new Vector3(0, JumpForce, 0), ForceMode.Impulse);
            jump = false;

        }

        if (!Grounded && jumpPressedTime >= timeForTallerJump)
        {
            rb.AddForce(new Vector3(0, TallerJumpForce, 0), ForceMode.Acceleration);
            jumpPressedTime = 0;
        }

        //if (rb.velocity.y <= 0)
        //{
        //    rb.AddForce(new Vector3(0, -RisingMass, 0), ForceMode.VelocityChange);
        //}
        //else
        //{
        //    rb.AddForce(new Vector3(0, -FallingMass, 0), ForceMode.VelocityChange);

        //}

        cameraController.AddRotation(-mouseY, mouseX, 0, Sensitivity);
    }

    void moveSwinging()
    {
        rb.AddForce((myCamera.transform.right * horizontal +
           new Vector3(myCamera.transform.forward.x, 0, myCamera.transform.forward.z).normalized * vertical) * MoveAcceleration,
           ForceMode.Acceleration);

        if (rb.velocity.sqrMagnitude > currentMaxMoveSpeed * currentMaxMoveSpeed)
        {
            Vector3 horizontalVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            //rb.velocity = horizontalVel.normalized * currentMaxMoveSpeed + new Vector3(0, rb.velocity.y, 0);
        }


        if (rb.velocity.x != 0 || rb.velocity.z != 0)
        {
            Vector3 velocityFlatPlane = new Vector3(rb.velocity.x, 0, rb.velocity.z).normalized;
            

            transform.up = Hook.GetRopeDownVector(9);
            Quaternion toRot = Quaternion.LookRotation(velocityFlatPlane, Vector3.up);
            toRot.eulerAngles = new Vector3(0, toRot.eulerAngles.y, 0);
            transform.Rotate(new Vector3(0, toRot.eulerAngles.y, 0), Space.Self);
            //transform.rotation = Quaternion.RotateTowards(transform.rotation, toRot, 30);
        }


        if (jump && Grounded)
        {
            Vector3 vel = rb.velocity;
            vel.y = 0;

            rb.velocity = vel;

            rb.AddForce(new Vector3(0, JumpForce, 0), ForceMode.Impulse);
            jump = false;

        }

        if (!Grounded && jumpPressedTime >= timeForTallerJump)
        {
            rb.AddForce(new Vector3(0, TallerJumpForce, 0), ForceMode.Acceleration);
            jumpPressedTime = 0;
        }

        //if (rb.velocity.y <= 0)
        //{
        //    rb.AddForce(new Vector3(0, -RisingMass, 0), ForceMode.VelocityChange);
        //}
        //else
        //{
        //    rb.AddForce(new Vector3(0, -FallingMass, 0), ForceMode.VelocityChange);

        //}


        cameraController.AddRotation(-mouseY, mouseX, 0, Sensitivity);
    }

    void input()
    {
        horizontal = player.GetAxis("MoveHorizontal");
        vertical = player.GetAxis("MoveVertical");


        if (player.GetButtonDown("Jump") && Grounded)
        {
            jump = true;
            jumpPressedTime = 0;
        }
        else if (player.GetButton("Jump"))
        {
            jumpPressedTime += Time.deltaTime;
        }



        mouseX = player.GetAxis("LookHorizontal");
        mouseY = player.GetAxis("LookVertical");

    }

    private bool IsGrounded()
    {
        return Physics.CheckCapsule(col.bounds.center, new Vector3(col.bounds.center.x, col.bounds.min.y, col.bounds.center.z), col.radius * .9f, groundLayers);
    }


}