using System;
using UnityEngine;

public class ItemObjectPlaceNoNetwork : MonoBehaviour
{
    [System.Serializable]
    public class ItemProbability
    {
        [Header("アイテムのプレハブオブジェクト")]
        public GameObject itemPrefab; // アイテムのプレハブ

        [Header("アイテムの出現確率")]
        public float probability; // アイテムの出現確率
    }

    // アイテムとその確率の配列
    [Header("出現するアイテムのリスト")]
    [SerializeField] private ItemProbability[] itemProbabilities;

    [System.Serializable]
    public class RoomSpawnPosition
    {
        [Header("部屋の最小X座標")]
        public float minX; // 部屋の最小X座標

        [Header("部屋の最大X座標")]
        public float maxX; // 部屋の最大X座標

        [Header("部屋の最小Z座標")]
        public float minZ;

        [Header("部屋の最大Z座標")]
        public float maxZ;

        [Header("部屋のY座標")]
        public float positionY; // 部屋のY座標
    }

    [Header("部屋ごとのアイテム配置範囲のリスト")]
    [SerializeField] private RoomSpawnPosition[] roomSpawnPositions;


    [Header("配置するアイテムの最大値")]
    [SerializeField] private int maxItemObjectCount;

    [Header("アイテムを配置するx軸範囲（最小値）")]
    [SerializeField] private float minX = 0.0f;

    [Header("アイテムを配置するx軸範囲（最大値）")]
    [SerializeField] private float maxX = 0.0f;

    //マップの構造的に高低差があるのでY軸の範囲も設定する
    [Header("アイテムを配置するy軸の候補値")]
    [SerializeField] private float[] yPositionCandidates;

    [Header("アイテムを配置するz軸範囲（最小値）")]
    [SerializeField] private float minZ = 0.0f;

    [Header("アイテムを配置するz軸範囲（最大値）")]
    [SerializeField] private float maxZ = 0.0f;

    private void Start()
    {
        for (int i = 0; i < maxItemObjectCount; i++)
        {
            // アイテムを生成して配置する
            SpawnItem();
        }
    }

    /// <summary>
    /// どのアイテムを生成するかを確率に基づいてランダムに決めるメソッド
    /// </summary>
    /// <returns></returns>
    private GameObject GetRandomPrefabObject()
    {
        //合計確率の初期値
        float total = 0.0f;

        // ItemProbabilityごとのprobabilityの合計を計算
        //全部の確立を計算している
        foreach (var item in itemProbabilities)
        {
            total += item.probability;
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
        foreach (var item in itemProbabilities)
        {
            //現在のアイテムの確率を足していく
            current += item.probability;

            //取得したランダムな数値が現在のアイテムの出現確立の数値内にある場合
            //そのアイテムをGameObjectとして返す
            if (randomProbability < current)
            {
                return item.itemPrefab;
            }
        }

        return null;
    }

    private RoomSpawnPosition GetRandomRoom()
    {
        if(roomSpawnPositions==null|| roomSpawnPositions.Length == 0)
        {
            Debug.LogError("部屋のスポーン位置が設定されていません");
            return null;
        }

        int index=UnityEngine.Random.Range(0, roomSpawnPositions.Length);

        return roomSpawnPositions[index];
    }

    /// <summary>
    /// 座標をランダムに決めるメソッド
    /// Y座標は候補値からランダムに選ぶようにする
    /// </summary>
    private Vector3 GetRandomPosition()
    {
        //// X軸とZ軸は指定された範囲内でランダムに決める
        //float randomX = UnityEngine.Random.Range(minX, maxX);
        //float randomZ = UnityEngine.Random.Range(minZ, maxZ);

        ////配列内の候補からY座標の値をランダムに選ぶ
        //float randomY = yPositionCandidates[UnityEngine.Random.Range(0, yPositionCandidates.Length)];
        //Debug.Log($"Random Y Position: {randomY}"); // デバッグ用ログ

        RoomSpawnPosition roomSpawnPosition = GetRandomRoom();

        float randomX = UnityEngine.Random.Range(roomSpawnPosition.minX, roomSpawnPosition.maxX);
        float randomZ = UnityEngine.Random.Range(roomSpawnPosition.minZ, roomSpawnPosition.maxZ);

        float randomY = roomSpawnPosition.positionY;

        return new Vector3(randomX, randomY, randomZ);
    }

    /// <summary>
    /// イベント呼び出し時アイテムを生成するメソッド
    /// </summary>
    private void SpawnItem()
    {
        // 確率に基づいてランダムにアイテムのプレハブを選択
        GameObject spawnPrefab = GetRandomPrefabObject();

        if (spawnPrefab == null)
        {
            Debug.Log("生成Prefabが設定されていません");
            return;
        }

        // アイテムを生成してランダムに決めた座標に配置
        GameObject obj = Instantiate(spawnPrefab, GetRandomPosition(), Quaternion.identity);

        RegenerationCallOutNoNetwork callOut = obj.GetComponent<RegenerationCallOutNoNetwork>();

        if (callOut != null)
        {
            // イベント登録
            //イベントが呼び出されたときにHandleRegenerateメソッドが呼び出されるようにする
            callOut.OnNeedRegenerate += HandleRegenerate;
        }
    }

    /// <summary>
    /// 生成時にマップ上に生成されないアイテムを削除して
    /// 新しいアイテムを生成するメソッド
    /// </summary>
    private void HandleRegenerate(RegenerationCallOutNoNetwork item)
    {
        Debug.Log("再生成開始");

        // 古いアイテム削除
        Destroy(item.gameObject);

        // 新しく生成
        SpawnItem();
    }
}
