using UnityEngine;

public class ItemDataStorage : MonoBehaviour
{
    [SerializeField] private SampleMasterData sampleMasterData;

    [Header("スポーン時に設定するデータをランダムに決めるかを判断するフラグ")]
    public bool useRandomData = false;

    public SampleMasterData itemData => sampleMasterData;

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
