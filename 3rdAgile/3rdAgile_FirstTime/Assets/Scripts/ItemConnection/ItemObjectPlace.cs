using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

/// <summary>
/// 生成したアイテムをランダム配置するクラス
/// </summary>
public class ItemObjectPlace: MonoBehaviour
{
    [Header("配置するアイテム")]
    [SerializeField] private GameObject itemObjectPrefab;

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

    [Header("アイテムを配置する際のレイヤーマスク")]
    [SerializeField] private LayerMask layerMask;

    private void Start()
    {
        for (int i = 0; i < maxItemObjectCount; i++)
        {
            Vector3 randomPosition = GetRandomPosition();
            Instantiate(itemObjectPrefab, randomPosition, Quaternion.identity);
        }
    }

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
}
