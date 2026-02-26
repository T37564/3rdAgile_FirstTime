using UnityEngine;

/// <summary>
/// アイテムの情報が入っているscriptableobjectを格納するクラス
/// </summary>
public class ItemDataStorage : MonoBehaviour
{
    [SerializeField] private SampleMasterData sampleMasterData;

    [Header("スポーン時に設定するデータをランダムに決めるかを判断するフラグ")]
    public bool useRandomData = false;

    public SampleMasterData itemData => sampleMasterData;

    /// <summary>
    /// このメソッドが呼び出されたときアイテムの情報を格納しているscriptableobjectを変更するメソッド
    /// </summary>
    public void SetData(SampleMasterData newData)
    {
        sampleMasterData = newData;
        ApplyData();
    }

    private void ApplyData()
    {
        if (sampleMasterData == null)
        {
            Debug.LogError("SampleMasterDataが設定されていません");
            return;
        }
    }
}
