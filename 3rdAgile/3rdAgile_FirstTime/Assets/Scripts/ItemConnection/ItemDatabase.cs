using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance;

    [SerializeField] private SampleMasterData[] itemDatas;

    private Dictionary<int, SampleMasterData> dataMap;

    private void Awake()
    {
        Instance = this;

        dataMap = new Dictionary<int, SampleMasterData>();

        foreach (var data in itemDatas)
        {
            dataMap[data.id] = data;
        }
    }

    public SampleMasterData GetById(int id)
    {
        if (dataMap.TryGetValue(id, out var data))
            return data;

        Debug.LogError($"ID:{id} ‚Ìƒf[ƒ^‚ªŒ©‚Â‚©‚è‚Ü‚¹‚ñ");
        return null;
    }
}
