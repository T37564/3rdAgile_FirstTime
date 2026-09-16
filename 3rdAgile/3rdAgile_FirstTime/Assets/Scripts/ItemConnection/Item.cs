using UnityEngine;

/// <summary>
/// アイテムに持たせる情報を管理するクラス
/// </summary>
public class Item : MonoBehaviour
{
    [Header("アイテムのUI上で表示する名前を管理するScriptableObject")]
    [SerializeField] private ItemUINameData itemUINameData=null;

    public ItemUINameData ItemData => itemUINameData;
}
