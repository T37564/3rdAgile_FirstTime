using Network.Player;
using UnityEngine;

public interface IInteractable
{
    /// <summary>
    /// オブジェクトの位置を取得するためのプロパティ
    /// </summary>
    Transform Transform { get; }

    /// <summary>
    /// このオブジェクトに対してインタラクト可能か
    /// </summary>
    bool CanInteract(PlayerController player);

    /// <summary>
    /// アイテムとプレイヤーそれぞれで処理が違うため空実装
    /// </summary>
    void Interact(PlayerController player);
}
