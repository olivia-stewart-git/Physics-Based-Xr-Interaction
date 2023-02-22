using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class XrMovementManager : MonoBehaviour
{
    public bool logEvents = true;
    private GameLogger g_Logger;
    public enum MoveMode { headDirection, handDirection, armSwinger }

    [Header("Movement settings")]
    public MoveMode xrMovementMode;
    public Transform moveTarget; //what we are actually movings
    public Rigidbody playerRigidBody;
    [Space]
    public Transform headTransform;
    public Transform moveHand;

    [Header("Movement Values")]
    public float targetSpeed = 10f;
    public float blockairControlMultiplier = 0.5f;
    public LayerMask groundedLayerMask;
    [Space]
    public bool canJump = true;
    public float jumpForce = 15f;
    public float jumpVelocityMultiplier = 1f;

    //private variables
    private Vector2 moveDelta = Vector2.zero;

    private bool grounded;
    private bool lastGrounded = false;
    public float groundedCheckRadius;

    private float lastJump;
    private float jumpCooldown = 0.1f;

    private void Start()
    {
        if (logEvents)
        {
            g_Logger = GetComponent<GameLogger>();
        }
    }

    //takes input from XRControlManager
    public void UpdateMoveDelta(Vector2 mDelta)
    {
        moveDelta = mDelta;

    }

    public void UpdateMovement()
    {
        //update values
        grounded = Physics.CheckSphere(moveTarget.transform.position, groundedCheckRadius, groundedLayerMask);
        Debug.Log(grounded + " grounded sstate");
        if(lastGrounded == false && grounded == true)
        {
            Land();
        }
        lastGrounded = grounded;
        //accelarate to speed    
        MovePlayer();
    }

    void MovePlayer()
    {
        Vector3 targDirFor = headTransform.forward;
        targDirFor.y = 0f;
        Vector3 targDirRight = headTransform.right;
        targDirRight.y = 0f;
        Vector3 move = targDirRight * moveDelta.x + targDirFor * moveDelta.y;
       // Debug.Log(move + "move value");

        float toUseS = targetSpeed;
        if (!grounded)
        {
            toUseS *= blockairControlMultiplier;
        }      
        playerRigidBody.AddForce((move.normalized * toUseS) * Time.deltaTime, ForceMode.VelocityChange);
    }

    public void CheckToJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("Check jump");
            if (!canJump) return;
            if (grounded && Time.time > lastJump)
            {
                Jump();
            }
        }
    }

    void Jump()
    {
        Debug.Log("PlayerJumped");
        lastJump = Time.time + jumpCooldown;
        Vector3 rawVelocity = playerRigidBody.velocity;
        rawVelocity.y = 0f;
        rawVelocity *= jumpVelocityMultiplier;
        playerRigidBody.AddForce((Vector3.up * jumpForce) + rawVelocity, ForceMode.Impulse);
    }

    void Land()
    {
        //play landing sound
    }


}
