using UnityEngine;

public interface IInteractable
{
    /// <summary>
    /// オブジェクトの位置を取得するためのプロパティ
    /// </summary>
    Transform Transform { get; }

    /// <summary>
    /// アイテムとプレイヤーそれぞれで処理が違うため空実装
    /// </summary>
    void Interact();
}
