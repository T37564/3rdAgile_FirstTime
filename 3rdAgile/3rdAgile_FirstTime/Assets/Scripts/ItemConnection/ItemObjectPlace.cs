using Fusion;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemProbability
{
    [Header("アイテムのプレハブオブジェクト")]
    public NetworkObject itemPrefab; // アイテムのプレハブ

}

[Serializable]
public class PhaseItemTable
{
    [Header("Wave番号")]
    public GamePhase phase;

    [Header("このWaveで出現するアイテム")]
    public ItemProbability[] items;
}

//[Serializable]
//public class RandomItemData
//{
//    [Header("ランダム性のあるアイテムのデータ候補のリスト")]
//    public SampleMasterData[] sampleMasterData;
//}

[Serializable]
public class ItemDataTable
{
    [Header("この候補データを使うアイテム種類")]
    public RandomDataType randomDataType;

    [Header("個別のアイテムにあるランダム性のあるデータ")]
    public SampleMasterData[] sampleMasterDatas;
}

[System.Serializable]
public class RoomSpawnPosition
{
    [Header("アイテムを配置する部屋の座標範囲を設定する")]
    [Header("部屋の最小X座標")]
    public float minX;

    [Header("部屋の最大X座標")]
    public float maxX;

    [Header("部屋の最小Z座標")]
    public float minZ;

    [Header("部屋の最大Z座標")]
    public float maxZ;

    [Header("部屋のY座標")]
    public float positionY; // 部屋のY座標
}



public class ItemObjectPlace : MonoBehaviour
{
    // アイテムとその確率の配列
    [Header("出現するアイテムのリスト")]
    [SerializeField] public ItemProbability[] itemProbabilities;

    [Header("フェーズごとのアイテム")]
    [SerializeField] public PhaseItemTable[] phaseItemTables;

    [Header("部屋ごとのアイテム配置範囲のリスト")]
    [SerializeField] private RoomSpawnPosition[] roomSpawnPositions;

    [Header("配置するアイテムの最大値")]
    [SerializeField] public int maxItemObjectCount;

    //[Header("アイテムのデータが入っている配列")]
    //[SerializeField] public RandomItemData randomItemDatas;

    [Header("ランダム性のあるアイテムのデータテーブル")]
    [SerializeField] private ItemDataTable[] itemDataTable;

    [SerializeField] private GameTimer gameTimer = null;

    [SerializeField] public SampleMasterData[] itemDataArrays;

