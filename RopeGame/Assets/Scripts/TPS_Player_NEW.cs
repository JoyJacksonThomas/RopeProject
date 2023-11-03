using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired.ControllerExtensions;
using Microsoft.Win32;

public class TPS_Player_NEW : MonoBehaviour
{
    public CharacterController Controller;
    public float Speed;
    
    public Transform MainCamera;
    public TPS_CameraController CameraController;
    public float LookSensitivity;
    float mouseX, mouseY;

    float gravity = -9.81f;
    public float GravityMultiplier;
    float verticalVelocity;

    public int playerId = 0;
    private Rewired.Player player { get { return Rewired.ReInput.players.GetPlayer(playerId); } }

    public Animator Animator;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float horizontal = player.GetAxis("MoveHorizontal");
        float vertical = player.GetAxis("MoveVertical");
        Vector3 direction = new Vector3(horizontal, 0, vertical).normalized;

        Animator.SetFloat("HorizontalMovement", direction.magnitude);

        if (direction.magnitude > .1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + MainCamera.eulerAngles.y;
            transform.rotation = Quaternion.Euler(0, targetAngle, 0);

            Vector3 moveDirection = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
            Controller.Move(moveDirection.normalized * Speed * Time.deltaTime);

            
        }

        ApplyGravity();

        mouseX = player.GetAxis("LookHorizontal");
        mouseY = player.GetAxis("LookVertical");

        CameraController.AddRotation(-mouseY, mouseX, 0, LookSensitivity);
    }

    void ApplyGravity()
    {
        if(Controller.isGrounded)
        {
            verticalVelocity = -1f;
        }
        else
        {
            verticalVelocity += gravity * GravityMultiplier * Time.deltaTime;
        }

        Controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        
    }
}
