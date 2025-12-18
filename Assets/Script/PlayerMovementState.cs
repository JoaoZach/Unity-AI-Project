using UnityEngine;
using System;

public class PlayerMovementState : MonoBehaviour
{
    public enum MoveState
    {
        Idle,
        Run,
        Jump,
        Fall,
        Double_Jump,
        Wall_Jump,
    }
    public MoveState CurrentMoveState {  get; private set; }

    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rigidbody;
    private const string idleAnim = "Idle";
    private const string runAnim = "Run";
    private const string jumpAnim = "Jump";
    private const string fallAnim = "Fall";
    private const string doubleJumpAnim = "Double Jump";
    private const string wallJumpingAnim = "Wall Jump";
    public static Action<MoveState> OnPlayerMoveStateChanged;
    private float xPosLastFrame;

    private void Update()
    {
        if (transform.position.x == xPosLastFrame && rigidbody.linearVelocity.y == 0)
        {
            SetMoveState(MoveState.Idle);
        }
        else if (transform.position.x != xPosLastFrame && rigidbody.linearVelocity.y == 0)
        {
            SetMoveState(MoveState.Run);
        }
        else if (rigidbody.linearVelocity.y < 0)
        {
            SetMoveState(MoveState.Fall);
        }

            xPosLastFrame = transform.position.x;
    }

    public void SetMoveState(MoveState moveState)
    {
        if (moveState == CurrentMoveState) return;

        switch (moveState)
        {
            case MoveState.Idle:
                HandleIdle();
                break;

            case MoveState.Run:
                HandleRun();
                break;

            case MoveState.Jump:
                HandleJump();
                break;

            case MoveState.Fall:
                HandleFall();
                break;

            case MoveState.Double_Jump:
                HandleDoubleJump();
                break;

            case MoveState.Wall_Jump:
                HandleWallJump();
                break;

            default:
                Debug.LogError($"Invalid movement state: {moveState}");
                break;
        }

        OnPlayerMoveStateChanged?.Invoke(moveState);
        CurrentMoveState = moveState;
    }

    private void HandleIdle()
    {
        animator.Play(idleAnim);
    }

    private void HandleRun()
    {
        animator.Play(runAnim);

    }

    private void HandleJump()
    {
        animator.Play(jumpAnim);

    }

    private void HandleFall()
    {
        animator.Play(fallAnim);

    }

    private void HandleDoubleJump()
    {
        animator.Play(doubleJumpAnim);

    }

    private void HandleWallJump()
    {
        animator.Play(wallJumpingAnim);

    }

}
