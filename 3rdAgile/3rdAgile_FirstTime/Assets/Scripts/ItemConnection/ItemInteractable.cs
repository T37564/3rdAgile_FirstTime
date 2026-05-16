using Network.Player;
using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Fusion;

public class ItemInteractable : NetworkBehaviour,IInteractable
{
    private int maxCarrierCount = 0;

    // アイテムを運ぶプレイヤーのリスト
    private List<PlayerController> carriers = 
        new List<PlayerController>();

    // IInteractableインターフェースの実装
    public Transform Transform => transform;

    private ItemDataStorage itemDataStorage;

    private void Awake()
    {
        Debug.Log("ItemInteractable Awake");
        itemDataStorage = GetComponent<ItemDataStorage>();
    }

    public bool CanInteract(PlayerController player)
    {
        if(itemDataStorage == null)
        {
            Debug.LogError("ItemDataStorageが見つかりませんでした。");
            return false;
        }

        // アイテムの必要人数を取得
        // 必要人数の情報が書かれていなかった場合の保険として1人に設定する（無くてもいい）
        maxCarrierCount = itemDataStorage.itemData.GetInt("RequiredPeople", 1);
        Debug.Log("必要人数: " + maxCarrierCount);

        if (carriers.Contains(player))
        {
            return false;
        }

        // 現在の運び手の数が必要人数以上であれば、これ以上運び手を追加できない
        if (carriers.Count >= maxCarrierCount)
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

        Debug.Log(player.name + " が持った");
    }
}
