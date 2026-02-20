using System;
using UnityEngine;

public class ItemObjectPlaceNoNetwork : MonoBehaviour
{
    [Header("配置するアイテム")]
    [Header("銅貨オブジェクト")]
    [SerializeField] private GameObject copperCoinPrefab;

    [Header("銀貨オブジェクト")]
    [SerializeField] private GameObject silverCoinPrefab;

    [Header("金貨オブジェクト")]
    [SerializeField] private GameObject goldCoinPrefab;

    [Header("謎のコインオブジェクト")]
    [SerializeField] private GameObject mysteriousCoinPrefab;

    [Header("高価な壺オブジェクト")]
    [SerializeField] private GameObject expensivePotPrefab;

    [Header("古代の壺オブジェクト")]
    [SerializeField] private GameObject ancientVasePrefab;

    [Header("銅貨の出現確立")]
    [SerializeField] private float copperCoinProbability = 0.0f;

    [Header("銀貨の出現確立")]
    [SerializeField] private float silverCoinProbability = 0.0f;

    [Header("金貨の出現確立")]
    [SerializeField] private float goldCoinProbability = 0.0f;

    [Header("謎のコインの出現確立")]
    [SerializeField] private float mysteriousCoinProbability = 0.0f;

    [Header("高価な壺の出現確立")]
    [SerializeField] private float expensivePotProbability = 0.0f;

    //確率の合計値
    private float totalProbability = 0.0f;

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
        //確率の合計値を設定
        totalProbability = 100.0f;

        for (int i = 0; i < maxItemObjectCount; i++)
        {
            // アイテムを生成して配置する
            SpawnItem();
        }
    }

    /// <summary>
    /// 座標をランダムに決めるメソッド
    /// Y座標は候補値からランダムに選ぶようにする
    /// </summary>
    private Vector3 GetRandomPosition()
    {
        // X軸とZ軸は指定された範囲内でランダムに決める
        float randomX = UnityEngine.Random.Range(minX, maxX);
        float randomZ = UnityEngine.Random.Range(minZ, maxZ);

        float randomY = yPositionCandidates[UnityEngine.Random.Range(0, yPositionCandidates.Length)];
        Debug.Log($"Random Y Position: {randomY}"); // デバッグ用ログ

        return new Vector3(randomX, randomY, randomZ);
    }

    /// <summary>
    /// イベント呼び出し時アイテムを生成するメソッド
    /// </summary>
    private void SpawnItem()
    {
        // 0.0から100.0の範囲でランダムな数値を生成
        float number = UnityEngine.Random.Range(0.0f, 100.0f);

        GameObject spawnPrefab = null;

        // ランダムに取得した値に基づいて生成するアイテムを決める
        if (number <= 30.0f)
        {
            Debug.Log("生成するアイテム: 銅貨");
            spawnPrefab = copperCoinPrefab;
        }
        else if (number <= 55.0f)
        {
            Debug.Log("生成するアイテム: 銀貨");
            spawnPrefab = silverCoinPrefab;
        }
        else if (number <= 75.0f)
        {
            Debug.Log("生成するアイテム: 金貨");
            spawnPrefab = goldCoinPrefab;
        }
        else if(number <= 90.0f)
        {
            Debug.Log("生成するアイテム: 謎のコイン");
            spawnPrefab = mysteriousCoinPrefab;
        }
        else if (number <= 95.0f)
        {
            Debug.Log("生成するアイテム: 高価な壺");
            spawnPrefab = expensivePotPrefab;
        }
        else
        {
            Debug.Log("生成するアイテム: 古代の壺");
            spawnPrefab = ancientVasePrefab;
        }

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
