//===========================================================================================
// 納品場所のクラス
// ScoreManagerに、納品されたアイテムのポイントを送る
// 製作者：スズキ
//===========================================================================================

using Network.Player;
using UnityEngine;
using Fusion;
using System.Collections.Generic;

/// <summary>
/// 納品場所のクラス
/// 運ばれてきたアイテムの持つポイントを取得し、トータルポイントに加算する処理はここに実装
/// トータルのスコアを持つクラスは別クラスが理想か？
///  → イメージ）シングルトンクラス ScoreManagerに総ポイント用変数、加算メソッドを実装
///                 加算メソッドを呼び出し、StateAuthorityが判断し処理
/// </summary>
public class DeliveryLocation : NetworkBehaviour
{
    [Header("アイテムを探すためのパラメータ")]
    [SerializeField] private float searchRadius = 3.0f;
    [SerializeField] private float scanInterval = 0.1f; // アイテムを探す間隔

    [Header("レイヤーマスク")]
    [SerializeField] private LayerMask itemLayerMask; // アイテムのレイヤーマスク
    [SerializeField] private LayerMask playerLayerMask; // プレイヤーのレイヤーマスク

    [Header("最大検出数")]
    [SerializeField] private int maxItemCount = 16;
    [SerializeField] private int maxPlayerCount = 16;

    private Collider[] itemHits;
    private Collider[] playerHits;

    private float scanTimer = 0f;

    private GameObject currentItemObject;
    private NetworkObject currentItemNetworkObject;
    private ItemInteractable currentItem;
    private ItemDataStorage currentItemStorage;

    private void Awake()
    {
        tag = TagName.DELIVERY_BOX;

        itemHits = new Collider[maxItemCount];
        playerHits = new Collider[maxPlayerCount];
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;
        Debug.Log("ホストさんよろしくお願いします。");

        scanTimer -= Runner.DeltaTime;

        if (scanTimer > 0.0f) return;

        scanTimer = scanInterval;

        UpdateCurrentItem();

        if (currentItem == null) return;
        Debug.Log("currentItemはnullじゃないよ～");

        int deliveryPlayerCount = CountDeliveryPlayers();

        if(deliveryPlayerCount < currentItem.RequiredPeople) return;
        Debug.Log("納品可能かな");

        DeliveryCurrentItem();
    }

    private void UpdateCurrentItem()
    {
        if (IsCurrentItemValid()) return;
        Debug.Log("returnしません！");

        ClearItem();

        int hitCount = Physics.OverlapSphereNonAlloc(
            transform.position,
            searchRadius,
            itemHits,
            itemLayerMask
        );
        Debug.Log($"hitCount: {hitCount}");

        float nearestSqrDistance = float.MaxValue;
        GameObject nearestItem = null;

        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = itemHits[i];
            Debug.Log("loop Start");
            if (hit == null) continue;
            Debug.Log("Collider field in not null");
            if (!hit.CompareTag(TagName.ITEM)) continue;
            Debug.Log("Successfully! Get Tag");

            float sqrDistance = (hit.transform.position - transform.position).sqrMagnitude;

            if (sqrDistance < nearestSqrDistance)
            {
                nearestSqrDistance = sqrDistance;
                nearestItem = hit.gameObject;
            }
            Debug.Log("loop Count" + i);
        }
        Debug.Log($"nearestItem: {nearestItem}");
        if (nearestItem != null)
        {
            Debug.Log("アイテム取得できてるよ！");
            TrySetItem(nearestItem);
        }
    }

    private bool IsCurrentItemValid()
    {
        if (currentItemObject == null) return false;
        if (currentItemNetworkObject == null) return false;
        Debug.Log("currentItemObjectもcurrentItemNetworkObjectもnullじゃないよ");

        float sqrDistance = (currentItemObject.transform.position - transform.position).sqrMagnitude;
        return sqrDistance <= searchRadius * searchRadius;
    }

    private void TrySetItem(GameObject itemObject)
    {
        if (!itemObject.TryGetComponent(out NetworkObject networkObject)) return;
        if (!itemObject.TryGetComponent(out ItemInteractable item)) return;
        if (!itemObject.TryGetComponent(out ItemDataStorage storage)) return;
        Debug.Log("Component取得オールクリア！");

        currentItemObject = itemObject;
        currentItemNetworkObject = networkObject;
        currentItem = item;
        currentItemStorage = storage;
    }

    private int CountDeliveryPlayers()
    {
        int hitCount = Physics.OverlapSphereNonAlloc(
            transform.position,
            searchRadius,
            playerHits,
            playerLayerMask
        );

        int count = 0;

        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = playerHits[i];

            if(hit == null) continue;
            if(!hit.CompareTag(TagName.PLAYER)) continue;
            if (!hit.TryGetComponent(out PlayerController player)) continue;

            if (!player.IsHoldingItem) continue;

            count++;
        }

        Debug.Log($"count：{count}");
        return count;
    }

    private void DeliveryCurrentItem()
    {
        if (currentItemStorage == null) return;
        if (currentItemNetworkObject == null) return;
        Debug.Log("カレントアイテムっ！！！");

        int score = currentItemStorage.itemData.GetInt("Amount");

        //ScoreManager.Instance.AddScore(score);
        MoneyManager.Instance.AddAmount(score);

        Runner.Despawn(currentItemNetworkObject);

        ClearItem();
    }

    private void ClearItem()
    {
        currentItemObject = null;
        currentItemNetworkObject = null;
        currentItem = null;
        currentItemStorage = null;
        Debug.Log("クリア～");
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, searchRadius);
    }
#endif

    
}
