using Network.Player;
using System;
using UnityEngine;
using static Unity.Collections.Unicode;

public class TestPlayerAnimetion : MonoBehaviour
{
    // private Animator animator;



    //public override void Spawned()
    //{
    //    inputHandler = GetComponent<PlayerInputHandler>();

    //    if (Object.HasInputAuthority)
    //    {
    //        PlayerInputGetter inputGetter = FindAnyObjectByType<PlayerInputGetter>();
    //        if (inputGetter != null)
    //        {
    //            inputGetter.RegisterLocalInput(inputHandler);
    //        }
    //    }

    //    if (Object.HasStateAuthority)
    //    {
    //        IsAlive = true;
    //        IsHoldingItem = false;
    //    }

    //    prevAlive = IsAlive;
    //    prevHoldingItem = IsHoldingItem;

    //    animator = GetComponent<Animator>();
    //}

    //private void Move(Vector2 moveInput)
    //{
    //    Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y);

    //    if (move.sqrMagnitude > 1f)
    //    {
    //        move.Normalize();
    //    }

    //    transform.position += move * moveSpeed * Runner.DeltaTime;

    //    // =========================
    //    // アニメーション制御
    //    // =========================
    //    bool isMoving = move.sqrMagnitude > 0.01f;

    //    bool isRunning = moveInput.magnitude > 0.8f;

    //    animator.SetBool("Walk", isMoving && !isRunning);

    //    animator.SetBool("Run", isRunning);
    //}

}
