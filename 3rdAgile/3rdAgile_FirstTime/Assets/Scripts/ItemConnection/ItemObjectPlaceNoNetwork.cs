using System;
using UnityEngine;

public enum ItemKind : int
{
    CopperCoin=0,
    SilverCoin,
}
public class ItemObjectPlaceNoNetwork : MonoBehaviour
{
    [Header("配置するアイテム")]
    [SerializeField] private GameObject CopperCoinPrefab;

    [SerializeField] private GameObject SilverCoinPrefab;

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

    //列挙型（enum）の変数の宣言
    private ItemKind itemKind;

    private void Start()
    {
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
        // 列挙型の値の要素数を取得
        int maxCount = Enum.GetNames(typeof(ItemKind)).Length;

        //列挙型の値をランダムに取得
        itemKind = (ItemKind)UnityEngine.Random.Range(0, maxCount);
        Debug.Log($"Generated ItemKind: {itemKind}"); // デバッグ用ログ

        GameObject spawnPrefab = null;

        //取得した値に応じて生成するアイテムを変える
        switch (itemKind)
        {
            case ItemKind.CopperCoin:
                spawnPrefab = CopperCoinPrefab;
                break;
            case ItemKind.SilverCoin:
                spawnPrefab = SilverCoinPrefab;
                break;
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
