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
    [SerializeField] private float searchRadius = 3.0f;
    private LayerMask layerMask;

    private string ItemName = string.Empty;
    // アイテムのObject
    private GameObject currentItemObject = null;
    private ItemInteractable currentItem;
    private ItemDataStorage currentItemStorage;

    private readonly HashSet<PlayerController> deliveryPlayers = new();

    // 範囲内に入ったアイテムを運んでいるプレイヤーの数
    private int itemDeliveryCount = 0;


    private void Awake()
    {
        tag = TagName.DELIVERY_BOX;
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasInputAuthority) return;

        if (currentItem == null) return;

        int requiredPlayers = currentItem.RequiredPeople;

        // アイテムを運ぶのに必要な人数より範囲内に運んでいるプレイヤーが少ない場合は処理をしない
        if (deliveryPlayers.Count < requiredPlayers) return;

        // 以下で回収以降の処理を書く
        // 1.取ったアイテムを納品　アイテムのポイントをScoreManagerに送り更新をしてもらい、アイテムを削除
        //ReleaseItem();

        // 納品したアイテムを削除するためアイテムの情報は明示的に消す

        DeliveryCurrentItem();
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag(TagName.ITEM))
        {
            //GetItem(collider.gameObject);
            TrySetItem(collider.gameObject);
            return;
            // アイテムの必要人数を取る
            // colliderをアイテムのクラスにasキャストして必要人数を取るのが正解か？
        }

        if (collider.CompareTag(TagName.PLAYER))
        {
            if (collider.TryGetComponent(out PlayerController playerController))
            {
                if (playerController.IsHoldingItem)
                {
                    deliveryPlayers.Add(playerController);
                }
            }
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.CompareTag(TagName.ITEM))
        {
            if (collider.gameObject == currentItemObject)
            {
                ClearItem();
                FindNearItemObject();
            }
            return;
        }

        if (collider.CompareTag(TagName.PLAYER))
        {
            if (collider.TryGetComponent(out PlayerController playerController))
            {
                deliveryPlayers.Remove(playerController);
            }
        }
    }

    private void TrySetItem(GameObject itemObject)
    {
        if (!itemObject.TryGetComponent(out ItemInteractable deliveryItem)) return;
        if(!itemObject.TryGetComponent(out ItemDataStorage deliveryItemStrage)) return;

        currentItemObject = itemObject;
        currentItem = deliveryItem;
        currentItemStorage = deliveryItemStrage;
    }

    private void DeliveryCurrentItem()
    {
        int score = currentItemStorage.itemData.GetInt("Amount");

        ScoreManager.Instance.AddScore(score);

        Runner.Despawn(currentItemObject.GetComponent<NetworkObject>());

        ClearItem();
        FindNearItemObject();
    }

    private void ClearItem()
    {
        currentItemObject = null;
        currentItem = null;
        currentItemStorage = null;
    }

    private void FindNearItemObject()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, searchRadius, layerMask);

        float minSqrDistance = float.MaxValue;
        GameObject nearestItem = null;

        foreach (Collider hit in hits)
        {
            float sqrDistance = (hit.transform.position - transform.position).sqrMagnitude;
            if (sqrDistance < minSqrDistance)
            {
                minSqrDistance = sqrDistance;
                nearestItem = hit.gameObject;
            }
        }
        if (nearestItem != null)
        {
            TrySetItem(nearestItem);
        }
    }

    ///// <summary>
    ///// 取得したアイテムを保持するためのメソッド
    ///// </summary>
    //private void GetItem(GameObject hitItem)
    //{
    //    isItemCollision = true;
    //    ItemName = currentItemObject.name;
    //    TrySetItem(hitItem);
    //}

    //private void ReleaseItem()
    //{
    //    isItemCollision = false;
    //    currentItemObject = null;
    //    ItemName = null;
    //}
}
