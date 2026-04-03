// -----------------------------------------------------------------------------------
// プレイヤーのPrefabデータをまとめて管理するScriptableObject
// ロビー・ゲームシーン共通で使用
// Create.by TakahashiSaya
//-----------------------------------------------------------------------------------
using Fusion;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/PlayerPrefabData")]
public class PlayerPrefabData : ScriptableObject
{
    [Header("Guest&Host")]
    [Tooltip("プレイヤー用のPrefab配列")]
    public NetworkObject[] playerPrefab;

    [Header("Spawn Settings")]
    [Tooltip("プレイヤー生成位置（全員共通）")]
    public Vector3[] prefabSpawnPosition;
}