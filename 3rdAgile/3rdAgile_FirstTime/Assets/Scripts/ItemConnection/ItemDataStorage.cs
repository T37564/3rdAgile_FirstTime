using Fusion;
using UnityEngine;

public enum RandomDataType
{
    mysteriousCoin,
    smallChest
}

/// <summary>
/// アイテムの情報が入っているscriptableobjectを格納するクラス
/// </summary>
public class ItemDataStorage : NetworkBehaviour
{
    [SerializeField] private SampleMasterData sampleMasterData;

    [Header("スポーン時に設定するデータをランダムに決めるかを判断するフラグ")]
    public bool useRandomData = false;

    public RandomDataType randomDataType;

    [Networked]
    public int dataId { get; set; }

    public SampleMasterData itemData => sampleMasterData;


    /// <summary>
    /// このメソッドが呼び出されたときアイテムの情報を格納しているscriptableobjectを変更するメソッド
    /// </summary>
    public void SetData(SampleMasterData newData)
    {
        sampleMasterData = newData;
        Debug.Log("最終設定 = " + itemData.name);
        ApplyData();
    }


    private void ApplyData()
    {
        //sampleMasterData = ItemDatabase.Instance.GetById(dataId);

        if (sampleMasterData == null)
        {
            Debug.LogError("SampleMasterDataが設定されていません");
            return;
        }
    }
}
