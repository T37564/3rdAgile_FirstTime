using System;
using UnityEngine;
using Fusion;

public class ItemGroundChecker : NetworkBehaviour
{
    [SerializeField] private LayerMask deliveryOfMaterialsArea;

    [SerializeField] private float rayLength = 1f;

    [SerializeField] private Vector3 boxSize = new Vector3(1.0f, 1.0f, 1.0f);

    public static Action<int> OnGroundedStateChanged;

    private ItemDataStorage itemDataStorage;

    private bool isSold = false;

    public override void Spawned()
    {
        itemDataStorage = GetComponent<ItemDataStorage>();
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        if (isSold) return;

        if (IsFullyGrounded())
        {
            Debug.Log("アイテムが完全に納品エリアに接地していることを確認");
            isSold = true;

            int amount = itemDataStorage.sampleMasterData.GetInt("Amount");

            Debug.Log("アイテムが完全に納品エリアに接地しています。");

            //OnGroundedStateChanged?.Invoke(itemDataStorage.sampleMasterData.GetInt("Amount"));
            //Debug.Log("アイテムのAmount: " + itemDataStorage.sampleMasterData.GetInt("Amount"));
            OnGroundedStateChanged?.Invoke(amount);
            Debug.Log("アイテムのAmount: " + amount);
            //gameObject.SetActive(false);
            Runner.Despawn(Object);
        }
    }

    /// <summary>
    /// アイテムが完全に納品エリアに接地しているかを確認する
    /// </summary>
    private bool IsFullyGrounded()
    {
        Vector3 center = transform.position;

        float halfX = boxSize.x * 0.5f;
        float halfY = boxSize.y * 0.5f;
        float halfZ = boxSize.z * 0.5f;

        Vector3[] checkPoints =
        {
            center + new Vector3(halfX,-halfY, halfZ), // 前右
            center + new Vector3(-halfX,-halfY, halfZ), // 前左
            center + new Vector3(halfX,-halfY, -halfZ), // 後右
            center + new Vector3(-halfX,-halfY, -halfZ) // 後左
        };

        foreach (Vector3 point in checkPoints)
        {
            bool isHit = Physics.Raycast(point, Vector3.down, rayLength, deliveryOfMaterialsArea);

            if (!isHit)
            {
                return false;
            }
        }

        return true;

    }
}
