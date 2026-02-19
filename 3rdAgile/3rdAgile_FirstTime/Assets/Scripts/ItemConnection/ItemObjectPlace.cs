using Fusion;
using UnityEngine;

/// <summary>
/// 生成したアイテムをランダム配置するクラス
/// </summary>
public class ItemObjectPlace : NetworkBehaviour
{
    [Header("配置するアイテム")]
    [SerializeField] private NetworkObject itemObjectPrefab;

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

    private NetworkRunner networkRunner;

    public override void Spawned()
    {
        networkRunner = Runner;

        if (!networkRunner.IsServer)
        {
            // サーバーのみがアイテムを生成する
            // クライアントがこの処理をしないことでアイテムの重複生成を防ぐ
            return;
        }

        for (int i = 0; i < maxItemObjectCount; i++)
        {
            // アイテムを生成して配置する
            SpawnItem();
        }
    }

    //private void Start()
    //{
    //    networkRunner = FindAnyObjectByType<NetworkRunner>();

    //    for (int i = 0; i < maxItemObjectCount; i++)
    //    {
    //        // アイテムを生成して配置する
    //        SpawnItem();
    //    }
    //}


    /// <summary>
    /// 座標をランダムに決めるメソッド
    /// Y座標は候補値からランダムに選ぶようにする
    /// </summary>
    private Vector3 GetRandomPosition()
    {
        // X軸とZ軸は指定された範囲内でランダムに決める
        float randomX = Random.Range(minX, maxX);
        float randomZ = Random.Range(minZ, maxZ);

        float randomY = yPositionCandidates[Random.Range(0, yPositionCandidates.Length)];
        Debug.Log($"Random Y Position: {randomY}"); // デバッグ用ログ

        return new Vector3(randomX, randomY, randomZ);
    }

    /// <summary>
    /// イベント呼び出し時アイテムを生成するメソッド
    /// </summary>
    private void SpawnItem()
    {
        //if (!networkRunner.IsServer)
        //{
        //    // サーバーのみがアイテムを生成する
        //    // クライアントがこの処理をしないことでアイテムの重複生成を防ぐ
        //    return;
        //}

        // アイテムを生成してランダムに決めた座標に配置
        //GameObject obj = Instantiate(itemObjectPrefab, GetRandomPosition(), Quaternion.identity);
        NetworkObject obj= networkRunner.Spawn(itemObjectPrefab,GetRandomPosition(), Quaternion.identity);

        RegenerationCallOut callOut = obj.GetComponent<RegenerationCallOut>();

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
    void HandleRegenerate(RegenerationCallOut item)
    {
        Debug.Log("再生成開始");

        // 古いアイテム削除
        //Destroy(item.gameObject);
        networkRunner.Despawn(item.GetComponent<NetworkObject>());

        // 新しく生成
        SpawnItem();
    }
}
