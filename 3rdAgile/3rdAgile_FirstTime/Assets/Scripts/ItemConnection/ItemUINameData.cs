using UnityEngine;

[CreateAssetMenu(fileName = "ItemUINameData", menuName = "ScriptableObjects/ItemUINameData")]
/// <summary>
/// UI上で使用するアイテム名を管理するクラス
/// </summary>
public class ItemUINameData : ScriptableObject
{
    [Header("ゲーム内で表示するアイテム金額")]
    [SerializeField] private string itemPrice = "";

    [Header("ゲーム内で表示する運搬人数")]
    [SerializeField] private string  itemTransportCount= "";

    public string ItemPrice => itemPrice;
    public string ItemTransportCount => itemTransportCount;
}
