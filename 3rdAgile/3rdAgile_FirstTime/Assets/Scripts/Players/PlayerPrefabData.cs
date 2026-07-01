// -----------------------------------------------------------------------------------
// プレイヤーのPrefabデータをまとめて管理するScriptableObject
// ロビーとゲームシーンで共通して使用する
// PlayerPrefabData.cs
// Create.by TakahashiSaya
//-----------------------------------------------------------------------------------
using Fusion;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/PlayerPrefabData")]
public class PlayerPrefabData : ScriptableObject
{
    [Header("Player Prefab")]
    [Tooltip("プレイヤーPrefab配列")]
    public NetworkObject[] playerPrefabs;

    [Header("Spawn Settings")]
    [Tooltip("各プレイヤーの生成位置")]
    public Vector3[] playerSpawnPositions;

    [Tooltip("各プレイヤーの生成時の回転")]
    public Quaternion[] playerSpawnRotations;
}