    private static ItemObjectPlace instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        if (gameTimer == null)
        {
            gameTimer = FindAnyObjectByType<GameTimer>();
            gameTimer.GetComponent<GameTimer>();
        }
    }


    public NetworkObject GetRandomPrefabByPhase(GamePhase phase)
    {
        var table = Array.Find(phaseItemTables, t => t.phase == phase);
        Debug.Log(phase);
        if (table == null)
        {
            Debug.LogError($"Wave {phase} に対応するアイテムテーブルが見つかりません");
            return null;
        }

        return GetRandomPrefabObject(table.items, phase);
    }


    /// <summary>
    /// どのアイテムを生成するかを確率に基づいてランダムに決めるメソッド
    /// </summary>
    /// <returns></returns>
    public NetworkObject GetRandomPrefabObject(ItemProbability[] items, GamePhase phase)
    {
        //合計確率の初期値
        float total = 0.0f;
        
        // ItemProbabilityごとのprobabilityの合計を計算
        //全部の確立を計算している
        foreach (var item in items)
        {
            var data = item.itemPrefab.GetComponent<ItemDataStorage>().itemData;

            total += GetProbability(data, phase);
        }

        //totalが0以下の場合確率に基づく計算ができないのでエラーを出力してnullを返す
        if (total <= 0)
        {
            Debug.LogError("確率の合計が0です");
            return null;
        }

        // 0.0から合計確率の範囲でランダムな数値を生成
        float randomProbability = UnityEngine.Random.Range(0f, total);

        // ランダムな数値がどのアイテムに属するかを決定
        float current = 0.0f;

        // アイテムの確率を順番に足していき、ランダムな数値がどのアイテムの範囲に入るかを確認
        foreach (var item in items)
        {
            // ItemDataStorageクラスを取得しitemDataも取得
            var data = item.itemPrefab.GetComponent<ItemDataStorage>().itemData;

            //現在フェーズのアイテムの確率を足していく
            current += GetProbability(data, phase);

            //取得したランダムな数値が現在のアイテムの出現確立の数値内にある場合
            //そのアイテムをGameObjectとして返す
            if (randomProbability < current)
            {
                //int itemIndex = UnityEngine.Random.Range(0, item.items.Length);
                return item.itemPrefab;
            }
        }

        return null;
    }

    /// <summary>
    /// フェーズごとの出現確立を取得するメソッド
    /// スクリプタブルオブジェクトに設定している出現確立数値を参照する
    /// </summary>
    private float GetProbability(SampleMasterData data, GamePhase phase)
    {
        return phase switch
        {
            // GetFloatの文字列型引数と同じ文字を参照
            GamePhase.Phase1 => data.GetFloat("Phase1Probability"),
            GamePhase.Phase2 => data.GetFloat("Phase2Probability"),
            GamePhase.Phase3 => data.GetFloat("Phase3Probability"),
            _ => 0f
        };
    }

    /// <summary>
    /// 設定した部屋のアイテム配置範囲リストをランダムに選ぶメソッド
    /// </summary>
    private RoomSpawnPosition GetRandomRoom()
    {
        // roomSpawnPositionsがnullまたは配列に設定していない場合
        if (roomSpawnPositions == null || roomSpawnPositions.Length == 0)
        {
            Debug.LogError("部屋のスポーン位置が設定されていません");
            return null;
        }

        //配列の中からランダムに1つ選ぶ
        int index = UnityEngine.Random.Range(0, roomSpawnPositions.Length);

        //RoomSpawnPositionの配列で選ばれたものを返り値にする
        return roomSpawnPositions[index];
    }

    /// <summary>
    /// 座標をランダムに決めるメソッド
    /// Y座標は候補値からランダムに選ぶようにする
    /// </summary>
    public Vector3 GetRandomPosition()
    {

        RoomSpawnPosition roomSpawnPosition = GetRandomRoom();

        // 部屋の座標内のランダムな座標を代入
        float randomX = UnityEngine.Random.Range(roomSpawnPosition.minX, roomSpawnPosition.maxX);
        float randomZ = UnityEngine.Random.Range(roomSpawnPosition.minZ, roomSpawnPosition.maxZ);

        //決められたY座標に配置する
        float randomY = roomSpawnPosition.positionY;

        // 決められた座標を返り値にする
        return new Vector3(randomX, randomY, randomZ);
    }

    /// <summary>
    /// アイテムの情報をランダムに決めるメソッド
    /// </summary>
    public SampleMasterData GetRomdomItemData(NetworkObject networkObject)
    {
        RandomDataType dataType = networkObject.GetComponent<ItemDataStorage>().randomDataType;

        var table = Array.Find(itemDataTable, t => t.randomDataType == dataType);
        //var table = Array.Find(itemDataTable, t => t.itemPrefab == networkObject);
        //var table = Array.Find(itemDataTable,t => t != null && t.itemPrefab != null &&
        //t.itemPrefab == networkObject);

        if (table == null)
        {
            Debug.Log("データをランダムに決める必要がないアイテム");
            return null;
        }

        if(table.sampleMasterDatas==null|| table.sampleMasterDatas.Length == 0)
        {
            Debug.Log("候補データがないためデータを取得しない");
            return null;
        }

        //if (itemDataArrays == null || itemDataArrays.Length == 0)
        //{
        //    Debug.LogError("アイテムデータ候補が設定されていません");
        //    return null;
        //}

        // itemDataArraysの配列の数からランダムに1つ選ぶ
        int itemDataIndex = UnityEngine.Random.Range(0, table.sampleMasterDatas.Length);

        // itemDataArraysの配列で選ばれたものを返り値にする
        //return itemDataArrays[itemDataIndex];

        return table.sampleMasterDatas[itemDataIndex];
    }

}