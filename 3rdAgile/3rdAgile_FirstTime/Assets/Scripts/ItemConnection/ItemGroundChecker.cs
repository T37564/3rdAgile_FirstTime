using System;
using UnityEngine;
using Fusion;

public class ItemGroundChecker : NetworkBehaviour
{
    [Header("納品エリアのレイヤー")]
    [SerializeField] private LayerMask deliveryOfMaterialsArea;

    [Header("接地確認用のレイの長さ")]
    [SerializeField] private float rayLength = 1f;

    [Header("アイテムの大きさ")]
    [SerializeField] private Vector3 boxSize = new Vector3(1.0f, 1.0f, 1.0f);

    // アイテムが納品エリアに完全に接地している時に通知するイベント
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

            // sampleMasterDataにあるアイテムの売却値を取得する
            int amount = itemDataStorage.sampleMasterData.GetInt("Amount");

            Debug.Log("アイテムが完全に納品エリアに接地しています。");

            //OnGroundedStateChanged?.Invoke(amount);
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
        //アイテムの中心位置を取得
        Vector3 center = transform.position;

        // アイテムの中心から四隅に向かってレイを飛ばすためのオフセットを計算
        float halfX = boxSize.x * 0.5f;
        float halfY = boxSize.y * 0.5f;
        float halfZ = boxSize.z * 0.5f;

        // オブジェクトの四隅から下に向かってレイを飛ばすためのベクトルを定義
        Vector3[] checkPoints =
        {
            center + new Vector3(halfX,-halfY, halfZ), // 前右
            center + new Vector3(-halfX,-halfY, halfZ), // 前左
            center + new Vector3(halfX,-halfY, -halfZ), // 後右
            center + new Vector3(-halfX,-halfY, -halfZ) // 後左
        };

        // checkPointsのVector3型配列の数だけループする
        foreach (Vector3 point in checkPoints)
        {
            // アイテムの四隅から下に向かってレイを飛ばしてboolの判定を返す
            bool isHit = Physics.Raycast(point, Vector3.down, rayLength, deliveryOfMaterialsArea);

            // isHitがfalseの場合メソッドをfalseを返して終了する
            if (!isHit)
            {
                return false;
            }
        }

        return true;
    }
}
