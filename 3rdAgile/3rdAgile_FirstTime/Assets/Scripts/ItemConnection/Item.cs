using UnityEngine;

/// <summary>
/// アイテムに持たせる情報を管理するクラス
/// </summary>
public class Item : MonoBehaviour
{
    [Header("アイテムのUI上で表示する名前を管理するScriptableObject")]
    [SerializeField] private ItemUINameData itemUINameData=null;

    [Header("伝説の宝箱　アイテム情報")]
    [SerializeField] private SampleMasterData sampleMasterData = null;

    public ItemUINameData ItemData => itemUINameData;

    public SampleMasterData SampleMasterData => sampleMasterData;
}
