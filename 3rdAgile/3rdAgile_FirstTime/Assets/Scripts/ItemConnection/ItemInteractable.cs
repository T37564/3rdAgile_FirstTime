using Network.Player;
using UnityEngine;
using System.Collections.Generic;
using Fusion;

public class ItemInteractable : NetworkBehaviour, IInteractable
{
    public int RequiredPeople { get; private set; }

    // アイテムを運ぶプレイヤーのリスト
    private List<PlayerController> carriers =
        new List<PlayerController>();

    // IInteractableインターフェースの実装
    public Transform Transform => transform;

    public ItemDataStorage itemDataStorage;

    [SerializeField] private float followSpeed = 8.0f;
    [SerializeField] private Vector3 carryOffset = Vector3.zero;

    private bool isCarrying = false;

    // 運搬中で人手が欲しいことを知らせるフラグ
    [Networked] public NetworkBool IsHelpPeople { get; set; }

    public override void Spawned()
    {
        //Debug.Log("ItemInteractable Spawned");
        itemDataStorage = GetComponent<ItemDataStorage>();

        // アイテムの必要人数を取得
        // 必要人数の情報が書かれていなかった場合の保険として1人に設定する（無くてもいい）
        RequiredPeople = itemDataStorage.itemData.GetInt("RequiredPeople", 1);
    }

    public bool CanInteract(PlayerController player)
    {
        if (itemDataStorage == null)
        {
            Debug.LogError("ItemDataStorageが見つかりませんでした。");
            return false;
        }

        if (carriers.Contains(player))
        {
            return false;
        }

        // 現在の運び手の数が必要人数以上であれば、これ以上運び手を追加できない
        if (carriers.Count >= RequiredPeople)
        {
            return false;
        }

        return true;
    }

    public void Interact(PlayerController player)
    {
        if (!CanInteract(player))
            return;

        // プレイヤーを運び手リストに追加
        carriers.Add(player);

        player.SetHoldingItem(this);
        Debug.Log(player.name + " が持った");

        if (CanCarry())
        {
            StartCarry();
            IsHelpPeople = false;
        }
        else
        {
            // 人数が不足しているとき
            IsHelpPeople = true;
        }
    }

    /// <summary>
    /// 運搬可能かの判定
    /// </summary>
    /// <returns></returns>
    private bool CanCarry()
    {
        return carriers.Count == RequiredPeople;
    }

    private void StartCarry()
    {
        isCarrying = true;

        Vector3 center = GetCarriersCenter();
        carryOffset = transform.position - center;

        Debug.Log("アイテムを運び始める");
    }

    private Vector3 GetCarriersCenter()
    {
        Vector3 center = Vector3.zero;
        foreach (var carrier in carriers)
        {
            center += carrier.Transform.position;
        }
        center /= carriers.Count;

        return center;
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        if (!isCarrying) return;

        // 運搬中の処理
        FollowCarries();
    }

    private void FollowCarries()
    {
        if (carriers.Count == 0) return;

        Vector3 center = Vector3.zero;

        foreach (var carrier in carriers)
        {
            center += carrier.Transform.position;
        }

        center /= carriers.Count;

        Vector3 targetPosition = center + carryOffset;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            followSpeed * Runner.DeltaTime
        );
    }

    public void Release(PlayerController player)
    {
        if (!carriers.Contains(player)) return;

        carriers.Remove(player);
        player.ClearHoldingItem(this);

        if (carriers.Count < RequiredPeople)
        {
            isCarrying = false;
            Debug.Log("人数不足でアイテムの運搬を中止");
        }
    }

    /// <summary>
    /// このアイテムを運んでいるプレイヤーを全員解除する処理
    /// </summary>
    public void ReleaseAll()
    {
        //　アイテムを持っていない状態に戻す処理
        foreach (var carrier in carriers)
        {
            if (carrier != null)
            {
                carrier.ClearHoldingItem(this);
            }
        }

        // 運び手リストを空にする
        carriers.Clear();

        // 運搬状態を終了する
        isCarrying = false;
    }
}
