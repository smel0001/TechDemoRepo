/**
 * @author Sam Mellor
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float deceleration = 40f;
    [SerializeField]
    private float gravity = -20f;
    [SerializeField]
    private float speed = 8f;
    [SerializeField]
    private float groundDamping = 20f;
    [SerializeField]
    private float inAirDamping = 6f;

    public Ability curAbility;

    private PlayerController controller;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private bool facingRight;

    //Jumping States
    private enum JumpState
    {
        Ascend,             //Player moving up
        Descend,            //Player moving down
        Wait                //Jump cooldown
    }

    private JumpState jumpState;

    [SerializeField]
    private float jumpForce = 40f;
    [SerializeField]
    private float initialJumpForce = 5.0f;
    [SerializeField]
    private float maxJumpTime = 0.2f;

    private float currentJumpTime = 0.0f;

    void Awake()
    {
        controller = GetComponent<PlayerController>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        jumpState = JumpState.Wait;
    }

    void Update()
    {
        //Perform Normal Movement
        Jump();
        HorizontalMovement();
        controller.ApplyGravity(gravity);

        //Activate Ability
        curAbility.Activate(controller);

        //Perform Final Movement 
        controller.Move();

        //Animate
        Animate();
    }

    void Jump()
    {
        switch(jumpState)
        {
            case JumpState.Wait:
                if (InputController.Instance.Jump.Down)
                {
                    if (controller.grounded)
                    {
                        currentJumpTime = 0.0f;
                        controller.SetVerticalVelocity(initialJumpForce);
                        jumpState = JumpState.Ascend;
                    }
                }
                break;
            case JumpState.Ascend:
                if (currentJumpTime < maxJumpTime)
                {
                    if (InputController.Instance.Jump.Held)
                    {
                        currentJumpTime += Time.deltaTime;
                        controller.AddVerticalForce(jumpForce * (maxJumpTime - currentJumpTime) * 10);
                    }
                    else
                    {
                        jumpState = JumpState.Descend;
                    }
                }
                else
                {
                    jumpState = JumpState.Descend;
                }

                break;
            case JumpState.Descend:
                if (controller.grounded)
                {
                    jumpState = JumpState.Wait;
                }
                break;

        }
    }

    void HorizontalMovement()
    {
        var smoothedMovementFactor = controller.grounded ? groundDamping : inAirDamping;
        //Needs to have decay
        if (InputController.Instance.Horizontal.Value != 0f)
        {
            controller.SetHorizontalVelocity(Mathf.Lerp(controller.velocity.x, InputController.Instance.Horizontal.Value * speed, Time.deltaTime * smoothedMovementFactor));
        }
        else
        {
            controller.HorizontalDecelerate(deceleration);
        }
    }

    public void SetCurrentAbility(Ability incAbility)
    {
        curAbility.ExitAbility();
        curAbility = incAbility;
        curAbility.EnterAbility();
    }

    void Animate()
    {
        if (InputController.Instance.Horizontal.Value < 0)
        {
            spriteRenderer.flipX = true;
        }
        else if (InputController.Instance.Horizontal.Value > 0)
        {
            spriteRenderer.flipX = false;
        }

        if (controller.velocity.y > 0)
        {
            animator.Play(Animator.StringToHash("CharUp"));
        }
        else if (controller.velocity.y < 0)
        {
            animator.Play(Animator.StringToHash("CharDown"));
        }
        else if (InputController.Instance.Horizontal.Value != 0)
        {
            animator.Play(Animator.StringToHash("CharRun"));
        }
        else
        {
            animator.Play(Animator.StringToHash("CharIdle"));
        }
    }
}
