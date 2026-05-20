// -----------------------------------------------------------------------------------
// プレイヤーのPrefabデータをまとめて管理するScriptableObject
// ロビー・ゲームシーン共通で使用
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

    [Tooltip("各プレイヤーの生成時回転")]
    public Quaternion[] playerSpawnRotations;
}