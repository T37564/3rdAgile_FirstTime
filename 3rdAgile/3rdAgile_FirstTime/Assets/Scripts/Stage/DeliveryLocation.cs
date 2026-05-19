//===========================================================================================
// 納品場所のクラス
// ScoreManagerに、納品されたアイテムのポイントを送る
// 製作者：スズキ
//===========================================================================================

using Network.Player;
using UnityEngine;

/// <summary>
/// 納品場所のクラス
/// 運ばれてきたアイテムの持つポイントを取得し、トータルポイントに加算する処理はここに実装
/// トータルのスコアを持つクラスは別クラスが理想か？
///  → イメージ）シングルトンクラス ScoreManagerに総ポイント用変数、加算メソッドを実装
///                 加算メソッドを呼び出し、StateAuthorityが判断し処理
/// </summary>
public class DeliveryLocation : MonoBehaviour
{
    [SerializeField] private float searchRadius = 3.0f;
    private LayerMask layerMask;

    // 範囲内に入ったアイテムを運んでいるプレイヤーの数
    private int itemDeliveryCount = 0;

    // アイテムの運ぶのに必要な人数
    private int NumberOfPeopleRequiredForDelivery = 0;

    private bool isItemCollision = false;
    // アイテムの名前
    private string ItemName = string.Empty;
    // アイテムのObject
    private GameObject item = null;

    private void Awake()
    {
        tag = TagName.DELIVERY_BOX;
    }

    private void Update()
    {
        // アイテムの名前を取る
        if (string.IsNullOrEmpty(ItemName)) return;

        // アイテムを運ぶのに必要な人数より範囲内に運んでいるプレイヤーが少ない場合は処理をしない
        if (itemDeliveryCount <= NumberOfPeopleRequiredForDelivery) return;

        // 以下で回収以降の処理を書く
        // 1.取ったアイテムを納品　アイテムのポイントをScoreManagerに送り更新をしてもらい、アイテムを削除
        ReleaseItem();

        // 納品したアイテムを削除するためアイテムの情報は明示的に消す
        Destroy(item);
        item = null;

        // 納品したらアイテムが範囲内に無いかを見る
        // あった場合そのアイテムの情報を取る
        FindNearItemObject();
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag(TagName.ITEM))
        {
            GetItem(collider.gameObject);
            // アイテムの必要人数を取る
            // colliderをアイテムのクラスにasキャストして必要人数を取るのが正解か？
        }

        if (collider.CompareTag(TagName.PLAYER))
        {
            if (collider.TryGetComponent(out PlayerController playerController))
            {
                if (playerController.IsHoldingItem)
                {
                    itemDeliveryCount++;
                }
            }
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.CompareTag(TagName.ITEM))
        {
            isItemCollision = false;
        }

        if (collider.CompareTag(TagName.PLAYER))
        {
            itemDeliveryCount--;
        }
    }

    private void FindNearItemObject()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, searchRadius, layerMask);

        float minSqrDistance = float.MaxValue;

        foreach (Collider hit in hits)
        {
            float sqrDistance = (hit.transform.position - transform.position).sqrMagnitude;
            if (sqrDistance < minSqrDistance)
            {
                minSqrDistance = sqrDistance;
            }
            GetItem(hit.gameObject);
        }
    }

    /// <summary>
    /// 取得したアイテムを保持するためのメソッド
    /// </summary>
    private void GetItem(GameObject hitItem)
    {
        isItemCollision = true;
        item = hitItem;
        ItemName = item.name;
    }

    private void ReleaseItem()
    {
        isItemCollision = false;
        item = null;
        ItemName = null;
    }
}
