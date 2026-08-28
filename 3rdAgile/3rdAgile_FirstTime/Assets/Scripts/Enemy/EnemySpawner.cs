using Fusion;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public class GenerateEnemyType
{
    // 生成する敵のPrefab
    public NetworkObject enemyPrefabObject;

    // 出現するステージの種類
    public StageTypeKinds stageTypes;
}

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] public GenerateEnemyType[] generateEnemys;

    [SerializeField] private int generateCount = 0;

    [SerializeField] private EnemyObjectPlace enemyObjectPlace;

    private NetworkRunner runner;

    private void Awake()
    {
        runner = FindAnyObjectByType<NetworkRunner>();

        if (runner == null)
        {
            Debug.LogError("NetworkRunnerが見つかりません");
            return;
        }

        //GenerateEnemy();
    }

    private void OnEnable()
    {
        StageSpawner.OnNavMeshGenerated += GenerateEnemy;
    }

    private void OnDisable()
    {
        StageSpawner.OnNavMeshGenerated -= GenerateEnemy;
    }



    private void GenerateEnemy()
    {
        // runnerが取得できていない場合
        if (runner == null)
        {
            Debug.LogError("NetworkRunnerが取得できていません");
            return;
        }

        // ホスト以外は生成しない
        if (!runner.IsServer)
        {
            return;
        }

        for (int i = 0; i < generateCount; i++)
        {
            NetworkObject enemyPrefab = generateEnemys[i].enemyPrefabObject;

            if (enemyPrefab == null)
            {
                Debug.LogError($"{i}番目の敵PrefabはNULLです");
                continue;
            }

            Vector3 spawnPosition = enemyObjectPlace.GetRandomPosition(generateEnemys[i].stageTypes);
            Debug.Log(
    $"【敵生成】" +
    $"Enemy={enemyPrefab.name}, " +
    $"StageType={generateEnemys[i].stageTypes}, " +
    $"Position={spawnPosition}"
);

            runner.Spawn(enemyPrefab, spawnPosition, Quaternion.identity);
        }
    }

}
